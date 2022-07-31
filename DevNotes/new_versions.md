
# New Version Uploading Process   

## Prerequisites
- Set up Installer was built correctly as instructed in the `setup_installer.md` file.
- Access to OSF and GitHub


## Steps

### Build
1. In Setup Project Properties: Change version number -> click YES for changing product code
2. Right click on the VisualGaitLab Project in the Solution Explorer and select Properties located at the bottom.
   - In the Application tab select Assembly Information
   - Change File version to the version number
3. Right click on the Setup project in Solution Explorer and click Rebuild.
This will take a while so make sure everything else is fine before building.

### ZIP 
4. Navigate to the Debug Folder of the setup project and create a zip file containing the files: `setup.exe` and `vgl_setup.msi`    
5.  Name the zip file `vgl_latest_setup.zip` or `vgl_stable_setup.zip` accordingly    

### GitHub
6. Make sure the branches are updated as desired  
7. Go to releases and draft a new release  
8. Title: **V X.Y**, replacing *X* and *Y* with responding version numbers  
9. Mention release date and OSF link for documentation  
10. Mention a list of updates  
11. Add the installer zip file  
12. Add relevant updates (same as OSF) to the Wiki Section

### OSF
13. In the _Files_ section, update the corresponding zip file  
14. In the _Wiki_ section, update the version number(s) in `Home`  
15. _Wiki:_ Update `Version Updates & Changes`  
16. _Wiki:_ Update `Bug Reports` and `Gait Analysis Calculations` if needed  