
# Some Common Errors   


## Import Error: 'Tables'
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
- Moved the project folder to another directory
	- Open the .yaml file 
	- change the "project_path" to changed project directory path