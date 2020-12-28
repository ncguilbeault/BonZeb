---
permalink: /introduction/
sidebar:
  - title: "BonZeb"
    image: /assets/images/BonZeb_Logo_Man.svg
    image_alt: "BonZeb"
    text: "A Bonsai library for behavioural tracking and stimulation."
---

# Introduction
Welcome to BonZeb - a [Bonsai](https://bonsai-rx.org/) library for behavioural tracking and stimulation of zebrafish and other model organisms.

BonZeb is a [Bonsai](https://bonsai-rx.org/) library for high-resolution behavioural tracking and stimulation. 
BonZeb provides essential tools for zebrafish behavioural tracking and analysis.
Here, you will find information and examples for how to use BonZeb to implement behavioural analysis, multi-animal tracking, closed-loop and virtual open-loop visual stimulation, and more.
For more information on Bonsai, please check out the forums on [Gitter](https://gitter.im/bonsai-rx/Lobby) and [Google Groups](https://groups.google.com/forum/#!forum/bonsai-users)

# Download
This package was originally built in Bonsai version 2.4.0 but should work for newer versions of Bonsai as well. 
You can download the latest version of [Bonsai](https://bonsai-rx.org/docs/installation/) or download earlier versions [here](https://github.com/bonsai-rx/bonsai/releases).
BonZeb can be easily downloaded using Bonsai's built-in package manager. 
BonZeb can also be added to Bonsai manually by downloading the [GitHub Repository](https://github.com/ncguilbeault/BonZeb) and adding it to the package manager.
To add the package manually to the package manager, navigate to settings and add the .nuget folder of the repository to the list of available package sources. 

# Dependencies
When installing BonZeb, all of the essential dependencies will be installed automatically. However, some of the example workflows draw from other non-dependant Bonsai libraries. These libraries include:

* Bonsai - Arduino Library
* Bonsai - Core Library
* Bonsai - Design Library
* Bonsai - Dsp Library
* Bonsai - Dsp Design Library
* Bonsai - Editor
* Bonsai - Player
* Bonsai - Scripting Library
* Bonsai - Shaders Library
* Bonsai - Shaders Design Library
* Bonsai - System Library
* Bonsai - System Design Library
* Bonsai - Video Library
* Bonsai - Video Design Library
* Bonsai - Vision Library
* Bonsai - Vision Design Library
* Bonsai - Windows Input Library

# Getting Started with Bonsai
Bonsai is a powerful programming language with a visual programming interface that uses graphs, also called workflows, to represent data processing pipelines in the interactive development environement (IDE). 
The nodes in a Bonsai workflow, also called modules, can represent a variety of different Bonsai objects, such as data sources, transformations, combinators, or sinks. 
Modules in a workflow are linked by edges to form pipelines, also called data streams, where the output of one node is sent as input to another node. 
The basic premise for building Bonsai workflows is adding nodes into the workflow and connecting nodes together to form pipelines or data streams. 
Data streams can be split and merged together in various ways such that the final workflow can have multiple data streams operating synchronously or asynchronously, simultaneously.

If you are using Bonsai for the first time, check out [Basics](</basics/>) to learn more. If you are already familiar with Bonsai and how it works, feel free to skip this section.

# Video Acquisition
BonZeb is completely modular and works with any camera module alreay supported in Bonsai (Basler, FLIR, DirectShow, etc.). Additionally, BonZeb interfaces with Allied Vision, Teledyne Dalsa, and Euresys CameraLink Frame Grabber hardware devices for high-speed video acquisition. The following video acqusition packages are available for download:

* [Allied Vision](https://github.com/ncguilbeault/Bonsai.AlliedVision)
* [Teledyne Dalsa](https://github.com/ncguilbeault/Bonsai.TeledyneDALSA)
* [Euresys CameraLink](https://github.com/ncguilbeault/Bonsai.EuresysCameraLink)

Users must install the manufacturers software development kit (SDK) and ensure the camera is connected to the computer and working properly before using the camera in Bonsai.
Both the Allied Vision and Teledyne Dalsa modules require bonsai to be run in no boot mode. To do this, Bonsai must be started from the command line with the following argument:

`Bonsai.exe --noboot`

# Visual Stimulation
BonZeb can be used with Bonsai Shaders to generate closed-loop and open-loop visual stimuli with controlled behavioural feedback.
The Bonsai Shaders package utilizes OpenGL for rendering visual stimuli and offers immense flexibility.

See the [Visual Stimulation](<Examples/Visual Stimulation>) folder for how to generate visual stimuli such as optomotor gratings, optokinetic gratings, looming dots, small prey, and phototaxic stimuli.

# Calibration
BonZeb provides a simple solution for users to perform calibration of visual display devices.
Users can use BonZeb to map the area used for visual stimulus rendering to a camera’s field of view (FOV) or a specified region within the FOV.

See the [Calibration](<Examples/Calibration>) folder for how to calibrate visual display devices for rendering visual stimuli.

# Behavioural Tracking and Analysis
BonZeb provides many different modules and workflows for behavioural tracking and analysis.
These include methods for background calculation, centroid tracking, tail tracking, eye tracking, and tail beat analysis.
BonZeb can be used for online tracking, offline tracking, and batch processing.

See the [Behavioural Tracking and Analysis](<Examples/Behavioural Tracking and Analysis>) folder for how to implement these methods.

# Multi-animal Tracking
BonZeb provides examples for how to perform multi-animal free-swimming and head-fixed tracking. 

See the [Multi-animal Tracking](<Examples/Multi-animal Tracking>) folder for how to perform multi-animal free-swimming and head-fixed tracking.

# Virtual Open-loop Free-swimming
BonZeb provides examples for how to build and implement virtual open-loop free-swimming behavioural assays. 

See the [Virtual Open-loop Free-swimming](<Examples/Virtual Open-loop Free-swimming>) folder for how ti implement virtual open-loop free-swimming behavioural assays.

# Closed-loop Head-fixed
BonZeb provides examples for how to build and implement closed-loop head-fixed behavioural assays. 

See the [Closed-loop Head-fixed](<Examples/Closed-loop Head-fixed>) folder for how a closed-loop head-fixed behavioural assay can be used and implemented.

# Optogenetics
BonZeb provides examples for how to perform optogenetics. 

See the [Optogenetics](<Examples/Optogenetics>) folder for how to implement an optogenetic stimulation paradigm.

# Calcium Imaging
BonZeb provides examples for how to perform calcium imaging with closed-loop visual stimulation. 

See the [Calcium Imaging](<Examples/Calcium Imaging>) folder for how to implement calcium imaging with closed-loop visual stimulation.

# Acknowledgements
Thank you to all of the people who tested BonZeb and provided useful feedback on the manuscript. Thank you to the developers for creating the elegant and powerful programming language Bonsai and to the larger community for welcoming the exchange of ideas. 