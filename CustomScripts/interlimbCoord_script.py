# -*- coding: utf-8 -*-
"""
Created on Wed Dec 15 14:55:02 2021

@author: Zahra Ghavasieh
@original MATLAB script author: Linda Kim

To Run:
    - runfile('C:/Users/Judgy/OneDrive/Documents/WorkStuff/interlimbCoord_script.py', args='test_data/HindLeftInStance.txt test_data/FrontLeftInStance.txt test_data/FrontRightInStance.txt test_data/HindRightInStance.txt')

Limb Order:
    0. HindIpsi
    1. ForeIpsi
    2. ForeContra
    3. HindContra
"""


# imports
import sys
import matplotlib.pyplot as plt
import numpy as np
from circPlots import regIndex, calculateCircStat, windRosePlot


# Global Values
paw_labels = ['HindIpsi', 'ForeIpsi', 'ForeContra', 'HindContra']




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




# Fig1: Draw the footfall pattern of all four limbs
def drawFootfallPattern(inStanceValues, animalID):
    
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
        axs[i].set_yticks([0.5], labels=[paw_labels[i]])

    plt.savefig("out/"+animalID+"-footfallpatternbar-fig1.png", bbox_inches='tight')
    plt.show()




# Fig2: Calculate and Graph Footfall by onset
def getFootfallOnset(inStanceValues, animalID):
    
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
    plt.savefig("out/"+animalID+"-footfallpatternline-fig2.png", bbox_inches='tight')
    plt.show()
    
    return (footfallOnsets, onsets)





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
    
    print('\n','Phaseval in radians = \n', phaseval_rad,'\n')
    return phaseval_rad




# Circular Plots
def circ_plots(phaseval_rad, animalID):
    
    # Plot Labels
    coupling_labels = ['Forelimbs','Hindlimbs','Ipsilesionals','Contralesionals',
                       'ContraFore-IpsiHind','IpsiFore-ContraHind']
    
    for i in range(len(coupling_labels)):
        
        # Titles
        hist_title = animalID + '-histogram-' + coupling_labels[i]
        dens_title = animalID + '-densityplot-' + coupling_labels[i]
        
        # Plots
        windRosePlot(phaseval_rad[i], 20, hist_title, False)
        windRosePlot(phaseval_rad[i], 20, dens_title, True)
  
    
  

# MAIN
def main():
    
    # Input length
    if len(sys.argv) < 5:
        print('ERROR: Not enough arguments!')
        return -1
    
    # Process Input
    inStanceValues = processInput(sys.argv[1:])
    if len(inStanceValues) < 4:
        return -1
    
    # Figure 1
    animalID = input("What is the animal ID? (Eg. 6-OHDAM#Pre or SalineM#Post etc: ")
    drawFootfallPattern(inStanceValues, animalID)
    
    # Figure 2
    footfallOnsets, onsets = getFootfallOnset(inStanceValues, animalID)
    
    # Regularity Index
    regIndex(footfallOnsets[1])
    
    # Plot Circular Stats
    phaseval_rad = circStats(onsets)
    circ_plots(phaseval_rad, animalID)
    
    


if __name__ == "__main__":
    main()