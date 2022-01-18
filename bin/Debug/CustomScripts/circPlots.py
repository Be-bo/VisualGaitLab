# -*- coding: utf-8 -*-
"""
Created on Fri Dec 31 14:17:30 2021

@author: Zahra Ghavasieh
@original MATLAB script author: Linda Kim

To be used by the InterlimbCoord_script.py
"""

# Import modules
import matplotlib.pyplot as plt
import numpy as np



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
def regIndex(footfallOnsets):
    
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
    regularityindex = (sum(stepseq_count)*4/len(steppattern)) * 100
    stepseq_percent = [i/sum(stepseq_count) * 100 for i in stepseq_count]
    
    # Print results
    print('\n','Regularity Index =', regularityindex)
    print('Stepsequence Percentage =', stepseq_percent)




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




# Windrose Plot
# Theta is a list of angles (in radians) that determines the angle of each bin from the origin.
# The length of each bin reflects the number of elements in theta that fall within a group, 
# which ranges from 0 to the greatest number of elements deposited in any one bin.
def windRosePlot(theta, binNum,title='', scatter=False):
    
    # Calculate items in each bin
    width = np.pi * 2 / binNum
    bins = []
    for i in range(binNum):
        bins.append([t for t in theta if i*width <= t < (i+1)*width])

    # Compute pie slices
    radii = [len(b)/len(theta) for b in bins if len(b)!=0]
    bars = [(i*width) + width/2 for i in range(binNum) if len(bins[i])!=0]
    tick_step = max(radii) / 4
    max_bar = max(radii) + tick_step
    
    # Draw Plot
    ax = plt.subplot(111, projection='polar')
    ax.bar(bars, radii, width=width, bottom=0, color=(0,0,0,0), edgecolor='blue')

    # Mean direction
    ax.vlines(sum(theta)/len(theta), 0, max_bar-(tick_step/2), color='red', zorder=3)
    
    # Scatterplot on top of polar plot
    if scatter:
        ax.scatter(theta, [max_bar for _ in range(len(theta))], color=(0,0,0,0), edgecolor='red')
        ax.set_xticks(np.arange(0, 2.0*np.pi, np.pi/2))
        ax.set_xticklabels(['0', 'π/2', '±π', '-π/2'])
        
    else:
        ax.set_xticks(np.arange(0, 2.0*np.pi, np.pi/6))
        ax.set_rmax(max_bar)
        
    # Ticks
    ax.set_rticks(np.around(np.arange(tick_step,max_bar,tick_step),1)) # only 4 ticks

    # Save and Show Plot
    ax.set_title(title)
    plt.savefig('out/'+title+".png", bbox_inches='tight')
    plt.show()
    
    return

