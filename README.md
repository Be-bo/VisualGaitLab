<p align="center">
  <img src="https://github.com/Be-bo/VisualGaitLab/blob/master/logo.png" width="100" height="100">
</p>

This repository is a Visual Studio Project & Solution containing the code base for VisualGaitLab software.

---

## Setup and Installation

1. Install Visual Studio 2019 & VisualGaitLab (https://osf.io/2ydzn/).

2. Clone this repository into your directory of choice (we recommend using GitHub Desktop).

3. Navigate to the directory from step 2 and open "VisualGaitLab.sln" in Visual Studio.

  (4.) VisualGaitLab is installed into C: by default. As a result some operations require elevated permissions. VGL is set up such that it automatically asks for elevation if it hasn't been run as administrator, this kills Visual Studio's debugging mode. If you want to see debug output from the Visual Studio project in the console, you need to run Visual Studio as administrator.

<b>Important Note</b>: VisualGaitLab uses Miniconda 3 (installed automatically during VisualGaitLab installation in step 1). This is because it's using DeepLabCut (https://github.com/DeepLabCut/DeepLabCut), and DLC's functions are called through a Miniconda environment. When you are making any changes to your version of this repository, remember that it interfaces with the folders of the actual software that were installed in step 1. I.e. RUNNING THIS CODE WITHOUT INSTALLING VGL WILL NOT WORK. See the visual below for a clearer understanding.

![Visual](https://github.com/Be-bo/VisualGaitLab/blob/master/readme_visual.png)

---

## Common Errors

### Import Error: 'Tables'
This error will happen during any of DLC's operations (eg. labeling, training, etc.).
1.	Run the Anaconda prompt using administrative privileges
2.	Type and enter the following in order: 
  ```
  conda activate dlc-windowsGPU
  conda install pytables
  pip install tables
  ```
3.	For testing, type and enter the following:
  ```
  ipython
  import tables
  ```
 
If there are no error messages after importing the tables module, the problem has been resolved. 
Otherwise, you might get this error:
``` 
ImportError: DLL load failed: The specified module could not be found.
``` 
 
If this is the case, install the snappy module in the same anaconda prompt window (solution found on [stackoverflow][1]):
4.	Quit the ipython program by typing:       `quit`
5.	Then install snappy:
  ```
  conda install snappy
  ```
6.	Test using ipython and import tables as in step *3*.

[1]: https://stackoverflow.com/questions/63022939/having-trouble-loading-tables-in-a-conda-environment-after-an-apparently-sucessf


### Miscellaneous Errors
- Make sure your project .yaml file contains the correct file path
  - Open the .yaml file
  - change the "project_path" to correct project directory path

---
