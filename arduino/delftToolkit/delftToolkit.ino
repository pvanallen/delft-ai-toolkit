/*
create constants for each serial type
use serial event to build command, then indicate command ready
run command


*/

//events = ["move","leds","delay"]
//
//types = ["stop", "forward", "backward", "turnRight", "turnLeft", "set", "allOff", "pause"]
//
//easings = ["none", "easeIn", "easeOut", "easeInOut"]

#include "CurieIMU.h"
#include <TaskScheduler.h>
#include <Adafruit_MotorShield.h>
#include <Adafruit_NeoPixel.h>
#include <Servo.h>
#include <math.h>

// set up constants
// events
const short EV_MOVE = 0;
const short EV_LEDS = 1;
const short EV_DELAY = 2;
const short EV_ANALOGIN = 3;
const short EV_SERVO = 4;

// types
const short TY_STOP = 0;
const short TY_FORWARD = 1;
const short TY_BACKWARD = 2;
const short TY_TURNRIGHT = 3;
const short TY_TURNLEFT = 4;
const short TY_SET = 5;
const short TY_BLINK = 6;
const short TY_ALLOFF = 7;
const short TY_PAUSE = 8;
const short TY_START = 9;
const short TY_IMMEDIATE = 10; // servo
const short TY_VARSPEED = 11; // servo

// easing
const short EA_NONE = 0;
const short EA_IN = 1;
const short EA_OUT = 2;
const short EA_INOUT = 3;

// motor normal speed
int normalSpeed = 64; // 0-255

// command event variables
bool eventReady = false;

short event = EV_MOVE;
short type = TY_STOP;
float duration = 1.0;
float moveSpeed = 1.0;
short ledNum = 0;
short easing = EA_NONE;
short colorR = 127;
short colorG = 127;
short colorB = 127;
short angle = 90;
short port = 9;
short varspeed = 0;


// NeoPixel setup
//
#define NEOPIN 6
#define NUMPIXELS 12
//Adafruit_NeoPixel pixel = Adafruit_NeoPixel(NUMPIXELS, NEOPIN, NEO_GRB + NEO_KHZ800 );
Adafruit_NeoPixel pixel = Adafruit_NeoPixel(NUMPIXELS, NEOPIN, NEO_RGBW + NEO_KHZ800 );

// TaskScheduler Setup
//

// Callback methods prototypes
void transmitSensors();
//void waitForImu();
//void transmitImu();
//void runMotors();
void blinkLeds();

// Tasks
Task tTransmitSensors(20, TASK_FOREVER, &transmitSensors);
//Task tWaitForImu(10, TASK_FOREVER, &waitForImu);
//Task tTransmitImu(20, 75, &transmitImu);
//Task tRunMotors(20, TASK_FOREVER, &runMotors);
Task tBlinkLeds(250, 5, &blinkLeds);

Scheduler runner;

//int analogPort = 0;
short analogPorts[] = {0,0,0,0,0,0};
byte analogPortPins[] = {A0,A1,A2,A3,A4,A5};
bool blinkState = false;
int blinkR = 60;
int blinkG = 0;
int blinkB = 127;

// Create the motor shield object with the default I2C address
//
Adafruit_MotorShield AFMS = Adafruit_MotorShield();
// And connect 2 DC motors to port M1 & M2
Adafruit_DCMotor *L_MOTOR = AFMS.getMotor(1);
Adafruit_DCMotor *R_MOTOR = AFMS.getMotor(2);

// Setup Servos
Servo tilt;
Servo pan;

//const SERVOLOW = 1000;
//const SERVOHIGH = 2000;
//
//int servo1Target = 1000;
//int servo2Target = 1000;

void setup() {
  Serial.begin(115200);

  // Init TaskScheduler
  runner.init();
//  runner.addTask(tWaitForImu);
//  runner.addTask(tTransmitImu);
  runner.addTask(tTransmitSensors);
  runner.addTask(tBlinkLeds);

  // Motor setup
  AFMS.begin();  // create with the default frequency 1.6KHz
  // turn off both motors
  L_MOTOR->setSpeed(0);
  R_MOTOR->setSpeed(0);
  L_MOTOR->run(RELEASE);
  R_MOTOR->run(RELEASE);

  // Attach servo
  tilt.attach(9);
  pan.attach(10);

  // Setup the neopixel
  pixel.begin();
  pixel.setBrightness(127); //medium brightness
  pixel.show();

  tilt.write(60);
  delay(200);
  pan.write(35);
  delay(200);
  pan.write(120);
  delay(200);
  pan.write(90);
  delay(200);
  tilt.write(120);
  startBlinkLeds(120,4,127,0,0);
  
}

void loop() {
  if (eventReady) {
//    Serial.print("event: ");
//    Serial.print(event);
//    Serial.print('\n');
    eventReady = false;
  }
  
  // run TaskScheduler
  runner.execute();
}

void serialEvent() {
  //Serial.println("got serial");
  int interval = 20;
  int port = 0;
  if (Serial.available()) {
    // get the event and type
    event = Serial.parseInt();
    type = Serial.parseInt();

//    Serial.print("event: ");
//    Serial.print(event);
//    Serial.print(" type: ");
//    Serial.println(type);
    
    switch (event) {
      case EV_MOVE:
        duration = Serial.parseFloat();
        moveSpeed = Serial.parseFloat();
        easing = Serial.parseInt();

        switch(type) {
          case TY_STOP:
            L_MOTOR->run(RELEASE);
            R_MOTOR->run(RELEASE);
            break;
          case TY_FORWARD:
            L_MOTOR->run(FORWARD);
            R_MOTOR->run(FORWARD);
            break;
          case TY_BACKWARD:
            L_MOTOR->run(BACKWARD);
            R_MOTOR->run(BACKWARD);
            break;
          case TY_TURNRIGHT:
            L_MOTOR->run(FORWARD);
            R_MOTOR->run(BACKWARD);
            break;
          case TY_TURNLEFT:
            L_MOTOR->run(BACKWARD);
            R_MOTOR->run(FORWARD);
            break;
          default:
            L_MOTOR->run(RELEASE);
            R_MOTOR->run(RELEASE);
            type = TY_STOP;
            break;
        }

        if (type == TY_STOP) {
          L_MOTOR->setSpeed(0);
          R_MOTOR->setSpeed(0);
        } else {
          int motorSpeed = constrain(round(normalSpeed * moveSpeed), 0, 255);
          L_MOTOR->setSpeed(motorSpeed);
          R_MOTOR->setSpeed(motorSpeed);
        }
        
        break;
      case EV_LEDS:
        duration = Serial.parseFloat();
        ledNum = Serial.parseInt();
        colorR = Serial.parseInt();
        colorG = Serial.parseInt();
        colorB = Serial.parseInt();

        if (type == TY_SET) {
          setLedsColor(ledNum, colorR, colorG, colorB);
        } else if (type == TY_BLINK) {
          int delayTime = round((duration * 1000.00));
          startBlinkLeds(delayTime,ledNum,colorR,colorG,colorB);
        } else { // all off
          setLedsColor(-1, 0, 0, 0);
        }
        
        break;
      case EV_DELAY:
        duration = Serial.parseFloat();
        break;
      case EV_ANALOGIN:
        interval = Serial.parseInt();
        port = Serial.parseInt();
        //Serial.print(event);Serial.print(type);Serial.print(interval);Serial.println(port);
        if (type == TY_START) {
          configureSensors(true,interval,port);
        } else if (type == TY_STOP) { // stop this sensor
          configureSensors(false,interval,port);
        } else {
          configureSensors(false,interval,-1);
        }
        break;
      case EV_SERVO:
        angle = Serial.parseInt();
        port = Serial.parseInt();
        varspeed = Serial.parseInt();
        easing = Serial.parseInt();
//        Serial.print(" angle: ");
//        Serial.print(angle);
//        Serial.print(" port: ");
//        Serial.println(port);
        if (type == TY_IMMEDIATE) {
          if (port == 9) {
            tilt.write(angle);
          } else if (port == 10) {
            pan.write(angle);
          }
        } else {
          // varSpeedServo code using move_rate & easing
        }
        break;
      default:
        break;
    }
    char lineEnd = (char)Serial.read();
    if (lineEnd == '\n') {
      eventReady = true;
    }
  }
}

void setLedsColor(int num, short r, short g, short b) {
  r = constrain(r,0,255);
  g = constrain(g,0,255);
  b = constrain(b,0,255);
  if (num == -1) {
    for (int i = 0; i < NUMPIXELS; i++) {
      pixel.setPixelColor(i, g, r, b,0);
    }
  } else {
    num = constrain(num,0,NUMPIXELS-1);
    pixel.setPixelColor(num, g, r, b,0);
  }
  pixel.show();
}

void blinkLeds () {
  if (blinkState) {
    setLedsColor(-1,0,0,0);
    //Serial.println("leds off");
  } else {
    setLedsColor(-1,blinkR,blinkG,blinkB);
    //Serial.println("Leds on");
  }
  blinkState = !blinkState;
}

void startBlinkLeds(int ms, int blinks, int r, int g, int b) {
  blinkState = false;
  blinkR = r; blinkG = g; blinkB = b;
  tBlinkLeds.setInterval(ms);
  tBlinkLeds.setIterations(blinks*2);
  tBlinkLeds.restart();
}

void transmitSensors() {
  int value = 0;
  for (unsigned int i = 0; i < (sizeof(analogPorts) / sizeof(analogPorts[0])); i++) {
    if (analogPorts[i] == 1) {
      value = analogRead(analogPortPins[i]);
      Serial.print("/num/analogin/");
      Serial.print(i);
      Serial.print("/ ");
      Serial.print(value);
      Serial.print(" 0 0\n");
    }
    delay(5);
  }
}

void configureSensors(bool start, int interval, int port) {
  if (start) {
    tTransmitSensors.setInterval(interval);
    tTransmitSensors.setIterations(TASK_FOREVER);
    analogPorts[port] = 1;
    tTransmitSensors.restart();
  } else {
    bool portsInUse = false;
    if (port < 0) { // shut all ports off
      for (unsigned int i = 0; i < (sizeof(analogPorts) / sizeof(analogPorts[0])); i++) {
        analogPorts[i] = 0;
      }
    } else { // stop this port
      analogPorts[port] = 0;
      for (unsigned int i = 0; i < (sizeof(analogPorts) / sizeof(analogPorts[0])); i++) {
        if (analogPorts[i] == 1) {
          portsInUse = true;
        }
      }
    }
    if (!portsInUse) {
      tTransmitSensors.disable();
    }
  }
}

