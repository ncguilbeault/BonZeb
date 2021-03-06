# BonZeb
![](../../Resources/BonZeb_Logo_Man.png)

# Behavioural Tracking and Analysis
BonZeb was designed to perform rapid behavioural tracking and analysis of zebrafish.
BonZeb behavioural tracking seamlessly integrates with Bonsai to enable flexible and robust tracking paradigms.
BonZeb provides nodes to perform tail curvature analysis, tail beat analysis, eye angle analysis, and more.

This folder contains the following sections:
1. [Basic behavioural tracking and analysis](#basic-behavioural-tracking-and-analysis)
2. [Head-fixed Tracking](#head-fixed-tracking)
3. [Timed online tracking](#timed-online-tracking)
4. [Automated offline video tracking](#automated-offline-video-tracking)
5. [Automated offline tail beat analysis](#automated-offline-tail-beat-analysis)
6. [Automated offline batch process](#automated-offline-batch-process)

# Basic behavioural tracking and analysis
Most behavioural tracking protocols for free-swimming adhere to the following pipeline:

![](images/basictracking1.png)

Video is processed using background subtraction.
The background subtracted image is thresholded to obtain a binary image.
Connected components are calculated from the pixels of the binary image.
The centroid of the connected component corresponding to the animal is then used to calculate points along the tail of the animal.
The angles between each of these points is calculated and normalized to the heading angle.
The tail angles can then be used to analyze tail beat kinematics.
Below is an example of a Bonsai workflow which can implement this behavioral tracking and analysis pipeline using BonZeb.

![](images/basictracking2.png)

The `CameraCapture` node generates images of the animal.
These images are then converted to grayscale using the `Grayscale` node.

![](images/basictracking3.png)

The grayscale images are then passed onto a `CalculateBackground` node to calculate the background as the darkest or lightest pixels over time.
The grayscale image and the background are then zipped together using a `Zip` node.
The background is then subtracted from incoming images using the `AbsoluteDifference` node.

![](images/basictracking4.png)

Following background subtraction, the centroid is calculated using the `CalculateCentroid` node.
The `CalculateCentroid` node performs a threshold to create a binary image.
The raw image moments are then calculated from the binary image moments.
The center of mass derived from the raw image moments is taken as the centroid of the animal.
The background subtracted image and the centroid are zipped together with `Zip`.
These are then passed to the `CalculateTailPoints` node, which searches for the tail points in the image, given the centroid.

![](images/basictracking5.png)

The tail points are then passed to the `CalculateTailCurvature` node to normalize and compute the angle between successive points.
These angles, commputed in radians, are then converted to units of degrees using the `ConvertRadiansToDegrees` module.
The `DetectTailBeatKinematics` node receives the tail angles as input and computes the tail beat frequency, amplitudes, and bout instance.
The angles and the tail kinematics are saved to seperate csv files using `CsvWriter` nodes.

![](images/basictracking6.png)

This pipeline sets up the basis for tracking in more complex online and offline workflows.

# Head-fixed Tracking
The tracking protocol used for head-fixed tracking is very similar to free-swimming tracking with some exceptions.
Below is an example workflow that uses this method.

![](images/headfixed1.png)

The main difference is that rather than calculating a moving centroid, a static point is used to seed the tail tracking algorithm.
The `HeadingDirection` property of the `CalculateTailPoints` node is set to the fixed heading angle of the animal.
We generate a single static point using the `CreatePoint2f` node and combine it with the latest image from the camera using `WithLatestFrom`.

![](images/headfixed2.png)

# Timed online tracking
The following workflow sets up a more complex system.

![](images/timedtracking1.png)

Here, things are organized into `GroupWorkflows`.
Below shows what is inside the `VideoAcquisition` group.

![](images/timedtracking2.png)

All that happens inside the `VideoAcquisition` group is the `CameraCapture` node broadcasts images to the workflow using a `PublishSubject` node called `Image`.
To use a specific camera module, simply change the `CameraCapture` node to a similar node which produces the `IplImage` data type.
Below is what happens inside the `CalculateBackground` group node.

![](images/timedtracking3.png)

The background is calculated using the `CalculateBackground` node.
When the user believes that the background has been sufficiently extracted from the live video, the user presses the `Tab` key on the keyboard.
When the `Tab` key is pressed, the background calculate stops and the `Tracking` group is initiated.

![](images/timedtracking4.png)

Inside the `Tracking` group, there is a lot of overlap with the previous example.
The images from the camera are combined with the background followed by background subtraction.
Once the centroid is calculated, the tail points are calculated followed by the the tail angles.
The `ExpressionTransform` node takes the average of the last 3 tail segments.

![](images/timedtracking5.png)

Instead of saving all of the tail kinematic data into a single csv file, we select each value (frequency, amplitude, and instance) and save each individually to a csv file.

![](images/timedtracking6.png)

We also introduce a protocol for tracking of the eyes and heading angle.

![](images/timedtracking7.png)

The eyes are tracked by taking a `Threshold` of the image.
The threshold value must be large enough such that the eyes are seperated into 2 distinct binary regions.
The image produced by this `Threshold` is zipped with the tail points and passed to the `FindEyeContours` function.
The `FindEyeContours` node finds the binary regions corresponding to the left and right eye.
These data are generated as a `ConnectedComponentCollection`.

![](images/timedtracking8.png)

The `ConnectedComponentCollection` generated by the `FindEyeContours` node contain the `ConnectedComponents` corresponding to each eye.
This output is zipped with the tail points and passed to the `CalculateEyeAngles` node.
The `CalculateEyeAngles` node uses information about the tail points and the binary regions of the eyes to effectively normalize and calculate each eye angle with respect to the heading angle.
The angle for the left and right eye are then saved using `CsvWriter` nodes.

![](images/timedtracking9.png)

The `ConnectedComponentCollection` produced from the `FindEyeContours` is zipped with the tail points and passed onto the `CalculateHeadingAngle` node.
The `CalculateHeadingAngle` node uses the centroids of each eye and the tail points to calculate the heading angle.
The heading angle is saved to file.

![](images/timedtracking10.png)

In the final branch of the `Tracking` group workflow, a video with the tracking data overlaid is saved to a `.avi` file using the `VideoWriter` node.
The `DrawTailPoints` node overlays the calculated tail points onto the image and produces a new image.
The `DrawHeadingAngle` node draws an arrow corrsponding to the heading angle onto the image.
The `DrawEyeAngles` node draws the angles of each eye onto the image.

![](images/timedtracking11.png)

The `Timer` group workflow sets the time for how long we would like to track live video.
The `Timer` group receives tracked video from the `Tracking` group and waits one minute until terminating the workflow.
The `WorkflowInput` node, represented as the `Source` to the group, is subscribed to by the `Background` subject using the `SubscribeWhen` node.
This mechanism is what tells the `Tracking` node to wait until the background has been calculated.

![](images/timedtracking12.png)

The `Timer` node sets the duration for how long tracking should proceed.
Setting the `Period` property of the `Timer` will adjust how long data will be collected.
When the `Timer` branch fires after the elapsed `Period`, the `Tracking` data stream stops, which is controlled by the `TakeUntil` and `TakeLast` nodes.

![](images/timedtracking13.png)

# Automated offline video tracking
Similar to the more complicated example above, the workflow can be adopted to automatically track a pre-acquired video.
The offline tracking workflow is shown below.

![](images/offlinetracking1.png)

The `CalculateBackground` group uses the entire pre-acquired video to calculate the background and takes the final background calculated for tracking.

![](images/offlinetracking2.png)

The `Tracking` group workflow is the same as before.
The `Tracking` group is initiated once the background has been calculated.
The `TakeLast` node determines when the last image of the video has been processed and then terminates the workflow.

![](images/offlinetracking3.png)

# Automated offline tail beat analysis
BonZeb can also be used to further process data files which have already been collected.
In the example below, the tail beat kinematics are calculated based on a pre-acquired tail angles data file.

![](images/offlineanalysis1.png)

The `CsvReader` node loads all of the data from the csv file.
It produces a single `String` output for each row contained in the data file.
The `String` is parsed using the `Parse` node, which seperates each row of data into a list of strings.
Each string in the list corresponds to a single tail angle at a given time.
These values are then converted into a `Double` data type.
The data are then used to calculate the tail beat frequency, amplitude, and bout instance, using the `DetectTailBeatKinematics` module, and subsequently saved to file.

# Automated offline batch process
Offline batch processing of videos is also possible with BonZeb.
In the example below, the workflow is set up for batch processing.

![](images/batchprocess1.png)

The `GetFiles` node loads a list of filenames inside of a folder.
The list of strings is passed to a `SelectMany` node.
The `SelectMany` node projects each of these names into an observable window, which get processed using the following workflow.

![](images/batchprocess2.png)

The `Concat` node takes the list of filenames and concatenates them such that each filename is processed into a seperate observable window in sequence.
The filename is processed by a higher-order observable sequence defined inside the `CreateObservable` node.
The higher-order observable sequence generated by the `CreateObservable` node is concatenated again and added into a list, which also gets concatenated outside of the `SelectMany` node.
Inside the `CreateObservable` node, the name of each file is received as input and is broadcast to the `CalculateBackground` and `Tracking` group workflows.

![](images/batchprocess3.png)

The `CalculateBackground` group starts calculating the background, using the `Filename` subject to determine which video to use for processing.

![](images/batchprocess4.png)

When the background is finished, the `Tracking` group begins.
It uses the same tracking pipeline as the `Automated offline video tracking` example with one exception.
The `Filename` of each video is used to create a prefix which gets added to the naming of each of the csv files generated during data collection.

![](images/batchprocess5.png)
