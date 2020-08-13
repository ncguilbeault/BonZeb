# BonZeb
![](../../Resources/BonZeb_Logo.png)

# Calibration
Calibration is essential to closed-loop and open-loop visual feedback.
There are multiple ways to calibrate a visual display in Bonsai. 
We provide a simple calibration method to map the area used for visual stimulus rendering to a camera’s FOV or a specified region within the camera’s FOV.
Below is the full Bonsai workflow.

![](images/image1.png)

For calibration, we generate a simple white rectangle on a black background. 
We ensure that we can see the white rectangle in our camera’s FOV either by removing the IR filter or using a common reference point visible to us by eye and the camera (i.e. a translucent ruler). 
The goal for calibration is to match the edges of the presented rectangle to the edges of the camera’s FOV.
The `DrawRectangle` node generates the values for the x and y offset, as well as the x and y range needed to update the vertex file. 


