## String Condition Node##

### Setting the input data source###
There are several things you need to set for the node receive the data it will evaluate
* The robot you are listening to (e.g. ding1)
* Whether the data is coming form the Physical robot (**Phys**) or the Unity Virtual robot (**Virt**)
* The "Incoming Signal Filter" to specify the specific incoming data that the condition will evaluate. Set the URL to match the type of source you want.


``` /str/speech2text/ - from "Phys" robot, converting speech to text
/str/recognize/ - from "Phys" robot object classification
/str/keydown/ - from "Virt" keyboard key pressed
```
&nbsp;

 <img src="StringCondition-objectRecognition2.jpg" width="254"><img src="StringCondition-speech2text.jpg" width="254"><img src="StringCondition-keydown.jpg" width="254">

In each trigger condition text field, you can then set the response you are looking for. In the Speech to Text example above, you can see that there are four conditions the node is set to match - "hello" "recognize" "no transcription" (i.e. the person didn't speak, or the speech to text transcription failed), and Otherwise (which means any other input of what the person said).

Note also that you can have multiple options in within a single trigger condition. For example, in the Object Recognition node, the first trigger condition has "car,van,vehicle,jeep" which shows how using commas between each object name allows that single condition to respond to a range of inputs, and cause the same action for all four.

Lastly, after you have created the trigger conditions, you connect the output of each one to the appropriate next node to be triggered when that condition is met.
