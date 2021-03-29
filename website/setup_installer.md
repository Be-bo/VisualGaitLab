
# Setup Installer   

## Prerequisites
- Visual Studio
- deps folder (includes a frozen version of deeplabcut)


## Steps
1. Download and install (with VS closed): https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects
2. Open the VGL solution in the VS. Right click the solution in the solution explorer and add -> new project. 
look for "Setup Project" and add it. 
Call it whatever you want, check all the boxes when it asks about including files and when it asks about additional files just click finish (i.e. don't add any). 
And then you should see what the person in the video is seeing at 2:30, if not, right click the newly created project and choose view -> file system.
3. Start watching at 2:30: https://www.youtube.com/watch?v=fehVTLNQorQ
4. FileSystem -> Application Folder: put everything that's in the "deps" folder.
5. Also automatically install the primary output into the proper folder (default program files path it'll suggest to you + VisualGaitLab).
6. Add shortcuts and icons as shown in the video
7. Setup Project Properties: 
   - Author & Manufacturer: The Wheelan Lab
   - ManufacturerURL: https://patrick-whelan-lab-calgary.squarespace.com
   - ProductName: Visual Gait Lab
   - SupportURL: https://github.com/Be-bo/VisualGaitLab
   - RemovePreviousVersions: True
   - Title: VGL Setup
   - Version: (add a new number -> click YES for changing product code)
8. Right Click Setup Project in Solution Explorer, View -> User Interface
9. UI: remove the installation folder step for both normal and admin Install.
10. UI dialog properties: add banner, readme, license, etc.
