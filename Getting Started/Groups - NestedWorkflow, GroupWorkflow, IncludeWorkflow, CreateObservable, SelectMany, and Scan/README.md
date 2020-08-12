# BonZeb
![](../../Resources/BonZeb_Logo.png)

# Groups
The group modules are class of modules with diverse functions. 
Group nodes encapsulate a more complex workflow into a single node.
They tend to receive input from the main workflow, operate on the input using the encapsulated workflow, and then produce output back to the main workflow. 
It is also possible to define a group node that receives no input and produces no output.

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

You can learn more about higher-order observables and groups [here](https://bonsai-rx.org/docs/higher-order/).
