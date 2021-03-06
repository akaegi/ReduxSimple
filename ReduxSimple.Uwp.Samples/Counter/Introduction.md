﻿The Counter example is a simple example to understand the logic of the Redux pattern and so the ReduxSimple library.

The documentation is divided in 9 parts that will focus on the code that will help you understand of the library works.

* The `State` part is a single class/object that defines the entire model/data used in your application
* The `Entities` that can be used in the state to manage collection of entities
* The `Actions` defines all possible events that will update/mutate your state
* The `Reducers` handles the logic of updating the state using the so-called Reducer pattern which is a function that given a state and an action give a new state 
* The `Selectors` provides reusable functions to consume a part of the state
* The `UI` that is still written in XAML
* The `Code-behind` that will listen to UI events and dispatch actions to the Store & listen to state change to alter the view 
* The `Effects` handle the logic outside of the State -> UI cycle, side effects are mainly asynchronous tasks that can potentially generate new actions
* The `Dependencies` block is here to give you information on what libraries/packages are needed to achieved what you see on screen whether it is UI or code-behind library 

The Counter example focus on simple things: a dead simple State with two simple Actions (Increment and Decrement) with a minimal UI and minimum code-behind.