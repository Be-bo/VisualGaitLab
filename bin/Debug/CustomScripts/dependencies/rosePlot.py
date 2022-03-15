# -*- coding: utf-8 -*-
"""
Created on Tue Feb 22 11:24:02 2022

@author: Zahra Ghavasieh

An Implementation of the Rose Plot From MATLAB
"""


import numpy as np
import matplotlib.pyplot as plt



# Windrose Plot
# Theta is a list of angles (in radians) that determines the angle of each bin from the origin.
# The length of each bin reflects the number of elements in theta that fall within a group, 
# which ranges from 0 to the greatest number of elements deposited in any one bin.
def windRosePlot(theta, binNum, outDir, title='', scatter=False):
    
    # Calculate items in each bin
    width = np.pi * 2 / binNum
    bins = []
    for i in range(binNum):
        bins.append([t for t in theta if i*width <= t < (i+1)*width])

    # Compute pie slices
    radii = [len(b)/len(theta) for b in bins if len(b)!=0]
    print(radii, bins)
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
    plt.savefig(outDir+title+".png", bbox_inches='tight')
    flushPlot()


def flushPlot():
    plt.draw()
    plt.clf()
    plt.cla()
    plt.close()

