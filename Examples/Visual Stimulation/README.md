# BonZeb
![](../../Resources/BonZeb_Logo_Man.png)

# Visual Stimulation
BonZeb was built to interface with the Bonsai Shaders package to generate visual stimuli.
The Bonsai Shaders package utilizes OpenGL for rendering visual graphics. 
Bonsai Shaders provide extensive flexibility for programming in OpenGL. 
Visual stimuli in BonZeb are generated using a vertex file and a fragment file.

This folder contains the following sections:
1. [Visual Stimulus Library](#visual-stimulus-library)
2. [Visual Stimulation Workflow](#visual-stimulation-workflow)

# Visual Stimulus Library
The visual stimulus library contains the following visual stimuli:
1. Solid black
2. Solid white
3. Black-white flashes
4. Left Phototaxis
5. Right Phototaxis
6. Left Looming dot
7. Right Looming dot
8. Left Optomotor gratings
9. Right Optomotor gratings
10. Converging Optomotor gratings
11. Diverging Optomotor gratings
12. Concentric optomotor gratings
13. Left Optokinetic gratings
14. Right Optokinetic gratings
15. Forward Moving prey
16. Left Moving prey
17. Right Moving prey
18. Left Stationary prey
19. Right Stationary prey

# Visual Stimulation Workflow
The Bonsai workflow demonstrates how to control visual stimuli using variables calculated in Bonsai.
The position and heading angle are simulated in the workflow using the mouse cursor for position and a float variable for heading angle.
In a normal behavioural experiment, these values are calculated using behavioural data captured in real-time.
The `UpdateUniform` node connects values computed in the Bonsai workflow to uniform variables defined in the OpenGL shader.
Below is a picture of the Bonsai workflow that describes what each stream of the workflow is doing.

![](images/image1.png)

The stream labelled `Time` provides input to the time variable in the shader.
Each time a new frame is rendered, the `UpdateFrame` node produces an output value.
The `EventArgs.Time` attribute of the output is used to obtain the time between successive updates of the shader window.
The `Accumulate` node keeps track of the time in seconds since the start of the workflow.
The output of the `Accumulate` node feeds into the time variable of the shader.

The stream labelled `Simulated Fish Position` provides the inputs to the fish x and y position variables in the shader.
When the mouse cursor crosses into the the shader window, the `MouseMove` node produces an output.
The `NormalizedDeviceCoordinates` node maps the position of the cursor to coordinates relative to the shader.
`ExpressionTransform` nodes are used to extract the x and y coordinates into seperate streams.
The x and y coordinates are normalized once more inside the `ExpressionTransform` node.
The coordinates are converted to floats using the `single()` function inside the `ExpressionTransform`.
The output of each `ExpressionTransform` node is passed to the x and y position variables in the shader, respectively. 

The stream labelled `Stimulus Number` determines the visual stimulus number.
The output of the `Integer` node is sent to the stimulus number variable in the shader.

The stream labelled `Simulated Heading Angle` provides the inputs to the fish heading angle variable in the shader.
The output of the `Float` node is passed to the fish heading angle variable in the shader.
