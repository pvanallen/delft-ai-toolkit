# Delft AI Toolkit
# Visual Authoring Toolkit for AI Smart Things

**As of early March 2019, this version is a stable release. Of course, many improvements are planned, some things are still rough, and documentation is still in progress**

This system uses [xNode](https://github.com/Siccity/xNode), which is being enhanced by Siccity as part of this project.

<!-- TOC START min:2 max:3 link:true asterisk:false update:true -->
- [Description](#description)
- [Documents](#documents)
- [System Components](#system-components)
- [Current Features](#current-features)
- [Roadmap](#roadmap)
- [System Architecture](#system-architecture)
  - [Hardware](#hardware)
  - [Raspberry Pi Disk Image](#raspberry-pi-disk-image)
- [Getting Started](#getting-started)
  - [Starting the System Up](#starting-the-system-up)
  - [Robot Command Line Essentials](#robot-command-line-essentials)
  - [Installing The software](#installing-the-software)
<!-- TOC END -->

## Description

![system diagram](https://www.philvanallen.com/wp-content/uploads/2018/01/Pasted_Image_1_16_18__3_50_PM.jpg?resize=640%2C350)

The Delft AI Toolkit a system for designing smart things. It provides a visual authoring environment that incorporates machine learning, cognitive APIs, and other AI approaches, behavior trees, and data flow to create smart behavior in autonomous devices.

The goal of this project is to develop an approach to authoring AI that enables designers to easily and iteratively prototype smart things. This approach includes the ability to Wizard-of-Oz AI behaviors and simulate physical hardware in 3D, and then migrate these simulations to working prototypes that use machine learning and real hardware.

## Documents
* [Toolkit Documentation](docs/README.md)
* [ACM Interactions Article on Project](http://interactions.acm.org/archive/view/november-december-2018/prototyping-ways-of-prototyping-ai)
* [Short Project description](http://www.philvanallen.com/portfolio/delft-ai-toolkit/)

## System Components
* Authoring & Control System running on a computer
  * Visual Authoring with nodes in the Unity3D authoring environment
* Robot/Device
  * Raspberry Pi + Arduino (we may transition to the Adafruit Crikit for RPi once we have a chance to evaluate it - this would eliminate the need for the Arduino)
  * Motors, servos, sensors, LEDs, microphone, speaker, camera, etc.

Each of these has a codebase, and includes a range of libraries. **We are using and helping create an improved version of the open source [xNode Unity asset](https://github.com/Siccity/xNode)**

## Current Features
* **Action Types** - speak, listen, object recognize, servos, movement, leds, sensor input
* **Action Options** - repeat, random
* **Conditions** - numeric gt/lt/range, string start/end/contains/multiple
* **Behavior Trees/Data Flow** - multiple node ins/outs, loops, restart, individual node trigger for testing, visual indication of active node and action within node

## Roadmap

* More video and written documentation
* More and simpler hardware configurations
* DONE: Add OSC remote marionetting back in
* Add support for IBM Watson AI Cognitive Services (e.g. Assistant, Speech2Text, Text2Speech, Emotion, Sentiment, etc.)
* Add support for Snip.ai -- edge based speech assistant
* Better support for hardware sensors and IoT (e.g. IFTTT, webhooks)
* Integrate gesture recognition learning and classification
* Integrate Unity Reinforcement learning

## System Architecture
![hardware architecture](http://www.philvanallen.com/wp-content/uploads/2018/01/toolkit-architecture-diagram.jpg?resize=640%2C350)

### Hardware
The physical robot is currently based on a simple robot platform from Adafruit combined with a Raspberry Pi to perform the local edge AI, local text-to-speech, and make use of cloud APIs. The RPi talks over serial to an Arduino with a motor hat for the DC motors and Servos. The robot RPi communicates with Unity on the computer with the OSC network protocol.

Note: The next hardware version planned will eliminate the Arduino and replace it with the Adafruit [Crickit Hat](https://www.adafruit.com/product/3957) for the RPi. This will simplify hardware and software, and make a more compact robot.

[More details on the hardware](docs/hardware.md).

<img src="docs/images/robot3.jpg" width="512">

### Raspberry Pi Disk Image
* Download the [RPi disk image](https://www.dropbox.com/s/ory2ydrt6lkyrty/delft-toolkit-2019-03-03.dmg.zip?dl=0)
* Disk Image [Installation Details](docs/hardware.md#installing-the-delft-ai-toolkit-disk-image)

## Getting Started

### Starting the System Up
1. **Power the Robot**: Power on the Arduino and Raspberry Pi (RPi) in the following order:
     * **Motors**: Turn on the 6V AA battery pack (you can leave this off to disable the servos and wheel motors, or to save the batteries. The robot will work fine other than the motors)
     * **Arduino** Powered by the USB cable from the RPi
     * **RPi**: Connect a 5V 2A AC adapter, or the USB battery to the micro USB connector

1. **Get the WiFi IP address of the Robot**
     * **Your RPI must already connect by WiFi** - If you haven't already, [set up your RPI to connect to your local WiFi](https://www.raspberrypi.org/documentation/configuration/wireless/wireless-cli.md) using an ethernet cable.
     * **Connect by Ethernet if necessary** - Some networks don't allow connection by the .local (Bonjour) name. If so, hook up an ethernet cable between your computer and the Raspberry Pi (RPi) on the robot (you may need a [USB-C adapter](https://www.amazon.com/dp/B01M6WQ0T0/ref=twister_B06ZZLKBWJ)
     * **Login to the RPi** - Open a terminal window on your computer, and log into the RPi by typing in the below. Change the number at the end of delftbt1 to match the number of your robot if you changed the name.

     ```bash
     ssh pi@delftbt1.local
     ```
     * **Password** - The default password for the standard Delft AI Toolkit disk image is "adventures"
     * **Get the RPi WiFi IP Address** - Once logged in, copy and save the IP address of the RPi by typing the below. You'll see an entry for **"wlan0"** which is the WiFi connection - from there copy the IP address (e.g. 10.4.27.47)

     ```bash
    ifconfig
    ```
     * **Logout** - of the RPI by typing

     ```bash
     exit
     ```

     * **Disconnect** - Unplug the ethernet cable

1. **Start the Delft Toolkit software on the RPi**

     * **Power Motors** - If you are using the motors, turn on the 6V battery pack (or save the batteries for now, and turn the battery pack on when you are ready)
     * **Login to RPi** - In the terminal app log in to the RPi over WiFi by typing:

     ```bash
     ssh pi@10.4.27.47 # replace this IP address with the one you got from ifconfig above
     ```

     * **Toolkit Dirctory** - Once logged in, change directory to the delft-ai-toolkit directory:

     ```bash
     cd delft-ai-toolkit
     ```
     * **Start the software** - Type the below command, putting the IP address of your computer at the end for --server_ip (get the IP address of your computer by opening Network Preferences)

     ```bash
     python3 delft_toolkit.py --server_ip 10.4.18.109
     ```
     * **Startup Sequence** - The software will take a little time to start up. When it finishes, the robot will say "Hello."

     * **IMPORTANT: Powering off the RPi Procedure**: Before you disconnect the power from the RPi, you must properly shut it down with the following command. Wait for 10 seconds after the poweroff command, then it is safe to unplug the power from the RPi.

     ```bash
     sudo poweroff
     ```

1. **Start the software running in Unity**

      * **Open Project** - Open the "delft-toolkit" project in Unity3D (we've tested in version 2018.2.x)
      * **Open Scene** - In the Project, open the the scene that matches the toolkit graph you are using (e.g. Assets>Scenes>MainExamples)
      * **Open Visual Graph** - In the Project Assets>DelftToolkitGraphs directory, double click on the toolkit visual graph you are currently using (e.g. Assets>DelftToolkitGraphs>MainExamples)
      * **Physcial Robot** - If you are using the physical robot (the toolkit will work fine without the robot)
        * Click on the simulated robot in the Hierarchy (e.g. ding1), and in the inspector, enable the "Ding Control Physical" script. If you are not using the physical robot, keep this script unchecked and inactive.
        * Still in the inspector at "Ding Control Physical," paste in the IP address of the robot where it says "Target Addr"
        * <img src="docs/images/DingControlPhysical-IP.png" width="254">
      * **Play** - Click on the Unity **Play** button
      * **Start Graph** - In the xNode Toolkit graph pane, click on the "Start" node Trigger button to run the whole graph, or use Trigger on any individual node
        * **For keyboard or OSC** - Click on the Game pane (this is to ensure Unity is receiving all commands -- if you find it is not responding to the keyboard or OSC, click this pane)


### Robot Command Line Essentials

```bash
# login to the RPi via ethernet
ssh pi@delftbt0.local
# get the IP address of the RPi from the wlan0 section
ifconfig
# login to the RPi via WiFi, change the example IP to that RPi
ssh pi@10.4.27.47
# change to the toolkit software directory
cd delft-ai-toolkit
# start the RPi software, change the example IP to that of the computer running Unity
python3 delft_toolkit.py --server_ip 10.4.18.109
# shutdown before disconnecting the power, then wait for 10 seconds
sudo poweroff
```

### Installing The software

1. **Install [Unity3D](https://store.unity.com)**
1. **Download the toolkit software** and place on your computer. This includes the Unity project, RPi code, and Arduino code from [DelftToolkit](https://github.com/pvanallen/delft-toolkit-v2). **NOTE**: Because we use xNode as a submodule (i.e. we get the code from the original repo), a simple download or clone will leave the xNode code out and the directory for xNode will be empty: delft-toolkit>Assets>Scripts>delftToolkit>Submodules>xNode
     <br>There are two solutions for this:
     * **Clone** - Use the following command to clone the repo with the submodule included:
     <pre>git clone --recurse-submodules https://github.com/pvanallen/delft-toolkit-v2.git</pre>
     * **Download** - After you download the toolkit from our site with the download button, go to the [xNode repo](https://github.com/Siccity/xNode) and download xNode. Then place this xNode folder in the toolkit Unity project at delft-toolkit-v2>unity>delft-toolkit>Assets>Scripts>delftToolkit>Submodules>xNode
   * Disk image for [RPi]() - NEW VERSION NOT YET AVAILABLE
1. **Arduino**:
   * Install delftToolkit.ino on your Arduino with the Arduino IDE
1. **RPi**: Burn the RPi image to your SD card
   * [Set up your WiFi](https://www.raspberrypi.org/documentation/configuration/wireless/wireless-cli.md) with `sudo nano /etc/wpa_supplicant/wpa_supplicant.conf`
   * Change the hostname from the default of delftbt0 (e.g. delftbt1, delftbt2, etc.) if you are using more than one robot on your network
