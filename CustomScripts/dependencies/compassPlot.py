# -*- coding: utf-8 -*-
"""
Created on Tue Feb 22 11:22:06 2022

@author: Zahra Ghavasieh

References: 
    - https://ocefpaf.github.io/python4oceanographers/blog/2015/02/09/compass/

An Implementation of the Compass Plot From MATLAB
"""


import numpy as np
import matplotlib.pyplot as plt
from matplotlib.lines import Line2D




# Regular polar compass
def polarCompass(X, Y, outDir='', title='compass', ext='.png', Colors=['r','b','y','m','c','g'], leg=[]):
    
    drawCompass(X, Y, title=title, Colors=Colors, leg=leg)

    # Save and Show Plot
    savePlot(title, outDir=outDir, ext=ext)





# Compass Plot with a clockwise orientation and 0 at the top
def northCompass(X, Y, outDir='', title='compass', ext='.png', Colors=['r','b','y','m','c','g'], leg=[]):

    ax = drawCompass(X, Y, title=title, Colors=Colors, leg=leg)

    # Make graph clockwise and move 0 to the top
    ax.set_theta_direction(-1)
    ax.set_theta_offset(np.pi/2.0)
    
    # Save and Show Plot
    savePlot(title, outDir=outDir, ext=ext)





# Save and Show Plot
def savePlot(title, outDir='', ext='.png'):
    
    plt.savefig(outDir+title+ext, bbox_inches='tight')
    plt.draw()
    plt.clf()
    plt.cla()
    plt.close()




# Draw a Compass graph
# Params:
    ## (X,Y)    - list of float coordinates
    ## title    - Plot Title
    ## Colors   - List of matplotlib colors to use per compass arrow
def drawCompass(X, Y, title='compass', Colors=['r','b','y','m','c','g'], leg=[]):
    
    # Convert from Cartesian to Polar Coordinates
    radii = np.hypot(X, Y)
    angles = np.arctan2(Y, X)
    
    # Draw Plot
    fig, ax = plt.subplots(subplot_kw=dict(polar=True))
    
    # Construct dictionary of arguments used for each arrow
    kw = dict(arrowstyle="->", color='k')

    # Draw Arrow
    for i in range(len(angles)):
        angle = angles[i]
        radius = radii[i]
        kw.update(color=Colors[i % len(Colors)])
        
        ax.annotate("", xy=(angle,radius), xytext=(0,0), arrowprops=kw)

    
    ax.set_ylim(0, np.max(radii))
    ax.set_rlim(0, 1)

    
    # Ticks
    ax.set_xticks(np.arange(0, 2.0*np.pi, np.pi/6))
    ax.set_rticks(np.arange(.2,1,.2))
    
    # Legend
    if len(leg) != 0:
        # Shrink current axis by 20%
        box = ax.get_position()
        ax.set_position([box.x0, box.y0, box.width * 0.8, box.height])
        
        custom_lines = []
        for i in range(len(angles)):
            custom_lines.append(Line2D([0],[0],color=Colors[i % len(Colors)], lw=2))
        
        ax.legend(custom_lines,
                  [leg[i % len(leg)] for i in range(len(angles))],
                  loc='upper left',
                  bbox_to_anchor=(0.9, 0.15))    # loc is anchored to bbox (x,y)
    
    
    ax.set_title(title)
    return ax;

