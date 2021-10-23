---
permalink: /docs/visual-stimulation-workflow/
sidebar:
  - nav: "docs"
---

# Visual Stimulation Workflow
The Bonsai workflow demonstrates how to control visual stimuli using variables calculated in Bonsai.
The position and heading angle are simulated in the workflow using the mouse cursor for position and a float variable for heading angle.
In a normal behavioural experiment, these values are calculated using behavioural data captured in real-time.
The `UpdateUniform` node connects values computed in the Bonsai workflow to uniform variables defined in the OpenGL shader.

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