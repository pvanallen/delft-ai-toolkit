# Delft AI Toolkit - Version 2
## Visual Authoring Toolkit for Smart Things

**This is a new 2.0 version of this project with a significantly changed architecture ([original version](https://github.com/pvanallen/delft-toolkit)). The NodeCanvas system has been replaced with a customize version of [xNode](https://github.com/Siccity/xNode). In addition, the system now communicates directly with the Raspberry Pi (instead of using node.js and bluetooth).**

**As of late June 2018, this version is going through significant changes and is not ready for general use. We hope to have a release by the end of July.**

### Description

The Delft Toolkit a system for designing smart things. It provides a visual authoring environment that incorporates machine learning and behavior trees to create smart behavior in autonomous devices.

![system diagram](https://i0.wp.com/www.philvanallen.com/wp-content/uploads/2018/01/Pasted_Image_1_16_18__3_50_PM.jpg?resize=640%2C350)

The toolkit is currently in rough prototype form as a part of my research. **It is likely to change significantly as I iteratively develop a technical and design strategy.**

The goal of this project is to develop an authoring system approach that enables designers to easily and iteratively prototype smart things. This approach includes the ability to Wizard-of-Oz AI behaviors and simulate physical hardware in 3D, and then migrate these simulations to working prototypes that use machine learning and real hardware.

* [Overall Project description](http://www.philvanallen.com/portfolio/delft-ai-toolkit/)
* [Process Blog](http://ai-toolkit.tumblr.com)

The system currently has two parts:
* Control System running on a PC
  * Visual Authoring System, Unity3D
* Robot
  * Raspberry Pi
  * Arduino
  * Motors, sensors, microphone, camera, etc.

Each of these has a codebase, and includes a range of libraries. **We are now using a modified version of the open source [xNode Unity asset](https://github.com/Siccity/xNode).** ~~In particular, the Unity system is based on the [NodeCanvas](http://nodecanvas.paradoxnotion.com) paid asset for Unity3D, and provides the foundation for the visual authoring of behavior trees. My hope is to eventually make this part of the system free in some way.~~

# The below is currently out of date, and needs to be revised to confrom to the new architecture. Stay Tuned.

## Starting the system
1. **Power robot**: Power on the Arduino and Raspberry Pi (RPi)
   * **Adapters**:
     * **Arduino**: Connect a USB cable from computer to Arduino
     * **RPi**: Connect a wall power adapter to the micro-usb connector
   * **Batteries**:
     * **Arduino**: Turn on the 9V battery
     * **Motors**: Turn on the AA battery pack
     * **RPi**: Connect the fast charging USB battery to the micro USB connector
1. **Login to RPi**: Open a terminal app on your computer and login to the RPi by typing:
   * **ssh pi@delftbt0.local** (change the last digit to match your setup)
1. **Get IP addresses**:
   * **Computer**
     * **Mac**: Hold the option key down, and click on your Wifi toolbar icon.
     * **PC**: See https://www.windowscentral.com/4-easy-ways-find-your-pc-ip-address-windows-10-s
   * **RPi**: On the command line, type the command: **ifconfig** In the output section for "wlan0" you'll see the IP address
1. **Start software**: In the following order
   * **Arduino**: Power on the device
   * **RPi**:
     *  In the terminal connected to the RPi, type:
```
ssh pi@delftbt0.local
cd /home/pi/tutorials/image/imagenet
python3 raspi-ding-server.py --server_ip 10.0.1.15
```

     * In the above command, change the IP address to that of your computer. The software will take a minute or two to finish setting up the TensorFlow model.
   * **Node.js**:
     * Open a new terminal window/tab, and type the following:
```
cd /Users/Yourname/directoryWhereYouPutIt
node hub 10.0.1.28 delftbt0
```

     * The easiest way to do the "**cd**" command is to select the directory in the finder, and drag it into the temrinal window to get the path.
     * In the "**node**" command, change the IP address to that of your RPi, and the last digit of "delftbot0" to number of your setup.
   * **Unity3D**:
     * Open the "delft-toolkit" project in Unity3D
     * In the Hierarchy, click on the "Main Camera" and then open the "Canvas" tab
     * Click on the Play button
     * Click on the 3D window (this is to ensure Unity is receiving all commands -- if you find it is not responding to the keyboard or OSC, try this)
     * Note that the Unity project may crash if the Node process is not already running when you Play.

## Installing The software
Currently for my students -- some details may not be fully worked out

1. **Install dependencies**: [Unity3D](https://store.unity.com), [NodeCanvas](https://assetstore.unity.com/packages/tools/visual-scripting/nodecanvas-14914), [Arduino IDE](https://www.arduino.cc/en/Main/Software), [Node.js](https://nodejs.org/en/), (Mac) [Xcode](https://geo.itunes.apple.com/us/app/xcode/id497799835?mt=12) and accept dependencies
1. **Download the toolkit software** and place on your computer drive
   * Software from github
   * Disk image for [RPi](https://www.dropbox.com/s/f79kt8v7ear3i1z/delftbot_backup.img?dl=0)
1. **Arduino**:
   * Edit arduino-aitoolkit.ino to change the BLE local name if you will have more than one robot running at a time.
   * Install the arduino-aitoolkit.ino on your Arduino
1. **RPi**: Burn the RPi image to your SD card
   * Set up your WiFi
   * Change the hostname from the default of delftbt0 (e.g. delftbt1, delftbt2, etc.) if you are using more than one robot on your network
1. **Node.js**:
   * Open Xcode and accept the license terms
   * Open a terminal window and CD into the nodejs folder, and run
   * **npm install**
1. **Unity3D**:
   * Install NodeCanvas in the toolkit Project if it is not there
   * Click on the Project tab, and double click the "Main" scene
