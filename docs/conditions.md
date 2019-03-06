# Condition Nodes

The condition nodes (String/Text and Float/Numbers) wait for conditions to be met, and then pass control to the next node associated with the met conditions. They only act on new data received from it's input.

For example, if a sensor on the robot is reading distance, a condition can be set up to pass control to an action node that turns the robot when the distance indicates the robot is close to an object in front of it. If the robot is far enough away, another condition trigger can connect to a node the moves the robot straight forward.

Note that the interface for these nodes is still rough, and we hope to make them easier to use in the near future. 

## String Condition Node

### Setting the input data source
There are several things you need to set for the node receive the data it will evaluate
* **Robot ID** - The robot you are listening to (e.g. Ding 1)
* **Virtual or Physical Robot** - Whether the incoming data is from the Physical robot (**Phys**) or the Unity Virtual robot (**Virt**)
* **Data Source** - The "Incoming Signal Filter" to specify the specific incoming data that the condition will evaluate. Set the URL to match the type of input source you want. Use the following:


``` bash
/str/speech2text/ - from "Phys" robot, converting speech to text
/str/recognize/ - from "Phys" or "Virt" robot object classification
/str/keydown/ - from "Virt" keyboard key pressed
```
&nbsp;

 <img src="images/StringCondition-objectRecognition2.jpg" width="254"><img src="images/StringCondition-speech2text.jpg" width="254"><img src="images/StringCondition-keydown.jpg" width="254">

### Creating Trigger Conditions
The trigger conditions determine which node(s) will run next. If the trigger condition is met, the node connected (from the green dot) to that condition will run next. Multiple conditions are possible, and more than one can trigger at the same time.

* **Add** - To add a condition, click on the "+" button
* **Reorder** - Drag the "=" symbol to reorder them
* **Delete** - To delete a condition, select the condition and click the "-" button

### Four kinds of conditions
* **Starts With** - If the incoming text starts with the text in the condition field, the attached node will run
* **Ends With** - If the incoming text ends with the text in the condition field, the attached node will run
* **Contains** - If the incoming text contains the text in the condition field, the attached node will run. For this type, you can have multiple options separated by commas. If any of the comma delimited entries is matched, the condition will be true and the attached node will run. For example, in the above object recognition node, one condition is set to any of "car,van,vehicle,jeep" to handle different kinds of vehicles identified in the same way.
* **Otherwise** - If no conditions above the Otherwise condition have matched, the node connected to this condition will run

### The ! Not Checkbox
The ! checkbox will invert the condition you set up. So for example, if the condition is `Starts With` and text of `car` with the `! checkbox` checked then any incoming text that does NOT start with "car" will trigger that condition and corresponding connected node.
