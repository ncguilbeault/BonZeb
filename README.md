# BonZeb
![](Resources/BonZeb_Logo.png)

# Table of Contents
1. [Preface](#preface)
2. [Download](#download)
3. [Dependencies](#dependencies)
4. [Video Acquisition](#video-acquisition)
5. [Behavioural Tracking and Analysis](#behavioural-tracking-and-analysis)
6. [Acknowledgements](#acknowledgements)

# Preface
BonZeb is a [Bonsai](https://bonsai-rx.org/) library for high-resolution zebrafish behavioural tracking and analysis. 
Bonsai-Rx provides a useful programming framework for applications in behavioural neuroscience. 
BonZeb provides essential tools for zebrafish researchers looking to implement closed-loop and open-loop behavioural feedback, with a specific focus on visual stimulation.
BonZeb is built on top of the Bonsai framework and users will need to be somewhat familiar with Bonsai before getting started.
For more information, please check out Bonsai's forums at [Gitter](https://gitter.im/bonsai-rx/Lobby) and [Google Groups](https://groups.google.com/forum/#!forum/bonsai-users)

# Download
This package was originally built in Bonsai version 2.4.0 but should work for new versions as well. 
You can download the latest [Bonsai](https://bonsai-rx.org/docs/installation/) or go to the [Bonsai Archives](https://bitbucket.org/horizongir/bonsai) to download earlier versions.
BonZeb can be downloaded online and installed using Bonsai's built-in package manager. BonZeb can also be added to Bonsai by downloading the [GitHub Repository](https://github.com/ncguilbeault/BonZeb) and adding it manually to the Bonsai package manager.

# Dependencies
Users should install the following Bonsai packages in addition to Bonsai's core packages: 
Bonsai – Arduino Library
Bonsai – Vision Library
Bonsai – Vision Design Library
Bonsai – Scripting Library
Bonsai – Windows Input Library
Bonsai – Video Library
Bonsai – Video Design Library
Bonsai – Shaders Library
Bonsai – Shaders Design Library

# Video Acquisition
BonZeb provides modules for interfacing with Allied Vision, Teledyne Dalsa, and Euresys CameraLink Frame Grabber hardware devices for high-speed video acquisition.
Users must install the manufacturers software development kit (SDK) and ensure the camera is connected to the computer and working properly before using the camera in Bonsai.
Both the Allied Vision and Teledyne Dalsa modules require bonsai to be run in no boot mode. To do this, Bonsai must be started from the command line with the following argument:

`Bonsai.exe --noboot`

# Behavioural Tracking and Analysis
BonZeb provides many different modules for behavioural tracking and analysis including methods for centroid tracking, tail tracking, eye tracking, and tail beat analysis.

# Acknowledgements
Thank you to all of the people who tested BonZeb and provided useful feedback. Thank you to the developers and to the larger Bonsai community for creating a welcoming and useful platform. 
