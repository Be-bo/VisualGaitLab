# -*- coding: utf-8 -*-
"""
Created on Wed Dec 15 14:55:02 2021

@author: Zahra Ghavasieh
@original MATLAB script author: Linda Kim

Interlimb Coordination - Group (previously called "Part B")

To Run:
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordGroup.py', wdir='C:/Aaallmine/git_repos/vgl/CustomScripts', args='outB out_mouse/test1/test_mouse-phaseval.csv out_mouse/test2/test_mouse-phaseval.csv out_mouse/test3/test_mouse-phaseval.csv')
    - python [scriptname].py > output.txt

Limb Order:
    0. HindIpsi
    1. ForeIpsi
    2. ForeContra
    3. HindContra
    
References:
    - https://ocefpaf.github.io/python4oceanographers/blog/2015/02/09/compass/
"""


# imports
import sys
import os
import numpy as np
import pandas as pd
import math
from dependencies.compassPlot import northCompass


# Global Values
paw_labels = ['HindIpsi', 'ForeIpsi', 'ForeContra', 'HindContra']
coupling_labels = ['Forelimbs','Hindlimbs','Ipsilesionals','Contralesionals',
                       'ContraFore-IpsiHind','IpsiFore-ContraHind']



# 1. Read text files into list of integers
def processInput(fileNames):
    
    contents = []
    
    for fileName in fileNames:
        try:
            df = pd.read_csv(fileName, index_col=0, header=None)
            contents.append(df)
            
        except IOError:
            print('ERROR: File "' + fileName + '" does not exist')
    
    return contents




# Average Calculator
def calcCoordAverage(lis):
    
    # Filter NANs
    trueList = [x for x in lis if not math.isnan(x)]
    
    x = sum([math.cos(x) for x in trueList]) / len(trueList)
    y = sum([math.sin(x) for x in trueList]) / len(trueList)

    return (x, y)



# 2. Calculate Averages per limb pair
def calcAverages(outDir, phasevals):
    
    #averages = pd.Dataframe(columns=coupling_labels)
    averages = dict(zip(coupling_labels, [[] for _ in coupling_labels]))
    
    for fileDF in phasevals:
        for limbPair in fileDF.index:
            averages[limbPair].append(calcCoordAverage(fileDF.loc[limbPair, :]))
    
    
    return averages



# Write Averages to a csv file
def averageToCsv(outDir, fileNames, averages):
    
    # Flatten Averages Dictionary of list of Tuples
    flattened_ave = []
    columns = []
    for key in averages.keys():
        columns.append(key)
        flattened_ave.append(list(sum(averages[key], ())))

    # Create DataFrame and write to csv
    index = pd.MultiIndex.from_product([fileNames, ['X','Y']], names=["File", "Coordinate"])
    pd.DataFrame(
        np.array(flattened_ave).T.tolist(), 
        index=index, 
        columns=columns
        ).to_csv(outDir + "averages.csv")
    


# MAIN
def main():
    
    # Input length
    if len(sys.argv) < 2:
        print('ERROR: Not enough arguments!')
        return -1
    
    # Process Input
    outDir = "out2/"
    i = 1
    
    if len(sys.argv) > 2:
       outDir = sys.argv[1] + '/'
       i = 2
   
    if not os.path.exists(outDir):
       os.makedirs(outDir)
       
    phasevals = processInput(sys.argv[i:])
    

    # Calculate averages per limb pair
    averages = calcAverages(outDir, phasevals)
    averageToCsv(outDir, sys.argv[i:], averages)
    
    # Get Mouse IDs
    leg = []
    for file in sys.argv[i:]:
        csv_file = file.split('/')[-1]
        animal_id = csv_file.replace('-phaseval.csv', '')
        original_id = animal_id
        j = 1
        while animal_id in leg:
            animal_id = original_id + str(j)
            j += 1
        leg.append(animal_id)
    
    # Draw Compass Graphs
    for key in averages:
        xs, ys = zip(*averages[key])
        northCompass(xs, ys, outDir=outDir, title=key, leg=leg) #TODO add legend
    


if __name__ == "__main__":
    main()