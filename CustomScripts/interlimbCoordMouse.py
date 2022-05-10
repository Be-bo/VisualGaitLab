# -*- coding: utf-8 -*-
"""
Created on Wed May  4 12:37:11 2022

@author: Zahra Ghavasieh
@original MATLAB script author: Linda Kim

Interlimb Coordination part A but fusing multiple clips instead

INPUT:
    - output folder
    - animal id
    - limb order (FL, FR, HL, HR) -> (hindIpsi, foreIpsi, ForeContra, HindContra)
        - eg: HL,FL,FR,HR -> '2013'
    - list of analyzed videos' paths

To Run:
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordMouse.py', args='out_mouse test_mouse 2013 "test_data/test1" "test_data/test2" "test_data/test3"')
    
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordA.py', args='test test_data/HindLeftInStance.txt test_data/FrontLeftInStance.txt test_data/FrontRightInStance.txt test_data/HindRightInStance.txt')


Limb Order:
    0. HindIpsi
    1. ForeIpsi
    2. ForeContra
    3. HindContra
"""


import sys
import os
from interlimbCoordA import (
    drawFootfallPattern, 
    getFootfallOnset, 
    regIndex, 
    circStats, 
    circ_plots,
    save_phaseval
    )


coupling_labels = ['Forelimbs','Hindlimbs','Ipsilesionals','Contralesionals',
                       'ContraFore-IpsiHind','IpsiFore-ContraHind']

inStance_fileNames = ['FrontLeftInStance.txt', 'FrontRightInStance.txt', 
                      'HindLeftInStance.txt', 'HindRightInStance.txt']


# Read text files into list of integers
# clippath/gaitsavedstate/inStance_fileNames[limbOrder]
def processInput(clipPaths, limbOrder):
    
    contents = []
    
    # Each analyzed video
    for clipPath in clipPaths:
        
        path = clipPath + '/gaitsavedstate/'
        
        if not os.path.exists(path):
            print('ERROR: Video not analyzed:', clipPath)
            continue
        
        # Read inStance.txt files in order
        clip_contents = []
        for order in limbOrder:
            try:
                file = open(path + inStance_fileNames[int(order)], 'r')
                clip_contents.append([int(x) for x in file.readlines()])
                file.close()
            except:
                print('ERROR: File "' + path + inStance_fileNames[int(order)] + '" does not exist')
        
        contents.append(clip_contents)
    
    return contents



# MAIN
def main():
    
    # Input length
    if len(sys.argv) < 5:
        print('ERROR: Not enough arguments!', len(sys.argv))
        return -1
    
    outDir = sys.argv[1] + '/'
    if not os.path.exists(outDir):
       os.makedirs(outDir)
    
    # Process Input
    animalID = sys.argv[2]
    limbOrder = sys.argv[3]
    clips = sys.argv[4:]
    inStanceValues = processInput(clips, limbOrder)
    
    
    # Calculate footfall onsets per clip
    phaseval_rads = []
    for i in range(len(clips)):
        
        # clip directory
        clip_name = clips[i].split('/')[-1]
        clip_outDir = outDir + clip_name + '/'
        if not os.path.exists(clip_outDir):
            os.makedirs(clip_outDir)
    
        print("Processing Clip:", clip_name, "...")
    
        # Figure 1
        drawFootfallPattern(inStanceValues[i], animalID, clip_outDir)
        
        # Figure 2
        footfallOnsets, clip_onsets = getFootfallOnset(inStanceValues[i], animalID, clip_outDir)
        
        # Regularity Index (starting limb)
        regIndex(footfallOnsets[1], clip_outDir + animalID) 
        
        # CircStats
        phaseval = circStats(clip_onsets)    # Calculate circular stat per clip
        circ_plots(phaseval, animalID, clip_outDir)
        save_phaseval(clip_outDir, animalID, phaseval)
        phaseval_rads.append(phaseval)
        
        
    # Accumulate phasevals for each paw pair
    phaseval_rad = phaseval_rads[0]
    for rads in range(1, len(phaseval_rads)):
        for pair in range(len(phaseval_rads[rads])):
            phaseval_rad[pair].extend(phaseval_rads[rads][pair])
    
    # Plot Phaseval in radians on a circualr graph
    circ_plots(phaseval_rad, animalID, outDir)
    save_phaseval(outDir, animalID, phaseval_rad)




if __name__ == "__main__":
    main()