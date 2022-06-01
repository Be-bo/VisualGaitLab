# -*- coding: utf-8 -*-
"""
Created on Wed Dec 15 14:55:02 2021

@author: Zahra Ghavasieh
@original MATLAB script author: Linda Kim

Interlimb Coordination Part A - Single Clip

To Run:
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordA.py', args='test test_data/test2/HindLeftInStance.txt test_data/test2/FrontLeftInStance.txt test_data/test2/FrontRightInStance.txt test_data/test2/HindRightInStance.txt')
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordA.py', args='test test_data/HindLeftInStance.txt test_data/FrontLeftInStance.txt test_data/FrontRightInStance.txt test_data/HindRightInStance.txt')
    - python [scriptname].py > output.txt
    - What is the animal ID? (Eg. 6-OHDAM#Pre or SalineM#Post etc.)

Limb Order:
    0. HindIpsi
    1. ForeIpsi
    2. ForeContra
    3. HindContra
"""


# imports
import sys
import os
import csv
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from dependencies.rosePlot import windRosePlot


# Global Values
paw_labels = ['HindIpsi', 'ForeIpsi', 'ForeContra', 'HindContra']
coupling_labels = ['Forelimbs','Hindlimbs','Ipsilesionals','Contralesionals',
                       'ContraFore-IpsiHind','IpsiFore-ContraHind']



# Read text files into list of integers
def processInput(fileNames):
    
    contents = []
    
    for fileName in fileNames:
        try:
            file = open(fileName, 'r')
            contents.append([int(x) for x in file.readlines()])
            file.close()
        except:
            print('ERROR: File "' + fileName + '" does not exist')
    
    return contents



def flushPlot():
    plt.draw()
    plt.clf()
    plt.cla()
    plt.close()


# Fig1: Draw the footfall pattern of all four limbs
def drawFootfallPattern(inStanceValues, animalID, outDir):
    
    fig, axs = plt.subplots(len(inStanceValues),sharex=True)
    fig.suptitle('Figure 1: Footfall Pattern Bar')
    
    for i in range(len(inStanceValues)):
        
        # Construct bars [(start, len)]
        bars = []
        inStance = False
        start, width = (0,0)
        for frame in range(len(inStanceValues[i])):
            # Start of Stance
            if (not inStance) and inStanceValues[i][frame] == 1:
                start = frame
                inStance = True
            # End of Stance
            elif inStance and inStanceValues[i][frame] == 0:
                width = frame - start
                bars.append((start,width))
                inStance = False
        # Finished in Stance
        if inStance: 
            width = frame - start
            bars.append((start,width))
                
        # Draw plot
        axs[i].broken_barh(bars, (0, 1), facecolors='tab:blue')
        axs[i].set_yticks([0.5])
        axs[i].set_yticklabels([paw_labels[i]])

    plt.savefig(outDir+animalID+"-footfallpatternbar-fig1.png", bbox_inches='tight')
    flushPlot()




# Fig2: Calculate and Graph Footfall by onset
def getFootfallOnset(inStanceValues, animalID, outDir):
    
    colors = ['yellow', 'green', 'red', 'blue']
    pawNums = len(inStanceValues)
    
    onsets = [] 
    for paw in range(pawNums):
        
        # Calculate onsets per paw
        diffs = np.diff(inStanceValues[paw])
        onset = [i+2 for i in range(len(diffs)) if diffs[i] == 1]
        onsets.append(onset)
        
        # Plot diffs
        y = [pawNums - paw for _ in range(len(onset))]
        plt.plot(onset, y, '.', color=colors[paw])
    
    # Build List of Footfall Onsets
    lis = sorted([(j, pawNums-i) for i in range(len(onsets)) for j in onsets[i]])
    points = list(zip(*lis))
    footfallOnsets = [list(points[i]) for i in range(len(points))]
    
    # Complete Graph
    plt.plot(footfallOnsets[0], footfallOnsets[1], '-', color='black', linewidth=1)
    plt.yticks([pawNums-i for i in range(pawNums)], labels=paw_labels)
    plt.title('Figure 2: Footfall Pattern Onsets')
    plt.savefig(outDir+animalID+"-footfallpatternline-fig2.png", bbox_inches='tight')
    flushPlot()
    
    return (footfallOnsets, onsets)




# Regularity Index Calculation

''' 
Linda's Comments:

regularityindex = (number of normal step sequence patterns x 4/total number of pawplacements) x 100%
 - normal step patterns is the total amount defined AA, AB,CA,CB, RA, RB 
   gait patterns were use and paw placement is the total paw placement patterns
 - recorded from Vrinten and Hamers, 2003 https://doi.org/10.1016/s0304-3959(02)00382-2

Ca: RF-LF-RH-LH     Aa: RF-RH-LF-LH     Ra: RF-LF-LH-RH
Cb: LF-RF-LH-RH     Ab: LF-RH-RF-LH     Rb: LF-RF-RH-LH

 - eg.(AA) RF-RH-LF-LH, (AB) LF-RH-RF-LH, (CA) RF-LF-RH-LH and (CB) LF-RF-LH-RH
 - (AA) IF-IH-CF-CH, (AB) CF-IH-IF-CH, (CA) IF-CF-IH-CH and 
   (CB) CF-IF-CH-IH (RA) IF-CF-CH-IH, (RB) CF-IF-IH-CH

(AA) 2-1-3-4-2-1-3-4, (AB) 3-1-2-4-3-1-2-4, (CA) 3-2-4-1-3-2-4-1,
(CB) 2-3-4-1-2-3-4-1, (RA) 3-2-1-4-3-2-1-4, (RB) 2-3-4-1-2-3-4-1

CH start(AA)1342 (AB)1243 (CA)1324 (CB)1423 (RA)1432 (RB)1234
CF start(AA)2134 (AB)2431 (CA)2413 (CB)2314 (RA)2143 (RB)2341
IF start(AA)3421 (AB)3124 (CA)3241 (CB)3142 (RA)3214 (RB)3412
IH start(AA)4213 (AB)4312 (CA)4132 (CB)4231 (RA)4321 (RB)4123
'''
# footfallOnsets = paw number (1-4)
def regIndex(footfallOnsets, prefix):
    
    steppattern = ''.join([str(i) for i in footfallOnsets])
    
    # Step Preset Sequences
    stepseq = [
        [1342,1243,1324,1423,1432,1234], # CH start(AA)1342 (AB)1243 (CA)1324 (CB)1423
        [2134,2431,2413,2314,2143,2341], # CF start(AA)2134 (AB)2431 (CA)2413 (CB)2314 
        [3421,3124,3241,3142,3214,3412], # IF start(AA)3421 (AB)3124 (CA)3241 (CB)3142 
        [4213,4312,4132,4231,4321,4123]  # IH start(AA)4213 (AB)4312 (CA)4132 (CB)4231
    ] 
    
    # Check which category the onsets fall in
    stepseq_count = [0 for _ in range(len(stepseq[0]))]
    firstLimb = footfallOnsets[0]
    
    for i in range(len(stepseq[0])):        # (AA, AB, CA, CB, RA, RB)
        seq = str(stepseq[firstLimb-1][i])  # stringify each element corresponding to 1st limb
        stepseq_count[i] = steppattern.count(seq)   # count occurences of sequence
            
        
    # Regularity Index
    print(stepseq_count)
    regularityindex = (sum(stepseq_count)*4/len(steppattern)) * 100
    stepseq_percent = stepseq_count
    if (sum(stepseq_count) != 0):
        stepseq_percent = [i/sum(stepseq_count) * 100 for i in stepseq_count]
    
    # Save results in a csv
    out_file = prefix + "-regIndex.csv"
    df = pd.DataFrame(stepseq_percent, index=stepseq[firstLimb-1], columns=['StepSequence Percentage'])
    df.to_csv(out_file, index=True, header=True)
    
    with open(out_file, 'a') as fd:
        writer = csv.writer(fd)
        writer.writerow([])
        writer.writerow(['Regularity Index', regularityindex])

    




# Calculate circular stats
def calculateCircStat(onsets1, onsets2):
        
    totalLen = min(len(onsets1), len(onsets2))  
    phaseval_deg = [0 for _ in range(totalLen-1)]
    
    for total in range(totalLen-1):
        cyclePeriod = onsets2[total+1] - onsets2[total]
        phaseval = (onsets1[total] - onsets2[total]) / cyclePeriod
        
        # Convert Phaseval to degrees
        phaseval_deg[total] = phaseval * 360
        if phaseval < 0:
            phaseval_deg[total] = phaseval_deg[total] + 360

    
    # Convert to radians
    return [deg*(np.pi/180) for deg in phaseval_deg]





# Circular Stats 
# (Forelimbs, Hindlimbs, Ipsilesionals, Contralesionals and Diagonals)
# onsets = ['HindIpsi', 'ForeIpsi', 'ForeContra', 'HindContra']
def circStats(onsets):
    
    phaseval_rad = []
    phaseval_rad.append(calculateCircStat(onsets[2], onsets[1])) # ForeContra, ForeIpsi
    phaseval_rad.append(calculateCircStat(onsets[3], onsets[0])) # HindContra, HindIpsi
    phaseval_rad.append(calculateCircStat(onsets[1], onsets[0])) # ForeIpsi, HindIpsi
    phaseval_rad.append(calculateCircStat(onsets[2], onsets[3])) # ForeContra, HindContra
    phaseval_rad.append(calculateCircStat(onsets[2], onsets[0])) # ForeContra, HindIpsi
    phaseval_rad.append(calculateCircStat(onsets[1], onsets[3])) # ForeIpsi, HindContra    

    return phaseval_rad




# Circular Plots
def circ_plots(phaseval_rad, animalID, outDir):
    
    for i in range(len(coupling_labels)):
        
        # Titles
        hist_title = animalID + '-histogram-' + coupling_labels[i]
        dens_title = animalID + '-densityplot-' + coupling_labels[i]
        
        # Plots
        windRosePlot(phaseval_rad[i], 20, outDir, hist_title, False)
        windRosePlot(phaseval_rad[i], 20, outDir, dens_title, True)
  
    
  
# Save PhaseVal
def save_phaseval(outDir, animalID, phaseval_rad):
    # Save Phaseval for Part B
    out_file = outDir + animalID + "-phaseval.csv"
    df = pd.DataFrame(phaseval_rad, index=coupling_labels)
    df.to_csv(out_file, index=True, header=False)
  
  

# MAIN
def main():
    
    # Input length
    if len(sys.argv) < 6:
        print('ERROR: Not enough arguments!')
        return -1
    
    # Process Input
    animalID = sys.argv[1]
    inStanceValues = processInput(sys.argv[2:6])
    
    outDir = "out/"
    if len(sys.argv) > 6:
        outDir = sys.argv[-1] + '/'
        
    if not os.path.exists(outDir):
       os.makedirs(outDir)
    
    # Figure 1
    drawFootfallPattern(inStanceValues, animalID, outDir)
    
    # Figure 2
    footfallOnsets, onsets = getFootfallOnset(inStanceValues, animalID, outDir)
    
    # Regularity Index (starting limb)
    regIndex(footfallOnsets[1], outDir + animalID) 
    
    # Plot Circular Stats and Save Phaseval for Part B
    phaseval_rad = circStats(onsets)
    circ_plots(phaseval_rad, animalID, outDir)
    save_phaseval(outDir, animalID, phaseval_rad)
    


if __name__ == "__main__":
    main()