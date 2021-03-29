
# Some Common Errors   

## Debugging Environment
Anaconda prompt (run by administrator):
- conda activate dlc-windowsGPU


### Labelling
- Import Error: 'Tables'
	- conda install pytables
	- pip install tables
	- try to import tables in ipython as a test. If it doesn't work, try the first solution in the link:
		https://stackoverflow.com/questions/63022939/having-trouble-loading-tables-in-a-conda-environment-after-an-apparently-sucessf


### Miscallaneous Errors
- Moved the project folder to another directory
	- Open the .yaml file 
	- change the "project_path" to changed project directory path