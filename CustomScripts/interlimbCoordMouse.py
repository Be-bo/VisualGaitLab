# -*- coding: utf-8 -*-
"""
Created on Wed May  4 12:37:11 2022

@author: Zahra Ghavasieh
@original MATLAB script author: Linda Kim

To Run:
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordA.py', args='test test_data/test1/HindLeftInStance.txt test_data/test1/FrontLeftInStance.txt test_data/test1/FrontRightInStance.txt test_data/test1/HindRightInStance.txt')
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordA.py', args='test test_data/HindLeftInStance.txt test_data/FrontLeftInStance.txt test_data/FrontRightInStance.txt test_data/HindRightInStance.txt')
    - python [scriptname].py > output.txt
    - What is the animal ID? (Eg. 6-OHDAM#Pre or SalineM#Post etc.)

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
    circ_plots 
    )



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
    
    # Plot Circular Stats
    phaseval_rad = circStats(onsets)
    circ_plots(phaseval_rad, animalID, outDir)
    
    # Save Phaseval for Part B
    out_file = outDir + animalID + "-phaseval.csv"
    df = pd.DataFrame(phaseval_rad, index=coupling_labels)
    df.to_csv(out_file, index=True, header=False)




if __name__ == "__main__":
    main()