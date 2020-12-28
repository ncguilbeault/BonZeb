---
permalink: /basics/
---

# Getting Started with the Basics
In this section, users will find information about the basics of Bonsai and the Bonsai programmming language as they relate to BonZeb.
These guides are meant to help users with little to no experience in Bonsai.
Experienced users can skip this section.
For more information about Bonsai, please visit these websites: [1](https://bonsai-rx.org/resources/), [2](https://bonsai-rx.org/community/), [3](https://gitter.im/bonsai-rx/Lobby), and [4](https://groups.google.com/forum/#!forum/bonsai-users).

# Starting Bonsai
When you run Bonsai, two windows will appear.
One window displays the `Get Started` window and the other window displays a terminal window.
The terminal window is not used for programming in Bonsai, but will display error codes or print statements as they appear from Bonsai at runtime.
In the `Get Started` window, users can create new projects, open projects, browse the Bonsai gallery, and open the package manager.

# Bonsai Interface
After creating or openning a project, the Bonsai interactive development environment (IDE) appears.

The `Menubar` can be found at the top of the interface. 
There are several useful tabs to select from in the menu.
Notably, you will find the `Package manager` under `Tools`.
The icons can be used to save a workflow, open a new workflow, start and stop a workflow, and more.

The `Toolbar` can be found at the leftside of the interface. 
This is where modules can be browsed or selected. 
If you know which module you are looking for, you can type it into the search bar. 
To add a module to the workflow, select a module from the toolbox and click and drag it onto the workflow.
Alternatively, you can select and press enter, or double click on the module in the toolbox to bring the module into the workflow. 
At the bottom of the toolbox section, there is a box which displays a general description of the module selected inside the toolbox.

`Properties` are found at the rightside of the interface. 
There is a box at the top for displaying a general description of the module that has been selected in the workflow. 
Underneath this is where a module’s properties can be set by the user. 
Each module has a unique set of properties, and some modules do not have any user-defined properties. 
At the bottom, there is the property descriptions section which provides a description of the property that has been selected.

The `Statusbar` is found at the bottom left corner of the interface. 
Bonsai is a compiled language, so a compiler is run before the start of a workflow to check if there are any outstanding errors in the workflow before starting. 
The statusbar will display a red X and an error message if there are any errors detected in the construction of the workflow. 
The module in the workflow that is producing the error will also turn red to indicate which node in the workflow is causing the error. 
If no errors are detected before runtime, the statusbar will display a green checkmark when ready.

The `Workflow` is located in the center of the IDE.
The workflow space is where users add modules and build connections between them to form data streams.

# Package Manager
Bonsai’s package manager is fully integrated into Bonsai’s interface and provides a convenient way to install online and local bonsai packages to the Bonsai IDE.
Users can access the package manager from the `Getting Started` window or from within the Bonsai IDE.
The package manager can be accessed by going to the `Tools` menu and selecting `Manage Packages...`.

On the left side of the `Package Manager`, users can select and view packages which are currently installed by selecting the `Installed packages`.
This will show all of the Bonsai packages and package dependencies that have been installed.

If you select one of the installed packages in the packages section, an option to uninstall will appear.
You can find out information about a package by selecting it in the package manager, such as a description of the packages contents, the author of the package, and the packages dependencies.
If you select uninstall, this will remove the package from the Bonsai IDE.
Note: packages which have other packages dependant on them cannot be uninstalled until all of the dependant packages are uninstalled first.

In the online section, users can browse packages that are available to download in bonsai.
Bonsai’s core packages can be found in the `Bonsai Packages` tab. 
Users can also browse `Community Packages`, packages that have been published online by developers in the Bonsai community. 
The search bar can be used to look for specific packages online. 
When a package is selected, an option to install the package will appear if the package is not already installed.
Packages that are already installed are shown with a green checkmark.

# Adding a local package
To add a local Bonsai package to the package manager, select the settings button and you will be brought to the settings page.
A new window will appear where users can add a local package to the package manager. 
First, select the add (+) button and a new source will appear.
Enter a name for the package source and the path to the package folder saved on the computer.
Make sure the checkbox next to the package has been checked.
Once all of this is set, press the `OK` button in the bottom right.
After the above steps, the package will be made available inside of the package manager.
The new package is available to install under online packages.

# Observable Sequences
The developers of Bonsai do a magnificent job explaining concepts and I highly recommend users to read [this](https://bonsai-rx.org/docs/observables/).
In summary, Bonsai (Bonsai-Rx) is built in C#/.NET and is based on the [ReactiveX (Rx)](http://reactivex.io/) programming architecture. 
Bonsai is an object-oriented, compiled programming language with a visual programming interface that operates using observable sequences. 
Observable sequences are essentially streams of data. 
Nodes are connected together to perform operations on observable sequences.
A downstream node subscribes to an event from an upstream node and becomes notified to perform some operation when data becomes available. 

There are 2 major types of observable sequences, hot and cold. 
An observable sequence is hot if it continuously produces new data regardless of whether downstream nodes are subscribed to it or ready to receive new input. 
An observable sequence is cold if the observable sequence waits to produce new data until the downstream nodes are all subscribed and ready to accept new input. 
I like to use a bakery as an analogy to explain this difference. 
A bakery produces a loaf of fresh bread once every hour.
The bakery throws out fresh bread if there is bread leftover from the previous hour.
If the bakery sells the previous load and a new customer comes in, the customer must wait until the next loaf of bread is produced after the hour.
In this case, the observable sequence produced by the bakery would be hot because bread is produced at a constant rate, regardless of how much bread was left from the previous hour and will not produce bread faster than the rate it is producing fresh bread, even if customers are willing to pay. 
Another bakery only produces bread when a request is received from a customer. 
In this case, the observable sequence is cold because the bakery will wait to bake bread until a customer has ordered and is waiting to receive the bread.
These two different types of observable sequences can lead to vastly different outcomes and behaviours.

# Types of Nodes
There are a few major classes of modules in Bonsai: sources, transforms, conditionals, sinks, and combinators. 
Sources produce data and are usually upstream in a pipeline. 
Source nodes consist of cameras, Arduinos, audio recording devices, timers, etc. 
Transforms take an input and produce an output. 
Some transforms will output the same data type as the input whereas other transforms will take one data type input and produce a different data type. 
Conditionals apply some operation to filter the input, similar to if statements in other programming languages, except the output is the original input and the condition is a boolean operation that is used to decide when to pass along data. 
Sinks perform side operations on the input without modifying the data.
Sink nodes consist of modules for saving data to a csv file, sending commands to an Arduino, writing videos, etc. 
Combinators are a large class of diverse nodes.
Each combinator has a specific function and produces different behaviors.
You can read more about specific combinator nodes in the [Combinators Section](#combinators)

# Data Types
The inputs and outputs of modules in Bonsai are data types, similar to data types in other programming languages. 
Bonsai contains standard data types, such as integers, floating point numbers, character strings, etc. 
The output of a module can be a standard data type or a data structure.
For example, the output type of the FileCapture module, a module which reads a video from a file and outputs the frames inside a bonsai workflow, is an OpenCV.Net.IplImage.
The IplImage data type comes from the OpenCV.Net library and is a complex data structure that not only contains the image itself, but also contains a number of relevant properties, such as the height and width of the image, the number of channels, etc. 
You can view the output type of any module along with it’s associated properties by right clicking the module in the workspace and browsing the output.

# Member Selectors
The properties of any output can be accessed individually by selecting the property or by using the `MemberSelector` node. 
You can then pass these output properties as specific inputs to downstream modules. 
Both the Height and Width properties of the IplImage data structure are integers. 
These new outputs, which are downstream of the FileCapture module, can now be visualized or passed as specific inputs to other downstream nodes.

# Externalized Properties and Property Mapping
Each module is equipped with its own unique set of user-defined properties.
The values of these properties can be set in the properties interface. 
Some of these properties will update while a workflow is running whereas others may only update after the workflow has restarted.
In addition to changing the properties manually, it is possible to set the value of these properties within the workflow itself using externalized properties and property mapping.
Properties can be externalized to become accessible within the Bonsai workflow. 
Externalize the property of a module by right-clicking a node and selecting the property to externalize from the `Externalize Property` list.

The output of a module can be passed as the input to a property. 
This can be used to change the property of a single module, multiple modules, at the start of a workflow, or dynamically. 
The `PropertyMapping` module can be used to map multiple inputs to different properties of a module in a single node. 
The `InputMapping` node differs from the `PropertyMapping` node in that it synchronizes the update of a property to the timing of an input. 
For example, a `String` node can be used as input to the `FileName` externalized property of the `FileCapture` node.

Once a property has been externalized, the name and description of the property can be set manually. If the property of a node is set in the properties section but also has the same property externalized, then the externalized property will override the property set in the node’s properties section. Additionally, setting the property of a node in the properties section will not propagate upstream to the value of the externalized property, so it is important to remember that if a property has been externalized, the value of the upstream externalized property must be set in order to update the property of the node downstream. 

# Property Source
A property source is used to generate a source node corresponding to a property of a node. 
The property source will generate the same data type of the property selected. 
The property source can be used to set the property of a node or multiple nodes using property mapping as discussed above. 
This is a useful technique for creating a source node for a specific data type and broadcasting the property to nodes throughout the workflow.
To generate a property source, right-click a module select the property from the `Create Property Source` list.

For example, the `PositionUnits` property of the FileCapture module uses a specific data type called CapturePosition from the Bonsai.Vision Library.
This data type is not available as a source module in the toolbox, so a `PositionUnits` property source can be generated from the FileCapture module.
The `PositionUnits` property source can then be used to set the PositionUnits property of the FileCapture module.

# Visualizers
Visualizers allow users to view the observable sequence generated by each module.
Visualizer windows appear when a workflow is started and the node attached to the visualizer window is in operation. 
Each module has a different set of visualizers that are specific to the data type of the output. 
For example, the image associated with IplImage data type can be viewed online with the unique IplImage visualizer available from the `Bonsai Vision Design` package.

Data types can have multiple visualizers.
In the above example, the `ObjectTextVisualizer` shows descriptive text describing some of the properties of the incoming images, such as height and width. 
Double-clicking on a node while the workflow is running will open the first visualizer of a module.

# Combinators
Combinators are a class of operators which are very diverse and can be very useful for building Bonsai scripts.
There are many different types of combinators, all of which have unique properties and behaviors.
Below are descriptions and examples of some of the most commonly used combinators.

# Zip, CombineLatest, WithLatestFrom
`Zip`, `CombineLatest`, and `WithLatestFrom`, are essential combinators to understand.
Each operator works by combining individual elements from multiple data streams into a single data stream consisting of multiple elements.
The behavior of each operator differs in the timing and content of their outputs.

`Zip` produces output only when each of the previous data streams produces a value. 
Both data streams must produce a new value for `Zip` to generate a single output. 
The timing of `Zip` is limited by the timing of the slower of the two input data streams and the output will always link the sequence of both inputs together. 

`CombineLatest` produces an output each time either of the input data streams generates a new value. 
Thus, the timing of `CombineLatest` is faster than the timing of either of the two input data streams and the output will consist of the most recent combination of inputs.
`WithLatestFrom` produces an output each time the driver sequence produces a value. 
The timing of `WithLatestFrom` is the same as the driver sequence but contains the most recent combination of both input streams from when the driver sequence produced a value.

To illustrate this difference, two `Timer` nodes are used. 
One produces a value every 3 seconds and the other produces a value every 5 seconds.
The outputs of each `Timer` are combined in 3 seperate data streams using `Zip`, `CombineLatest`, and `WithLatestFrom`.
Every time the combinators produces a value, an `Int` of 1 is accumulated with an `Accumulate` node to observe the total number of outputs from each combinator. 
As shown in the example below, the difference between combining data streams with `Zip`, `CombineLatest`, and `WithLatestFrom` is in the content and timing of their output.

[![](\assets\workflows\Zip CombineLatest WithLatestFrom.svg)](\assets\workflows\Zip CombineLatest WithLatestFrom.bonsai)

At the start of the workflow, both timers produce a value of 0.
Upon starting the workflow, all 3 combinators are the same in that each combined the two streams and produced a single output.

After running the workflow for 5 seconds, the different behaviours of each combinator can be observed.
The `Zip` node produced a new value only after both timers produced a value, so the total number of outputs is 2. 
The `CombineLatest` module produced the same output as `Zip`, except the number of outputs was 3.
This is because the `CombineLatest` node produced a value when the 3 second timer fired and another value when the 5 second timer produced a value. 
`WithLatestFrom` produced a value when the 3 second timer fired but did not produce a value when the 5 second timer fired because the 3 second timer was set as the driver sequence.

After 6 seconds, the 3 second timer produced a second value. 
The output of `Zip` did not change, because both sequences had not produced a second value, only the 3 second timer produced a value. 
However, both `CombineLatest` and `WithLatestFrom` produced a new value.
Since the 3 second timer produced a new value, `CombineLatest` and `WithLatestFrom` generated the same value and the total number of outputs increased by 1 for both.

After just 10 seconds, the outputs of these 3 different combinators start to diverge. 
`Zip` produced a value of (2, 2), despite the fact that the 3 second timer had produced a 3 when the 5 second timer had produced a 2 value. 
The reason for this is because `Zip` combines the data elements from both input data streams in sequence. 
In the case of using `Zip`, values from the 3 second timer were slowed down or limited by the rate of firing of the 5 second timer. 
`CombineLatest`, however, produced a new value every time a value was received from either the 3 second timer or the 5 second timer.
While `CombineLatest` always combines the most recent values, it produces more outputs than either of the sequences upstream.
`WithLatestFrom` synchronizes the most recent values from both data streams contingent on the outputs from the driver sequence. 
Thus, `WithLatestFrom` produced a value when the 3 second timer fired but did not generate a new output when the 5 second timer fired.

Consider the exact same scenario except using 2 timers that are set to fire at the same time of 5 seconds. 
Despite the fact that the timers are both set to fire at the same time, their timings are not synchronized and thus their timings are off.
This is an important thing to keep in mind when building pipelines.

# Groups
The group modules are subclass of combinators. 
Group nodes encapsulate a more complex workflow into a single node.
They tend to receive input from the main workflow, operate on the input using the encapsulated workflow, and then produce output back to the main workflow, though it is also possible to use a group node that receives no input and produces no output.
You can learn more about higher-order observables and groups [here](https://bonsai-rx.org/docs/higher-order/).

# NestedWorkflow
The `NestedWorkflow` module groups or encapsulates a complex workflow into a single node and places the encapsulated workflow into a new build context. 
Defining a subject inside a `NestedWorkflow` will make the subject only accessible to subscribers within the `NestedWorkflow`.

# GroupWorkflow
The `GroupWorkflow` module encapsulates a complex workflow into a single node.
Subjects defined inside a `GroupWorkflow` can be accessed outside the `GroupWorkflow`. 

# IncludeWorkflow
The `IncludeWorkflow` module encapsulates an external bonsai workflow (one that has been saved to a seperate file) and makes it easy to distribute commonly used processes across workflows. 
Subjects defined within an `IncludeWorkflow` are globally accessible outside of the `IncludeWorkflow`. 

# CreateObservable
The `CreateObservable` module generates a higher-order observable sequence using the enclosed workflow.
A higher-order observable sequence is essentially an observable sequence of observable sequences. 

# SelectMany
The `SelectMany` module is similar to the `CreateObservable` node, except it generates a single observable sequence for each individual input and merges the inputs together to generate output. 

# Scan
The `Scan` module is similar to a recursive function.
The input to the `Scan` module consists of both the input sequence and the output sequence of the `Scan` module. 

# Condition
The `Condition` module is a type of group node that filters input data.
It uses the encapsulated workflow to create a boolean comparison of the data, similar to an `if` statement in other programming languages.
If the boolean returns `True`, then the input data from the upstream sequence is passed along to subscribers downstream.
If the boolean returns `False`, the no data are passed downstream.

# Subjects
Subjects are a useful way to take the output of any module in any data stream and broadcast it to other parts of the workflow. 
Subjects are similar to global variables in other programming languages/
There are different ways to define subjects to produce different workflow behaviours. 
Below is an example demonstrating how to use subjects and why they are important.
There are two timers set to produce a value every 3 seconds and a value every 5 seconds, respectively.
The outputs of each `Timer` are combined in 3 seperate data streams using `Zip`, `CombineLatest`, and `WithLatestFrom`.
Every time the combinators produces a value, an `Int` of 1 is accumulated with an `Accumulate` node to observe the total number of outputs from each combinator. 

This is an example of a workflow with no subjects. 
Can you tell which timer is firing at 3 seconds or 5 seconds? 
Is it the timer in the top pipeline or is it the timer on the bottom? 
Do you know which accumulate corresponds to which data stream? 
If you had access to the properties section of each Timer, then you could determine which node was which.
However, the timer cannot be inferred based on the visualizer alone. 
The same is true about which accumulate modules are associated with which data sequence.
Subjects can be used not only to broadcast variables, but can be used to assign relevant variable names to each output.

`PublishSubject` nodes were added in the example to help understand the behaviour and output of each timer/combinator.
Subjects are a type of sink, meaning they simply pass along the input from upstream nodes to downstream nodes without modifying the input.
In this example, subjects were used to provide each timer with a relevant variable name.
Visualizing the outputs of the subjects rather than their associated upstream node allows us to distinguish which data stream we are visualizing. 

Different types of subjects produce different behaviours. 
`PublishSubject` changes a cold observable sequence into a hot sequence.
`ReplaySubject` turns a hot sequence into a cold sequence.
`BehaviorSubject` is similar to `ReplaySubject` except that it will wait for subscription even if the sequence it is broadcasting has been terminated. 
The `MulticastSubject` pushes values from one data stream into a subject originating from another data stream. 
`SubscribeSubject` gives access to the observables of a subject. 
Depending on where subjects are placed inside of group nodes, they will only broadcast the subject within the group node and not to the entire global workflow. 
More information about subjects can be found [here](https://bonsai-rx.org/docs/subjects/).

It is important to understand the difference in behaviors between subjects. 
In the example below, a `Timer` is set to fire every 2 seconds.
The output is passed to a either a ReplaySubject or PublishSubject. 
In separate data streams, a SubscribeSubject is used to access the variables from either the ReplaySubject or PublishSubject. 
A `DelaySubscription` module is used for each `SubscribeSubject` which delays the subscription of data for 3 seconds.

After 2 seconds, the timer produced a value which was sent to both the `PublishSubject` and `ReplaySubject`. 
The `DelaySubscription` modules did not subscribe to the `SubscribeSubject` nodes for the `PublishSubject` and `ReplaySubject` variables until after 3 seconds. 
Once 3 seconds had passed, both DelaySubscription modules subscribed to the `PublishSubject` and `ReplaySubject` modules. 
Only the data stream subscribing to the `ReplaySubject` produced a value. 
This difference in behavior can be explained by the temperature of the observable sequence (i.e. hot vs cold).
The `PublishSubject` module generates a hot observable sequence whereas the `ReplaySubject` module generates a cold sequence.
Since the `PublishSubject` module did not produce any new values once the `DelaySubscription` node had subscribed to it, the value of the timer that was sent to the `PublishSubject` had been essentially discarded. 
This is not the case for the `ReplaySubject` module, which had generated a cold observable sequence. 
When the `DelaySubscription` module subscribed to the `ReplaySubject` even after the timer produced a value, the `ReplaySubject` produced the most recent value of the sequence. 
After letting the workflow run for a longer period of time, eventually both streams subscribing to the `PublishSubject` and `ReplaySubject` nodes receive the value of the timer once the timer produced a value.
To illustrate another important point, a `Take` module is placed after the timer to terminate the sequence after the first value is produced.

After running this workflow for 3 seconds, the workflow stops when the time to subscribe by the `DelaySubscription` modules elapses. 
The source node generating observables for both the PublishSubject and ReplaySubject terminates after the first value is produced.
Thus, even the `PublishSubject` and `ReplaySubject` nodes are incapable of producing values.
Since no more values are capable of being produced throughout the entire workflow, the workflow terminates.
Below, a `BehaviorSubject` is added into the workflow.

After 3 seconds, the workflow continues to run despite the fact that no more values are being produced. 
`BehaviorSubject` will wait to produce a value to any modules subscribing to the sequence, even after the data stream producing the values has terminated.
This only works if a subscriber is issued to `BehaviorSubject`.
If you remove the `SubscribeSubject` for the `BehaviorSubject` variable, the behaviour of the workflow will return to what it was previously and terminate after 3 seconds.

[![](\assets\workflows\Subjects.svg)](\assets\workflows\Subjects.bonsai)

# Shaders
BonZeb was built to interface with the Bonsai Shaders package to generate visual stimuli.
The Bonsai Shaders package utilizes OpenGL for rendering visual graphics. 
Bonsai Shaders provide extensive flexibility for programming in OpenGL. 
Visual stimuli in BonZeb are generated using a vertex file and a fragment file.

# Fragment and Vertex Files
Fragment and vertex files are essential components of the OpenGL shader rendering pipeline.
The vertex file (ending in .vert) processes vertices.
Vertices map areas of the shader window into a texture space to be processed by the fragment file.
The vertex coordinates, given by vp, can range from (-1, -1) to (1, 1), where each of these coordinates represent opposite corners of the shader window.
Below is an example of a vertex file.
In this case, we define the vertices to be in the furthest corners of the shader window so that the shader will be rendered to the entire window.

The fragment file (ending in .frag) receives texture coordinates and processes these into fragments.
Fragments determine what colour value to assign each coordinate.
The texture coordinates processed by the fragment shader range from (0, 0) to (1, 1).
Below is an example of a fragment file.
In this case, we colour each fragment white.

Together, the vertex and fragment files work to render visual stimuli to the shader window.
The result of this shader when run will be a white rectangle displayed to the entire window.

# Shader Configuration
For Bonsai Version <= 2.4, the `UpdateFrame` node is needed inside the Bonsai workflow to configure the shader.
The Bonsai shader configuration window specifies the shader's rendering properties and rendering environment. 
To open the shader configuration dialogue, double click on the `UpdateFrame` node.
The first set of properties correspond to the `Window` parameters of the shader.

In the `Render Settings` section, the `Display Device` property sets which video display to render the shader window onto.
Under `Window Style`, changing the height and width will change the resolution of the shader window.
Changing the `Window border` will determine whether the shader window can be resized, fixed, or borderless.
The `State` property determines whether the shader should be started normally, fullscreen, minimized, or maximized.

A mesh must be set in the `Meshes` menu as follows.
A mesh defines the area inside a window that will contain the shader.
A mesh with the type `TexturedQuad` is added to the list of meshes, but other types of OpenGL meshes can be added as well.
The `Name` property attributes the mesh a variable name that will be used for the defining the shader.
Changing the `QuadEffects` property will flip the visual stimulus.

The `Textures` tab alows you to instantiate and specify textures which will get rendered onto the shaders.
For this example, no textures are added to the shader.

A shader must be specifed in the `Shaders` menu as follows.

A `Material` is added as the type of shader.
The same variable name as the mesh or a new variable name can be ascribed to the `Name` property of the shader.
The `Name` of the shader will become available to other nodes inside the Bonsai workflow.
The `FragmentShader` property is given the path of the fragment file (.frag) which defines how we compute values for each coordinate in the shader.
The `VertexShader` property is set to the path of the vertex file (.vert) which defines how we map coordinates of the material into fragments.
The `MeshName` property takes the name of the mesh defined in the `Meshes` section.

For Bonsai Version >= 2.5, Shaders are loaded inside the Bonsai workflow directly.
The `CreateWindow` node is used to initialize and configure a bonsai shader window.
The `MeshResources` node configures the mesh resources for the shader.
The `TextureResources` node configures the texture resources for the shader.
The `ShaderResources` node configures the shader resources for the shader.
All of the configuration resources are loaded with the `LoadResources` node.

[![](\assets\workflows\Shaders.svg)](\assets\workflows\Shaders.bonsai)

# Uniform Variables
Uniform variables allow you to pass variables from the Bonsai workflow to the shader. 
Uniform variables are defined inside the shader and are used to calculate parameters of the visual stimuli.
Below is an example, which defines a uniform variable inside the fragment file of the shader.
In this example, the uniform variable `scale` is used to modify the grayscale values of each fragment.

Bonsai workflows can feed input to uniform variables using the `UpdateUniform` node.
Set the shader name property of the `UpdateUniform` node to the name of the shader defined during configuration.
Set the uniform variable property to the name of the variable defined inside the shader.
Then, a pipeline is set up to feed data from Bonsai to the uniform variable.
In the example below, a `Float` value is passed to the `UpdateUniform` node, which will determine the scale value of the fragments in the shader. 

In this case, the scale only changes value when we manually change the value of `Float`. 
We can go a step further and use a dynamic input to the uniform variable.
Below is an example where a dynamic value is generated inside the Bonsai workflow and processed by the shader.

A `MouseMove` node calculates the position of the mouse cursor in the shader window. 
The `NormalizedDeviceCoordinates` transforms (normalizes) the coordinates of the mouse with respect to the shader window to map the values of the cursor from -1 to 1. 
An `ExpressionTransform` node maps the values of the x coordinate between 0 and 1 for the fragment shader and converts this to a float type using the single function. 
The output is then passed to the `UpdateUniform` node.
In this way, the grayscale intensity of the shader window is dependant on the x position of the mouse, which changes as the cursor moves from left to right across the shader window.

[![](\assets\workflows\UpdateUniform.svg)](<\assets\workflows\UpdateUniform.zip>)
