# Action Node

The action node performs a sequence of actions from top to bottom, where each action completes before the next one down runs. Control will pass on to the next node(s) when the last action completes.

The actions are communicated to all virtual or physical device with the specified name (e.g. Ding 1). In a typical situation, this means that the virtual representation of the AI system inside of Unity, and the physical device driven by the Raspberry Pi will respond simultaneously.

Currently, only the physical device will respond to the "Speak" and "Listen" actions, but a future release will enable the virtual system to do this as well.

<img src="images/ActionNode.jpg" width="271">

## Creating Actions
Each action added to the node will perform in sequence.

* **Add** - To add an action, click on the "+" button
* **Reorder** - Drag the "=" symbol to reorder them
* **Delete** - To delete a condition, select the condition and click the "-" button

## Repeats

The repeats field sets the number of times the entire node sequence will run, top to bottom. If Random is enabled, the Repeat number determines the total number of random actions run, one per repeat.

## Random

The Random checkbox changes the behavior of the node to run one random action in the sequence each repeat. So if Repeats is set to 1, a single random action will run, and then control will pass to the next node. If Repeats is set to 5, a new random action in the sequence will run each repeat, for a total of 5 actions run.

Note that this is a standard random function currently, so it is possible for the same action to run twice in a row during random set of repeats.

## Actions
* **Move** - Causes the device to move
  * *Direction* - Forward or Backward, or turn Left or Right in place (i.e. the two wheels turn in opposite directions)
  * *Time* - Number of seconds the movement will run. Zero sets the movement to run until changed, and control passes immediately on the to the next action.
  * *Speed* - Speed of the movement
* **Servo** - Moves a servo to an angle
  * *Movement* Type - Not yet implemented. In the future will enable control of the speed of the servo motion. Currently moves at maximum speed.
  * *Port* - The port the servo is attached to
  * *Time* - Number of seconds the action waits before passing control to the next action
  * *Angle* - The target angle to move the servo to
* **Leds** - Sets the color of device LEDs
  * *Type* - Not yet implemented. In the future will blink capability
  * *Port* - Sets the LED to be set (0-12). If -1, all LEDs will be set
  * *Time* - Number of seconds the action waits before passing control to the next action
  * *Color* - Opens a color picker to set the color the LEDs will be set to
* **Play Sound** - Plays a sound effect
  * *Sound* - Sets the sound to be played
  * *Time* - Number of seconds the action waits before passing control to the next action
* **Analogin** - Starts or stops values from a sensor
  * *Action* - Start tells the virtual or physical device to start sending values. Stop ends the sending of values
  * *Port* - Specifies the port the sensor is connected to
  * *Interval* - Milliseconds of delay between each sensor value sent. E.g. 50ms means that values will be sent 20 times per second
* **Delay** - Pauses the sequence of actions
  * *Time* - Seconds of delay before the next action
* **Speak** - Causes the device to voice text
  * *Voice* - Not yet implemented
  * *Time* - Number of seconds the action waits before passing control to the next action
  * *Utterance* - The text to be spoken
* **Listen** - Causes the device to listen to a microphone and transcribe what it hears to text
  * *Mode* - Not yet implemented.
  * *Length* - Seconds the device listens before turning off microphone and transcribing. Control passes immediately on to the next action regardless of setting 
