# Delft AI Toolkit - Version 2
## Visual Authoring Toolkit for Smart Things

**This is a new 2.0 version of this project with a significantly changed architecture ([original version](https://github.com/pvanallen/delft-toolkit)). The NodeCanvas node system has been replaced with [xNode](https://github.com/Siccity/xNode), which is being enhanced by Siccity as part of this project. In addition, the system now communicates directly with the Raspberry Pi (instead of using node.js and bluetooth).**

**As of November 2018, this version is going through significant changes. We hope to have a more stable release by the end of 2018. At that time, we'll post a RasPi image that's ready to go to use with the toolkit. 

### Description

The Delft Toolkit a system for designing smart things. It provides a visual authoring environment that incorporates machine learning, cognitive APIs, and other AI approaches, behavior trees, and data flow to create smart behavior in autonomous devices.

![system diagram](https://i0.wp.com/www.philvanallen.com/wp-content/uploads/2018/01/Pasted_Image_1_16_18__3_50_PM.jpg?resize=640%2C350)

The toolkit is currently in rough prototype form as a part of my research. **It is likely to change significantly as I iteratively develop a technical and design strategy.**

The goal of this project is to develop an authoring system approach that enables designers to easily and iteratively prototype smart things. This approach includes the ability to Wizard-of-Oz AI behaviors and simulate physical hardware in 3D, and then migrate these simulations to working prototypes that use machine learning and real hardware.

* [Overall Project description](http://www.philvanallen.com/portfolio/delft-ai-toolkit/)
* [Process Blog](http://ai-toolkit.tumblr.com)

The system currently has two parts:
* Authoring & Control System running on a computer
  * Visual Authoring with nodes in the Unity3D authoring environment
* Robot/Device
  * Raspberry Pi
  * Arduino (we may transition to the Adafruit Crikit for RPi once it comes out and we have a chance to evaluate it)
  * Motors, servos, sensors, LEDs, microphone, speaker, camera, etc.

Each of these has a codebase, and includes a range of libraries. **We are now using and funding a new version of the open source [xNode Unity asset](https://github.com/Siccity/xNode), and no longer using [NodeCanvas](http://nodecanvas.paradoxnotion.com).**

**Hardware Architecture**
![hardware architecture](http://www.philvanallen.com/wp-content/uploads/2018/01/toolkit-architecture-diagram.jpg?resize=640%2C350)

# The below is currently being revised and is not complete. Stay Tuned.

## Starting the system
1. **Power robot**: Power on the Arduino and Raspberry Pi (RPi)
   * **Batteries**:
     * **Arduino** Powered by the USB cable from the RPi
     * **Motors**: Turn on the 6V AA battery pack
     * **RPi**: Connect the fast charging USB battery to the micro USB connector
1. **Login to RPi**: Open a terminal app on your computer and login to the RPi by typing:
   * **ssh pi@delftbt0.local** (change the last digit to match your setup)
1. **Get IP addresses**:
   * **Computer**
     * **Mac**: Hold the option key down, and click on your Wifi toolbar icon.
     * **PC**: See https://www.windowscentral.com/4-easy-ways-find-your-pc-ip-address-windows-10-s
   * **RPi**: On the command line, type the command: **ifconfig** In the output section for "wlan0" you'll see the IP address
1. **Start software**: In the following order
   * **Motors**: Power on the AA battery pack
   * **RPi**: Power and boot the RPi
     *  In the terminal, connect to the RPi and start the toolkit software. In the below, change the server_ip IP address to that of your computer. After launching delfToolkit.py the software will take a minute or two to finish setting up the object recognition models.
```
ssh pi@delftbt0.local
cd /home/pi/tutorials/image/imagenet
python3 delftToolkit.py --server_ip 10.0.1.15
```
   * **Unity3D**:
     * Open the "delft-toolkit" project in Unity3D
     * In the Hierarchy, open the main scene
     * In the Graphs directory, double click on the toolkit visual graph you are currently using (or one of the example graphs)
     * If you are using the robot hardware, click on the simulated robot in the Hierarchy, and enable the "Physical Ding" script. If you are not using the physical robot, keep this script unchecked and inactive.
     * Click on the Play button
     * Click on the 3D window (this is to ensure Unity is receiving all commands -- if you find it is not responding to the keyboard or OSC, try this)

## Installing The software

1. **Install dependencies**: [Unity3D](https://store.unity.com)
1. **Download the toolkit software** and place on your computer drive
   * Unity project and Arduino code from github [DelftToolkit](https://github.com/pvanallen/delft-toolkit-v2)
   * Disk image for [RPi]() NEW VERSION NOT YET AVAILABLE
1. **Arduino**:
   * Install delftToolkit.ino on your Arduino
1. **RPi**: Burn the RPi image to your SD card
   * Set up your WiFi
   * Change the hostname from the default of delftbt0 (e.g. delftbt1, delftbt2, etc.) if you are using more than one robot on your network
1. **Unity3D**:
   * Install NodeCanvas in the toolkit Project if it is not there
   * Click on the Project tab, and double click the "Main" scene
