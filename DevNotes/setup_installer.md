
# Setup Installer   

## Prerequisites
- Visual Studio
- deps folder (includes a frozen version of deeplabcut)
- VS InstallerProjects extension

## Notes
If the installer project has already been setup, you only need to build your solution and rebuild the installer project.


## Steps

### Set up Installer Project
1. Open the VGL solution in the VS. Right click the solution in the solution explorer and add -> new project. 
look for "Setup Wizard" and add it. 
2. In the Wizard:
   - Which project output groups to include: all
   - Which additional files: none 
And then you should see the File System window. If not, right click the newly created project and choose view -> file system.

### File System
3. Application Folder: drag and drop the contents of the "deps" folder (not the folder itself)
4. Add shortcuts and icons as shown in the video
5. Setup Project Properties: 
   - Author & Manufacturer: The Whelan Lab
   - ManufacturerURL: https://patrick-whelan-lab-calgary.squarespace.com
   - ProductName: Visual Gait Lab
   - SupportURL: https://github.com/Be-bo/VisualGaitLab
   - RemovePreviousVersions: True
   - Title: VGL Setup
   - Version: (add a new number -> click YES for changing product code)
6. Application folder properties: Change installation folder to: `[ProgramFilesFolder]\[ProductName]`

### User Interface
7. Right Click Setup Project in Solution Explorer, View -> User Interface, to open the UI window
8. Remove the installation folder step for both normal and admin Install.
9. Right click on Start dialog folder: Add new dialogs for readme and license in both regular and admin Install.
So Start should have the following dialogs by at the end: Welcome, License Agreement, Read Me, Confirm Installation
10. Add banner to all dialogs' properties.

### Build
11. Right click on the Setup project in Solution Explorer and click Rebuild.
This will take a while so make sure everything else is fine before building.
12. Go to the Debug folder in the project directory and use the .msi file as the installer. (the .exe file is unnecessary) 


## References and Tutorials
- https://youtu.be/fehVTLNQorQ
- https://youtu.be/Y5MSPyT1Gr0
