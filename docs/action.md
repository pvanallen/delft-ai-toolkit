# Action Node

The action node performs a sequence of actions from top to bottom, where each action completes before the next one down runs. Control will pass on to the next node(s) when the last action completes.

The actions are communicated to all virtual or physical device with the specified name (e.g. Ding 1). In a typical situation, this means that the virtual representation of the AI system inside of Unity, and the physical device driven by the Raspberry Pi will respond simultaneously.

Currently, only the physical device will respond to the "Speak" and "Listen" actions, but a future release will enable the virtual system to do this as well.
_________________
<!-- TOC START min:2 max:3 link:true asterisk:false update:true -->
- [Creating Actions](#creating-actions)
- [Repeats](#repeats)
- [Random](#random)
- [Actions](#actions)
  - [Move - Causes the device to move](#move---causes-the-device-to-move)
  - [Servo - Moves a pan-tilt servo to an angle](#servo---moves-a-pan-tilt-servo-to-an-angle)
  - [Leds - Sets the color of device LEDs](#leds---sets-the-color-of-device-leds)
  - [Play Sound - Plays a sound effect](#play-sound---plays-a-sound-effect)
  - [Analogin - Starts or stops values from an analog input sensor](#analogin---starts-or-stops-values-from-an-analog-input-sensor)
  - [Touch - Starts or stops values from a touch input sensor](#touch---starts-or-stops-values-from-a-touch-input-sensor)
  - [Delay - Pauses this sequence of actions](#delay---pauses-this-sequence-of-actions)
  - [Text To Speech - Convert text to spoken word](#text-to-speech---convert-text-to-spoken-word)
  - [Speech To Text - Transcribe spoken words to text](#speech-to-text---transcribe-spoken-words-to-text)
  - [Recognize - Perform object recognition from the robot camera or by tag in the Unity virtual environment](#recognize---perform-object-recognition-from-the-robot-camera-or-by-tag-in-the-unity-virtual-environment)
  - [Touch - Get values from the capacitive touch sensors (**phys** robot only)](#touch---get-values-from-the-capacitive-touch-sensors-phys-robot-only)
  - [Train - Collects a series of images from the robot camera for Teachable Machine (**phys** robot only)](#train---collects-a-series-of-images-from-the-robot-camera-for-teachable-machine-phys-robot-only)
<!-- TOC END -->
_________________

<img src="images/actions-all.gif" width="270">

## Creating Actions
Each action added to the node will perform in sequence.

* **Add** - To add an action, click on the "+" button in the lower righty, which add a duplicate of the last action
* **Reorder** - Drag the "=" symbol to reorder them
* **Delete** - To delete a condition, select the condition row and click the "-" button

## Repeats

The repeats field sets the number of times the entire node sequence will run, top to bottom. If Random is enabled, the Repeat number determines the total number of random actions run, one per repeat.

## Random

The Random checkbox changes the behavior of the node to run one random action in the sequence each repeat. So if Repeats is set to 1, a single random action will run, and then control will pass to the next node. If Repeats is set to 5, a new random action in the sequence will run each repeat, for a total of 5 actions run.

**Note**: This is a standard random function, so it is possible for the same action to run twice in a row during random set of repeats.

## Actions
### Move - Causes the device to move
* ***Direction*** - Forward or Backward, or turn Left or Right in place (i.e. the two wheels turn in opposite directions)
* ***Virtual or Physical Robot*** - Whether the move command is sent to the Physical robot (**Phys**), the Unity Virtual robot (**Virt**), or both (**Both**)
* ***Time*** - Number of seconds the movement will run. Zero sets the movement to run forever (don't forget to use a Move Stop action to stop it), and control passes immediately on the to the next action.
* ***Speed*** - Speed of the movement

### Servo - Moves a pan-tilt servo to an angle
* ***Movement*** Type - Not yet implemented (immediate or varspeed). In the future will enable control of the speed of the servo motion. Currently moves at maximum speed.
* ***Port*** - The port the servo is attached to - pan or tilt (ports 1 or 2), or Servo ports 3 or 4 on the robot CRICKIT
* ***Time*** - Number of seconds the action waits before passing control to the next action
* ***Angle*** - The angle to move the servo to

### Leds - Sets the color of device LEDs
* ***Type*** - Determines how the LEDs will behave
  * Set - Sets the color of the LEDs
  * Blink - Blinks the LEDs, using the "port" setting for the number of blinks, and the "time" for the amount of time for each "on" period in seconds
  * All Off - Turns all the LEDs off regardless of port/color settings
* ***Port*** - Sets the LED to be set (0-12). If -1, all LEDs will be set
* ***Time*** - Number of seconds the action waits before passing control to the next action
* ***Color*** - Opens a color picker to set the color the LEDs will be set to

### Play Sound - Plays a sound effect
* ***Sound*** - Sets the sound to be played
* ***Time*** - Number of seconds the action waits before passing control to the next action
* ***User Sounds*** - Note that there are five placeholder sound files (UserSound1.wav - UserSound5.wav) built into the system which can be replaced by the user with their own sound files

  File format: **16bit/stereo/44.1K/.wav**
  * Unity - To change the sounds in Unity, go to the folder *Assets>Resources>ui_sounds* and replace any of the UserSound#.wav files with your own.
  * Robot - To change the sounds on the robot, Use FTP or mount the RPi as a server, and replace any of the UserSound#.wav files with your own.
    * FTP
      * In your FTP program (e.g. CyberDuck) connect to the RPi
      * Type in the IP address
      * Put in the RPi login user:pi, password:adventures
      * Navigate to *delft-ai-toolkit>audio>ui_sounds* and replace the files
    * Mount the RPi as a server (Mac)
      * In the Finder, select Go>Connect to server…
      * Type in the IP address of your RPi in the following way: smb://10.4.17.93
      * Press Connect, and put in the RPi login user:pi, password:adventures
      * You will then see the RPi appear in your servers in the left side of the Finder
      * Navigate to *delft-ai-toolkit>audio>ui_sounds* and replace the files


### Analogin - Starts or stops values from an analog input sensor
* ***Action*** - Start tells the virtual or physical device to start or stop sending values.
* ***Port*** - Specifies the robot hardware port the sensor is connected to (1-8).
  * It is possible to have the robot send more than one sensor data at a time. Plug your sensors into one of the six analog inputs on the Adafruit Crikit Hat (the ultrasonic distance sensor is normally plugged into input 1)
* ***Interval*** - Milliseconds of delay between each sensor value sent. E.g. 50ms means that values will be sent 20 times per second

### Touch - Starts or stops values from a touch input sensor
* ***Action*** - Start tells the physical device to start or stop sending values.
* ***Port*** - Specifies the port the touch wire is connected to.
  * It is possible to have the robot send more than one sensor at a time. Connect your sensing wires to one of the four touch inputs on the Adafruit Crikit Hat (labeled 1-4)
* ***Interval*** - Milliseconds of delay between each sensor value sent. E.g. 50ms means that values will be sent 20 times per second

### Delay - Pauses this sequence of actions
* ***Time*** - Seconds of delay before the next action (can be fractional)

### Text To Speech - Convert text to spoken word
* ***Model*** - Select the model to perform the TTS, Watson (cloud), Pico (edge, robot only)
  * **Note**: - To use the Watson model, you must enter the API Key/IAM key obtained from your Watson account ([more info](../watson.md)) in the Unity menu Delft AI Toolkit>Show Settings
* ***Virtual or Physical Robot*** - Whether the speech is generated on the Physical robot (**Phys**), the Unity Virtual robot (**Virt**), or both (**Both**)
* ***Time*** - Number of seconds the action waits before passing control to the next action
* ***Voice*** - Select a language, country, voice combination. Not all voices are available in all models. (Note: we are looking into making a gender neutral voice available). Typically, the "1" version is a male voice, and the "2" version is female.
* ***Utterance*** - The text to be spoken


### Speech To Text - Transcribe spoken words to text
* ***Model*** - Only Watson currently (limited to the languages IBM offers), and this option is not shown. In the future, we hope to also implement [DeepSpeech by Mozilla](https://hacks.mozilla.org/2019/12/deepspeech-0-6-mozillas-speech-to-text-engine/), which is an edge based system (i.e. no internet connection required).
  * **Note**: - To use the Watson model, you must enter the iamkey obtained from your Watson account in the Unity menu: Delft AI Toolkit>Show Settings
* ***Virtual or Physical Robot*** - Whether the speech is transcribed on the Physical robot (**Phys**), the Unity Virtual robot (**Virt**), or both (**Both**)
* ***Length*** - Seconds the device listens before turning off microphone and transcribing. Control passes immediately on to the next action regardless of setting
* ***Language*** - Select a language, country combination. Not all languages are available in all models.


### Recognize - Perform object recognition from the robot camera or by tag in the Unity virtual environment
* ***Recognition Model*** - If you are doing "phys" recognition with the robot camera, select the machine learning model to be used. In the Unity virtual environment, the Recognition Model setting has no effect. The models listed include several well-known models such as [mobilenet](https://github.com/tensorflow/models/blob/master/research/slim/nets/mobilenet_v1.md) and [squeezenet](https://en.wikipedia.org/wiki/SqueezeNet). These models are based on different datasets (image databases), and use different machine learning techniques. They vary in speed and accuracy - the user is encouraged to experiment with different models to experience how well they work for the user's application.
  * **Create a custom [Teachable Machine](https://teachablemachine.withgoogle.com) model**: The action's models begin with "teachable1" through "teachable3", which allow the user to install up to three custom models from the Google's Teachable Machine). See the ["teachable" documentation](teachable.md) to understand how to create and install a custom model trained on your own images and categories.
  * **Note**: True object recognition happens on the robot through its actual camera. Be sure to check that the camera is taking an adequate photo -- the image is on the Raspberry Pi: /home/pi/delft-ai-toolkit/delft-image.jpg. In the "virt" Unity environment "recognition" occurs by putting a Unity tag on any gameobject in the scene. Whatever the virtual robot gameobject is pointed at, will return the tag name on the object it "sees." Note also that the pan/tilt on the "virt" robot affects what it "sees" and the direction forward from the upper "tilt" surface is what it is pointing at.
* ***Threshold*** - For "phys" recognition with the camera, this sets the minimum confidence level of categories recognized by the model. For example, the recognition model will return up to five top objects recognized in the camera image. Each category recognized has a value (0-1) that is the model's confidence level. By setting this, the model will only return categories with confidence higher than this amount.
* ***Minimum distance*** - In the virtual environment, this number determines if the recognition returns the tag of the object it is pointing at. If the tagged object is close enough (i.e. within this minimum distance), the action returns the tag on the nearby object.

### Touch - Get values from the capacitive touch sensors (**phys** robot only)
* ***Action*** - **Start** tells the robot to start sending values. **Stop** ends the sending of values
* ***Port*** - Specifies the port of the touch sensor (1-4).
  * It is possible to have the robot send more than one sensor at a time. Plug your conductive wire (e.g. copper tape, or alligator clips) into one of the four touch inputs on the CRICKIT
* ***Interval*** - Milliseconds of delay between each sensor value sent. E.g. 50ms means that values will be sent 20 times per second

### Train - Collects a series of images from the robot camera for Teachable Machine (**phys** robot only)
* **Note**: - It may be best to use this long running (several minutes) action without any other actions to prevent conflicts or distractions. The robot will turn on it's LEDs, and use voice to count down before each image is captured. See the ["Teachable Machine" documentation](teachable.md) for more information on the use of this action
* ***Category Name*** - Sets the name of the directory where the series of images are stored on the robot.
* ***Delay*** - Sets the amount of time in seconds between each photo captured
* ***Number of Images to Capture*** - Specifies how many images to capture for training this category
