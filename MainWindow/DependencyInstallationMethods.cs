using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using VisualGaitLab.OtherWindows;
using VisualGaitLab.SupportingClasses;

namespace VisualGaitLab {
    public partial class MainWindow : Window {

        //MARK: Additional Dependency Installation Functions

        string PFPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\VisualGaitLab\\";
        string DriveCommand = "c:";

        private void CheckInstallation() { //check whether the installation has been finished
            BarInteraction();
            string testGPUPath = ProgramFolder + @"\Miniconda3\envs\dlc-windowsGPU\Lib\site-packages\deeplabcut\pose_estimation_tensorflow\models\pretrained\resnet_v1_50.ckpt";
            if (File.Exists(testGPUPath)) {
                EnableInteraction();
                this.Visibility = Visibility.Visible;
                //ShowDisclaimer();
            }
            else FinishInstallation();
        }

        private void FinishInstallation() { //finish the installation by running the installation script
            if (MessageBox.Show("We recommend installing the latest GPU drivers. Keep in mind that only Nvidia GPUs are supported. By Clicking \"OK\" you're agreeing with Miniconda 3 and FFmpeg being installed onto your computer (into the VGL's installation folder).", //TODO ffmpeg?
               "Information", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel) {
                this.Close();
            }
            else {
                Console.WriteLine("about to initialize");
                DependencyLoadingWindow depWindow = new DependencyLoadingWindow();
                depWindow.Show();
                Console.WriteLine("done initializing");
                string[] yamlGPUlines = { //set up the initial .yaml Miniconda environment file
                "# dlc-windowsGPU.yaml ",
                "# ",
                "# DeepLabCut environment for use on FMI computers",
                "# FIRST: INSTALL CORRECT DRIVER for GPU, see https://stackoverflow.com/questions/30820513/what-is-the-correct-version-of-cuda-for-my-nvidia-driver/30820690",
                "# Suggested by Jan Eglinger see https://github.com/AlexEMG/DeepLabCut/issues/112",
                "#",
                "# install: conda env create -f dlc-windowsGPU.yaml ",
                "# update:  conda env update -f dlc-windowsGPU.yaml ",
                "name: dlc-windowsGPU",
                "dependencies:",
                "  - python=3.6",
                "  - jupyter==1.0.0",
                "  - tensorflow-gpu==1.13.1",
                "  - wxpython==4.0.4",
                "  - nb_conda=2.2.1",
                "  - cudnn=7",
                "  - pytables==3.4.3",
                "  - Shapely"
            };

                using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(PFPath, "dlc-windowsGPU.yaml"))) {
                    foreach (string line in yamlGPUlines)
                        outputFile.WriteLine(line);
                }

                string batPath = PFPath + @"\Miniconda3\Scripts\activate.bat"; //activate Miniconda
                Process p = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "cmd.exe";
                info.RedirectStandardInput = true;
                info.UseShellExecute = false;
                info.Verb = "runas";
                info.CreateNoWindow = true;
                p.EnableRaisingEvents = true;
                p.Exited += (sender1, e1) => {
                    MoveVDLCFiles();
                    InstallFFMPEG();
                    this.Dispatcher.Invoke(() => {
                        depWindow.Close();
                        this.Visibility = Visibility.Visible;
                    });

                };
                p.StartInfo = info;
                p.Start();

                using (StreamWriter sw = p.StandardInput) {
                    if (sw.BaseStream.CanWrite) //install every single Miniconda package separately (proved to be more reliable than using the .yaml environment file)
                    {
                        sw.WriteLine(DriveCommand);
                        sw.WriteLine("start /wait \"\" \"" + PFPath + @"\m3.exe" + "\" /InstallationType=JustMe /RegisterPython=0 /S /D=%PROGRAMFILES%\\VisualGaitLab\\Miniconda3"); //install it into the VDLC's folder
                        sw.WriteLine(DriveCommand);
                        sw.WriteLine("@\"" + batPath + "\"");
                        sw.WriteLine(DriveCommand);
                        sw.WriteLine("cd " + PFPath);
                        sw.WriteLine("conda update conda");
                        sw.WriteLine("conda env create -f dlc-windowsGPU.yaml");
                        sw.WriteLine("conda activate dlc-windowsGPU");

                        sw.WriteLine("pip install absl-py==0.8.1");
                        sw.WriteLine("pip install astor==0.8.0");
                        sw.WriteLine("pip install attrs==19.3.0");
                        sw.WriteLine("pip install backcall==0.1.0");
                        sw.WriteLine("pip install bleach==3.1.0");
                        sw.WriteLine("pip install certifi==2019.11.28");
                        sw.WriteLine("pip install chardet==3.0.4");
                        sw.WriteLine("pip install Click==7.0");
                        sw.WriteLine("pip install colorama==0.4.3");
                        sw.WriteLine("pip install cycler==0.10.0");
                        sw.WriteLine("pip install decorator==4.4.1");
                        sw.WriteLine("pip install deeplabcut==2.1");
                        sw.WriteLine("pip install defusedxml==0.6.0");
                        sw.WriteLine("pip install easydict==1.9");
                        sw.WriteLine("pip install entrypoints==0.3");
                        sw.WriteLine("pip install gast==0.3.2");
                        sw.WriteLine("pip install grpcio==1.16.1");
                        sw.WriteLine("pip install h5py==2.8.0");
                        sw.WriteLine("pip install idna==2.8");
                        sw.WriteLine("pip install imageio==2.6.1");
                        sw.WriteLine("pip install imageio-ffmpeg==0.3.0");
                        sw.WriteLine("pip install imgaug==0.3.0");
                        sw.WriteLine("pip install importlib-metadata==1.3.0");
                        sw.WriteLine("pip install intel-openmp==2019.0");
                        sw.WriteLine("pip install ipykernel==5.1.3");
                        sw.WriteLine("pip install ipython==7.11.1");
                        sw.WriteLine("pip install ipython-genutils==0.2.0");
                        sw.WriteLine("pip install ipywidgets==7.5.1");
                        sw.WriteLine("pip install jedi==0.15.2");
                        sw.WriteLine("pip install Jinja2==2.10.3");
                        sw.WriteLine("pip install joblib==0.14.1");
                        sw.WriteLine("pip install jsonschema==3.2.0");
                        sw.WriteLine("pip install jupyter==1.0.0");
                        sw.WriteLine("pip install jupyter-client==5.3.4");
                        sw.WriteLine("pip install jupyter-console==6.0.0");
                        sw.WriteLine("pip install jupyter-core==4.6.1");
                        sw.WriteLine("pip install Keras-Applications==1.0.8");
                        sw.WriteLine("pip install Keras-Preprocessing==1.1.0");
                        sw.WriteLine("pip install kiwisolver==1.1.0");
                        sw.WriteLine("pip install Markdown==3.1.1");
                        sw.WriteLine("pip install MarkupSafe==1.1.1");
                        sw.WriteLine("pip install matplotlib==3.0.3");
                        sw.WriteLine("pip install mistune==0.8.4");
                        sw.WriteLine("pip install mkl-fft==1.0.15");
                        sw.WriteLine("pip install mkl-random==1.1.0");
                        sw.WriteLine("pip install mkl-service==2.3.0");
                        sw.WriteLine("pip install mock==3.0.5");
                        sw.WriteLine("pip install more-itertools==8.0.2");
                        sw.WriteLine("pip install moviepy==1.0.1");
                        sw.WriteLine("pip install msgpack==0.6.2");
                        sw.WriteLine("pip install msgpack-numpy==0.4.4.3");
                        sw.WriteLine("pip install nb-conda==2.2.1");
                        sw.WriteLine("pip install nb-conda-kernels==2.2.2");
                        sw.WriteLine("pip install nbconvert==5.6.1");
                        sw.WriteLine("pip install nbformat==4.4.0");
                        sw.WriteLine("pip install networkx==2.4");
                        sw.WriteLine("pip install notebook==6.0.2");
                        sw.WriteLine("pip install numexpr==2.7.0");
                        sw.WriteLine("pip install numpy==1.17.2");
                        sw.WriteLine("pip install opencv-python==3.4.8.29");
                        sw.WriteLine("pip install opencv-python-headless==4.1.2.30");
                        sw.WriteLine("pip install pandas==0.25.3");
                        sw.WriteLine("pip install pandocfilters==1.4.2");
                        sw.WriteLine("pip install parso==0.5.2");
                        sw.WriteLine("pip install patsy==0.5.1");
                        sw.WriteLine("pip install pickleshare==0.7.5");
                        sw.WriteLine("pip install Pillow==7.0.0");
                        sw.WriteLine("pip install pip==19.3.1");
                        sw.WriteLine("pip install proglog==0.1.9");
                        sw.WriteLine("pip install prometheus-client==0.7.1");
                        sw.WriteLine("pip install prompt-toolkit==2.0.10");
                        sw.WriteLine("pip install protobuf==3.11.2");
                        sw.WriteLine("pip install psutil==5.6.7");
                        sw.WriteLine("pip install Pygments==2.5.2");
                        sw.WriteLine("pip install pyparsing==2.4.6");
                        sw.WriteLine("pip install pyrsistent==0.15.6");
                        sw.WriteLine("pip install python-dateutil==2.8.1");
                        sw.WriteLine("pip install pytz==2019.3");
                        sw.WriteLine("pip install PyWavelets==1.1.1");
                        sw.WriteLine("pip install pywin32==227");
                        sw.WriteLine("pip install pywinpty==0.5.7");
                        sw.WriteLine("pip install PyYAML==5.3");
                        sw.WriteLine("pip install pyzmq==18.1.0");
                        sw.WriteLine("pip install qtconsole==4.6.0");
                        sw.WriteLine("pip install requests==2.22.0");
                        sw.WriteLine("pip install ruamel.yaml==0.16.6");
                        sw.WriteLine("pip install ruamel.yaml.clib==0.2.0");
                        sw.WriteLine("pip install scikit-image==0.16.2");
                        sw.WriteLine("pip install scikit-learn==0.22.1");
                        sw.WriteLine("pip install scipy==1.3.2");
                        sw.WriteLine("pip install Send2Trash==1.5.0");
                        sw.WriteLine("pip install setuptools==44.0.0.post20200106");
                        sw.WriteLine("pip install Shapely==1.6.4.post2");
                        sw.WriteLine("pip install six==1.13.0");
                        sw.WriteLine("pip install statsmodels==0.10.1");
                        sw.WriteLine("pip install tables==3.4.3");
                        sw.WriteLine("pip install tabulate==0.8.6");
                        sw.WriteLine("pip install tensorboard==1.13.1");
                        sw.WriteLine("pip install tensorflow==1.13.1");
                        sw.WriteLine("pip install tensorflow-estimator==1.13.0");
                        sw.WriteLine("pip install tensorpack==0.9.8");
                        sw.WriteLine("pip install termcolor==1.1.0");
                        sw.WriteLine("pip install terminado==0.8.3");
                        sw.WriteLine("pip install testpath==0.4.4");
                        sw.WriteLine("pip install tornado==6.0.3");
                        sw.WriteLine("pip install tqdm==4.41.1");
                        sw.WriteLine("pip install traitlets==4.3.3");
                        sw.WriteLine("pip install urllib3==1.25.8");
                        sw.WriteLine("pip install wcwidth==0.1.7");
                        sw.WriteLine("pip install webencodings==0.5.1");
                        sw.WriteLine("pip install Werkzeug==0.16.0");
                        sw.WriteLine("pip install wheel==0.33.6");
                        sw.WriteLine("pip install widgetsnbextension==3.5.1");
                        sw.WriteLine("pip install wincertstore==0.2");
                        sw.WriteLine("pip install wxPython==4.0.4");
                        sw.WriteLine("pip install zipp==0.6.0");

                        sw.WriteLine("ipython");
                        sw.WriteLine("import deeplabcut");
                        sw.WriteLine("import tensorflow as tf");
                        sw.WriteLine("sess = tf.Session(config=tf.ConfigProto(log_device_placement=True))");

                    }
                }
            }
        }

        private void MoveVDLCFiles() {
            //erase all of the DeepLabCut files and replace it with a modified version that comes with this installation (modified for VDLC's purposes)
            string dlcTargetDir = Path.Combine(PFPath, @"Miniconda3\envs\" + EnvName + @"\Lib\site-packages\deeplabcut");
            string dlcInstallationDir = Path.Combine(PFPath, @"deeplabcut");
            FileSystemUtils.RecursiveDelete(new DirectoryInfo(dlcTargetDir));
            CopyFolder(dlcInstallationDir, dlcTargetDir);
        }

        private void InstallFFMPEG() //install ffmpeg - needed for some of VDLC's video manipulations
        {
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            info.Verb = "runas";
            p.EnableRaisingEvents = true;
            p.Exited += (sender1, e1) => {
                this.Dispatcher.Invoke(() => {
                    this.Close();
                    EnableInteraction();
                });
            };
            string binPath = PFPath + "\\ffmpeg\\bin";
            p.StartInfo = info;
            p.Start();

            using (StreamWriter sw = p.StandardInput) {
                if (sw.BaseStream.CanWrite) {
                    sw.WriteLine(DriveCommand);
                    sw.WriteLine("setx /M PATH \"" + binPath + ";%PATH%\""); //add ffmpeg environmental variable
                }
            }
        }

        private void CopyFolder(string sourceDir, string targetDir) //copy folder from src to target dest
        {
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir)) {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));
            }

            foreach (var dir in Directory.GetDirectories(sourceDir)) {
                CopyFolder(dir, Path.Combine(targetDir, Path.GetFileName(dir)));
            }
        }
    }
}
