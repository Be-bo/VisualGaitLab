<p align="center">
  <img src="https://github.com/Be-bo/VisualGaitLab/blob/master/logo.png" width="100" height="100">
</p>

This repository is a Visual Studio Project & Solution containing the code base for VisualGaitLab software.

Setup Steps:

1. Install Visual Studio 2019 & VisualGaitLab (https://osf.io/2ydzn/).

2. Clone this repository into your directory of choice (we recommend using GitHub Desktop).

3. Navigate to the directory from step 2 and open "VisualGaitLab.sln" in Visual Studio.

(4.) VisualGaitLab is installed into C: by default, hence it requires elevated permissions. If you want to see the debug output from the Visual Studio code in the console, you need to run Visual Studio as administrator.

<b>Important Note</b>: VisualGaitLab uses Miniconda 3 (installed automatically during VisualGaitLab installation in step 1). This is because it's using DeepLabCut (https://github.com/DeepLabCut/DeepLabCut), and DLC's functions are called through a Miniconda environment. When you are making any changes to your version of this repository, remember that it interfaces with the folders of the actual software that were installed in step 1. I.e. RUNNING THIS CODE WITHOUT INSTALLING VGL WILL NOT WORK. See the visual below for a clearer understanding.

![Visual](https://github.com/Be-bo/VisualGaitLab/blob/master/readme_visual.png)
