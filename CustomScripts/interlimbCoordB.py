# -*- coding: utf-8 -*-
"""
Created on Wed Dec 15 14:55:02 2021

@author: Zahra Ghavasieh

Interlimb Coordination Part B - Combines multiple part A results to achieve results of interlimbCoordMouse.py

To Run:
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordB.py', wdir='C:/Aaallmine/git_repos/vgl/CustomScripts', args='outB out_mouse/test1/test_mouse-phaseval.csv out_mouse/test2/test_mouse-phaseval.csv out_mouse/test3/test_mouse-phaseval.csv')
    - runfile('C:/Aaallmine/git_repos/vgl/CustomScripts/interlimbCoordB.py', wdir='C:/Aaallmine/git_repos/vgl/CustomScripts', args='outB out_mouse/test1/test_mouse-phaseval.csv')
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
import csv
from interlimbCoordA import (circ_plots, save_phaseval)




# 1. Read text files into list of integers
def processInput(fileNames):
    
    # [Forelimbs,Hindlimbs,Ipsilesionals,Contralesionals,ContraFore-IpsiHind,IpsiFore-ContraHind]
    phasevals = [[],[],[],[],[],[]]
    
    for fileName in fileNames:
        
        try:
            with open(fileName, 'r') as csvfile:
                reader = csv.reader(csvfile, delimiter=',')
                i = 0
                for row in reader:
                    phasevals[i].extend(float(value) for value in row[1:] if value)    # ignore row index and empty values
                    i += 1
            
        except IOError:
            print('ERROR: File "' + fileName + '" does not exist')
    
    return phasevals
    


# MAIN
def main():
    
    # Input length
    if len(sys.argv) < 2:
        print('ERROR: Not enough arguments!')
        return -1
    
    # Configure output Directory
    outDir = "outB/"
    i = 1
    
    if len(sys.argv) > 2:
       outDir = sys.argv[1] + '/'
       i = 2
   
    if not os.path.exists(outDir):
       os.makedirs(outDir)
       
    # Extract animal ID and phasevals from clips
    phasevals = processInput(sys.argv[i:])
    animalID = sys.argv[i].split('/')[-1].replace('-phaseval.csv', '')
    
    
    # Plot Phaseval in radians on a circualr graph
    circ_plots(phasevals, animalID, outDir)
    save_phaseval(outDir, animalID, phasevals)


if __name__ == "__main__":
    main()