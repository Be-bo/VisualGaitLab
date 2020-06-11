This repository is a Visual Studio Project containing the code base for VisualGaitLab software.

Setup Steps:

1. Install Visual Studio 2019 & VisualGaitLab (https://osf.io/2ydzn/).

2. Create a new project with a new Visual Studio solution OR open an existing solution.

3. Clone this repository into your folder of choice (we recommend using GitHub Desktop).

4. Open the solution from step 2.

5. Right-click the solution in the Solution Explorer and choose Add -> Existing Project -> Choose "VisualGaitLab.csproj" located within the folder containing this repository.

Important Note: VisualGaitLab uses Miniconda 3 (installed automatically during VisualGaitLab installation in step 1). This is because it's using DeepLabCut (https://github.com/DeepLabCut/DeepLabCut), and DLC's functions are called through a Miniconda environment. When you are making any changes to your version of this repository, remember that it interfaces with the folders of the actual software that were installed in step 1. I.e. RUNNING THIS CODE WITHOUT INSTALLING VGL WILL NOT WORK. See the visual below for a clearer understanding.

![Visual](https://github.com/Be-bo/VisualGaitLab/blob/master/readme_visual.png)
