# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com
# created with the help of TU Delft

# # TODO:
# test analog with new 3V proximity sensor

import time
import argparse
import os.path
import sys
import serial
import picamera
import serial.tools.list_ports
from pythonosc import dispatcher
from pythonosc import osc_server
from pythonosc import osc_message_builder
from pythonosc import udp_client

from threading import Thread
import multiprocessing
import socket

# import my libraries
import classify_pic_once as rec
import text_to_speech_pico as tts_pico
import text_to_speech_watson as tts_watson
import speech_to_text_watson as stt_watson
import play_wav as pw
from adafruit_crickit import crickit
import neopixel
from adafruit_seesaw.neopixel import NeoPixel

FLAGS = None
MCU = "CRICKIT"

events = ["move","leds","delay", "analogin", "servo", "speak", "listen", "chat"]
types = ["stop", "forward", "backward", "turnRight", "turnLeft", "set", "blink", "allOff", "pause", "start", "immediate", "varspeed", "male", "female", "timed", "auto", "standard"]
easings = ["none", "easeIn", "easeOut", "easeInOut"]

speak_task = False
speak_phrase = "hello world"
listen_task = False
listen_duration = 2

default_recognize_model = "squeezenet"

#CRICKET setup

#define the motors
motor_1 = crickit.dc_motor_1
motor_2 = crickit.dc_motor_2

# stop the motors
motor_1.throttle = 0.0
motor_2.throttle = 0.0

#NeoPixel
num_pixels = 16
# for blinking
blink = False
blink_state = False
blink_color = (127,0,0,0)
blink_delay = 0.1
blink_times = 2
blink_next_time = time.time() + blink_delay
# bpp=4 is required for RGBW
pixels = NeoPixel(crickit.seesaw, 20, num_pixels, brightness=0.02, pixel_order=neopixel.RGBW, bpp=4)
# black out the LEDs
pixels.fill((1,2,3,0)) # there's a bug in the neopixel lib that ignores zeros in rgbw
# https://github.com/adafruit/Adafruit_CircuitPython_seesaw/issues/32
# DEFINE sensors
# For signal control, we'll chat directly with seesaw, use 'ss' to shorted typing!
ss = crickit.seesaw

# analogin = False
analog_interval = .5
analog_next_time = time.time() + analog_interval
# ports to be scanned each interval
analog_ports = [False,False,False,False,False,False]


touch_interval = .5
touch_next_time = time.time() + touch_interval
# ports to be scanned each interval
touch_ports = [False,False,False,False,False]

move_stop_time = time.time()
move_stop_interval = 10.0 #seconds

def name_val(arr, name):
  if name in arr:
    return arr.index(name)
  else:
    return -1

def strip_adr(adr):
  return adr.replace("/", "")

def osc_loop():
  # runs as a thread waiting for incoming OSC messages
  # set up server
  server = osc_server.ThreadingOSCUDPServer((get_ip(), 5005), dispatcher)
  #server = osc_server.ThreadingOSCUDPServer(("127.0.0.1", 5005), dispatcher)
  print("Serving on {}".format(server.server_address))
  # blocks on this
  server.serve_forever()

def audio_output_loop(q):
  tts = None
  while True:
    command = q.get() # the queue has a tuple in it
    if command[0] == "init":
      print("Watson TTS initializing " + command[1])
      if command[1] == "watson":
        if tts == None:
          iamkey, url = command[2:4]
          tts = tts_watson.tts_watson(iamkey, url)
        else:
          print("Watson TTS Already Initialized ")
    elif command[0] == "speak":
      model, voice, utterance = command[1:4]
      if model == "pico":
        tts_pico.speak(utterance, voice)
        print("Pico Speaking... " + utterance)
      if model == "watson":
        if tts != None:
          tts.speak(utterance, voice)
          print("Watson Speaking... " + utterance)
        else:
          print("Can't speak, Watson not initialized...")
    elif command[0] == "playsound":
      filename = command[1]
      #print("Playing sound... " + filename)
      pw.play(filename)


def listen_loop(q):
  stt = None
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  watson_init = False
  watson_lang = ""
  while True:
    command = q.get()
    #time.sleep(0.1)
    print("got command: ",command)
    if command[0] == "init":
      if command[1] == "watson":
        if stt == None:
            iamkey, url = command[2:4]
            watson_lang = "enUS"
            timeout = -1
              # print("Watson STT initializing key: " + iamkey + " url: " + url)
            stt = stt_watson.stt_watson(iamkey, url, watson_lang, timeout)
            watson_init = True
        else:
          print("Watson STT Already Initialized ")
    elif command[0] == "transcribe":
      model, lang, time_limit = command[1:4]
      if model == "watson":
        if stt != None and watson_init and lang == watson_lang:
          print("request transcript")
          watson_lang = lang
          #print("Watson Transcribing... ")
          transcription = stt.transcribe(watson_lang, time_limit).replace("'","")
        else:
          # print("Can't transcribe, Watson not initialized...")
          # transcription = "Watson STT not initialized"
          print("Watson STT initializing key: " + iamkey + " url: " + url)
          stt.restart(watson_lang)
          transcription = stt.transcribe(watson_lang, time_limit).replace("'","")
          watson_init = True
        # transcription = sp.speech2text(duration).replace("'","")
        if (transcription != ""):
          client.send_message("/str/speech2text/", transcription)
          print("accepted final transcription: " + transcription)
        else:
          print("no transcription")
          client.send_message("/str/speech2text/", "no transcription")


def reconize_loop(q, e, FLAGS, model):
  #obj.take_picture_recognize.picture_being_taken= False
  camera = picamera.PiCamera()
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  print("server: " + FLAGS.server_ip)
  print("initializing recognition model...")
  rec.init(camera, model)
  e.set() # notify main process that model intialization is done
  while True:
    new_model = q.get()
    match_results = rec.run_inference_on_image(new_model)
    time.sleep(0.08)
    client.send_message("/str/recognize/", match_results)
    print("Obj recognition: " + match_results)

def get_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        # doesn't even have to be reachable
        s.connect(('8.8.8.8', 80))
        IP = s.getsockname()[0]
    except:
        IP = '127.0.0.1'
    finally:
        s.close()
    return IP

def move_cb(adr, type, move_time, speed, easing):
  global move_stop_time, move_stop_interval
  move_time = '%.5f'%(move_time) # Unity sends very long floats that upset the Arduino
  print("move: " + type + " " + str(move_time) + " " + str(speed) + " " +  easing)
  arduinoStr = '{},{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    move_time,
    speed,
    name_val(easings, easing)
  )
  if not send_serial_command(arduinoStr):
    # make sure the motor timeout is set before we start the motors
    move_stop_time = time.time() + move_stop_interval
    speed = float(speed)
    speed = max(-1, min(speed, 1)) # make sure the motor speed is between -1 and 1
    print("CRICKIT MOTOR speed: " + str(speed))
        # MOVE "stop", "forward", "backward", "turnRight", "turnLeft",
        # MOVE adr, type, time, speed, easing
    if type == "stop":
       left_val = 0.0
       right_val = 0.0
    elif type == "forward":
       left_val = 1.0 * speed
       right_val = 1.0 * speed
    elif type == "backward":
       left_val = -1.0 * speed
       right_val = -1.0 * speed
    elif type == "turnRight":
       left_val = 1.0 * speed
       right_val = -1.0 * speed
    elif type == "turnLeft":
       left_val = -1.0 * speed
       right_val = 1.0 * speed
    else: # stop
       type = type + " Unknown Type"
       left_val = 0
       right_val = 0

    motor_1.throttle = left_val
    motor_2.throttle = right_val

    print("Move TYPE: " + type + " L=" + str(left_val) + " R=" + str(right_val),move_stop_time)



def leds_cb(adr, type, dly_time, lednum, color):
  global blink, blink_color, blink_delay, blink_next_time, blink_state, blink_times
  dly_time = '%.5f'%(dly_time) # Unity sends very long floats that upset the Arduino
  #print("leds: " + type + " " +  str(dly_time) + " " + str(lednum) + " " +  color)
  arduinoStr = '{},{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    dly_time,
    lednum,
    color
  )
  if not send_serial_command(arduinoStr):
    red = int(color.split(',')[0])
    green = int(color.split(',')[1])
    blue = int(color.split(',')[2])
    white = 0
    set_color = (green, red, blue, white) #rgb r& g are reversed
    #print("led command...",set_color)

    #print("type: " + type + " " + str(name_val(types, "set")))
    if type == "set":
      #print("leds set")
      if lednum == -1:
        # set all the leds to the same color
        pixels.fill((set_color))
      elif lednum > 0 and lednum < num_pixels:
        pixels[lednum] = set_color
    #elif type == name_val(types, "allOff"):
    elif type == "allOff":
      print("leds allOff")
      pixels.fill((0,0,0,0))
    elif type == "blink":
      print("leds set blink...")
      blink_delay = float(dly_time)
      blink_next_time = time.time()
      blink = True
      blink_color = set_color
      blink_state = False
      blink_times = (lednum  * 2)

      #pixels.fill((color))
      #print(blink, blink_color, blink_delay, blink_next_time, blink_state, blink_times)


def delay_cb(adr, type, time):
  #print("delay: " + type + " " +  str(time))
  time = '%.5f'%(time) # Unity sends very long floats that upset the Arduino
  arduinoStr = '{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    time
  )
  if ser != None: ser.write(arduinoStr.encode())

def analogin_cb(adr, type, interval, port):
  global analog_ports, analog_interval
  port = max(0, min(port, 5)) # make sure port is between 0 & 5
  print("analogin: " + type + " interval: " +  str(interval) + " port: " + str(port))
  arduinoStr = '{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    interval,
    port
  )
  if not send_serial_command(arduinoStr):
      if type == "start":
          analog_ports[port] = True
          analog_interval = interval * 0.01
      else:
          analog_ports[port] = False

def touch_cb(adr, type, interval, port):
  global touch_ports, touch_interval
  port = max(1, min(port, 4)) # make sure port is between 1 & 4 -- the Adafruit CRICKIT is labeled 1,2,3,4
  print("touch: " + type + " interval: " +  str(interval) + " port: " + str(port))
  arduinoStr = '{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    interval,
    port
  )
  if not send_serial_command(arduinoStr):
      if type == "start":
          touch_ports[port] = True
          touch_interval = interval * 0.01
      else:
          touch_ports[port] = False

def servo_cb(adr, type, angle, port, varspeed, easing):
  print("servo: " + type + " " +  str(angle) + " port: " + str(port))
  arduinoStr = '{},{},{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    angle,
    port,
    varspeed,
    name_val(easings, easing)
  )
  if not send_serial_command(arduinoStr):
    port = str(port)
    print("CRICKIT SERVO: " + port)
    if port == "9": # first servo out
        realport = crickit.servo_1
    elif port == "10": # second servo out
        realport = crickit.servo_2
    elif port == "3":
        realport = crickit.servo_3
    elif port == "4":
        realport = crickit.servo_4
    else:
        realport = crickit.servo_1
    realport.angle = int(angle)

def crickit_servo(str):
  adr, type, angle, port, varspeed, easing = str.split()

def play_sound_cb(adr, filename, time):
  audio_output_q.put(("playsound",filename))

def listen_cb(adr, model, lang, duration):
  listen_q.put(("transcribe", model, lang, duration))

def initstt_cb(adr, model, iamkey, url):
  listen_q.put(("init", model, iamkey, url))

def speak_cb(adr, model, voice, utterance):
  print("speak: " + utterance)
  audio_output_q.put(("speak", model, voice, utterance))

def inittts_cb(adr, model, iamkey, url):
  audio_output_q.put(("init", model, iamkey, url))

def recognize_cb(adr, type, model):
  print("received cmd recognize: " + adr + " " + type + " " + model)
  recognize_q.put(model)

def main(_):
  global ser, blink, blink_state, blink_delay, blink_next_time, blink_color, blink_times
  global analogin, analog_ports, analog_interval, analog_next_time, move_stop_time
  global touch_ports, touch_interval, touch_next_time
  count = 0.0;
  while True:
      # print("touch",touch_ports,touch_next_time, check_touch())
      # shut down any motor moves after move_stop_time
      if time.time() > move_stop_time:
          if motor_1.throttle > 0 or motor_2.throttle > 0:
              motor_1.throttle = 0
              motor_2.throttle = 0
              print("#########TIMEOUT -- STOPPING MOTORS")
              #print(time.time(),move_stop_time, move_stop_time - time.time())
      #### BLINK
      if blink == True:
        #print("blink ON " + str(time.time()))
        if time.time() > blink_next_time and blink_times > 0:
          #print("blink... times=" + str(blink_times) + " next time=" + str(blink_next_time))
          if blink_state:
            pixels.fill((0,0,0,0)) #OFF
            #print("OFF")
          else:
            print(str(blink_times / 2) + " " + "ON")
            pixels.fill(blink_color) #ON
          blink_next_time = time.time() + blink_delay
          blink_times = blink_times - 1
          blink_state = not blink_state
        elif time.time() <= blink_next_time and blink_times < 1:
            #print("blink DONE")
            blink = False
            pixels.fill((0,0,0,0)) #OFF

      #### ANALOGIN
      # the interval is the same for all ports -- maybe have a separate array for intervals?
      if time.time() > analog_next_time and check_analog():
        analog_next_time = time.time() + analog_interval
        for i, read in enumerate(analog_ports):
            if analog_ports[i] == True:
                sensor = crickit.SIGNAL1
                if i == 0:
                    sensor = crickit.SIGNAL1
                elif i == 1:
                    sensor = crickit.SIGNAL2
                elif i == 2:
                    sensor = crickit.SIGNAL3
                elif i == 3:
                    sensor = crickit.SIGNAL4
                elif i == 4:
                    sensor = crickit.SIGNAL5
                elif i == 5:
                    sensor = crickit.SIGNAL6
                analog_value = float(ss.analog_read(sensor))
                osc_address="/num/analogin/" + str(i) + "/"

                print(osc_address + " :",i,analog_value, "analog interval:",analog_interval)
                builder = osc_message_builder.OscMessageBuilder(address = osc_address)
                builder.add_arg(analog_value)
                builder.add_arg(100)
                builder.add_arg(999)
                msg = builder.build()
                client.send(msg)

        #### TOUCH
      # print("touch",touch_ports,touch_next_time, check_touch())
      if time.time() > touch_next_time and check_touch():
        touch_next_time = time.time() + touch_interval
        for i, read in enumerate(touch_ports):
          if touch_ports[i] == True:
              sensor = crickit.touch_1
              if i == 1:
                  sensor = crickit.touch_1
              elif i == 2:
                  sensor = crickit.touch_2
              elif i == 3:
                  sensor = crickit.touch_3
              elif i == 4:
                  sensor = crickit.touch_4
              if sensor.value: # check if the touch port is active from a touch
                  touch_value = 1023
              else:
                  touch_value = 0
              # print("touch read, port:",i,touch_value, "touch interval:",touch_interval)

              osc_address="/num/touch/" + str(i) + "/"
              builder = osc_message_builder.OscMessageBuilder(address = osc_address)
              builder.add_arg(touch_value)
              builder.add_arg(100)
              builder.add_arg(999)
              msg = builder.build()
              client.send(msg)

      # handle incoming messages from Arduino
      elif MCU == "ARDUINO" and ser != None:
        if ser.inWaiting() > 0:
          # messages are in pseudo OSC format
          line = ser.readline().decode("utf-8")
          vals = line.split(' ')
          if vals[0].startswith('/num/analogin/'):
            # messages come in the form /num/analogIn/0/ 0 0 0 - where the last number in the url is the port
            #print ("Arduino message:" + line)
            builder = osc_message_builder.OscMessageBuilder(address=vals[0])
            builder.add_arg(float(vals[1]))
            builder.add_arg(float(vals[2]))
            builder.add_arg(float(vals[3]))
            msg = builder.build()
            client.send(msg)
          else:
            print ("unknown Arduino message:" + line)
      elif MCU == "ARDUINO":
        # send fake sensor numbers
        count += 1
        if count > 100: count = 0
        builder = osc_message_builder.OscMessageBuilder(address="/num/analogin/0/")
        builder.add_arg(count)
        builder.add_arg(count + 1)
        builder.add_arg(count + 2)
        msg = builder.build()
        client.send(msg)
        time.sleep(0.1)
        # blink leds
        time.sleep(0.01)

def check_analog():
    # are there any ports active?
    for port in analog_ports:
        if port:
            return True
    return False

def check_touch():
    # are there any ports active?
    for port in touch_ports:
        if port:
            return True
    return False

def reset_analog():
    for i, read in enumerate(analog_ports):
        analog_ports[i] = False
    return True

def send_serial_command(str_arg):
  #print("send_serial_command: " + str_arg)
  if (MCU == "ARDUINO"):
    str_e = str_arg.encode()
    # send out by USB serial port to ARDUINO, # otherwise talk directly to CRICKIT
    if ser != None: ser.write(str_e)
    return True
  else:
    return False

if __name__ == '__main__':
  parser = argparse.ArgumentParser()

  print("Delft Toolkit Initializing...")
  print("network: " + socket.gethostname() + " " + get_ip())
  parser.add_argument(
      '--server_ip',
      type=str,
      default='127.0.0.1',
      help='IP of server to where Unity is running'
  )

  parser.add_argument(
      '--usb',
      type=str,
      default='/dev/ttyACM0',
      help='serial port name for the arduino'
  )

  FLAGS, unparsed = parser.parse_known_args()

  # set up handlers for incoming OSC messages
  dispatcher = dispatcher.Dispatcher()
  dispatcher.map("/move/", move_cb)
  dispatcher.map("/leds/", leds_cb)
  dispatcher.map("/delay/", delay_cb)
  dispatcher.map("/analogin/", analogin_cb)
  dispatcher.map("/touch/", touch_cb)
  dispatcher.map("/servo/", servo_cb)
  dispatcher.map("/textToSpeech/", speak_cb)
  dispatcher.map("/inittts/", inittts_cb)
  dispatcher.map("/speechToText/", listen_cb)
  dispatcher.map("/initstt/", initstt_cb)
  dispatcher.map("/recognize/", recognize_cb)
  dispatcher.map("/playSound/", play_sound_cb)

  # setup USB Port for connection to Arduino
  if MCU == "ARDUINO":
      try:
        ser = serial.Serial(FLAGS.usb, baudrate=115200,
                        parity=serial.PARITY_NONE,
                        stopbits=serial.STOPBITS_ONE,
                        bytesize=serial.EIGHTBITS,
                        timeout=1
                        )
        print("Connected to USB port: " + FLAGS.usb)
      except:
        ser = None
        comlist = serial.tools.list_ports.comports()
        connected = []
        for element in comlist:
            connected.append(element.device)
        print("Can't connect to USB port: " + FLAGS.usb + ", Available USB ports: " + str(connected))
  # set up camera
  #camera = picamera.PiCamera()
  picture_ready = False
  picture_being_taken = False

  # turn off all analog ports
  analogin_cb("/analogin/", "stop", 50, -1)
  #############ADD TURN OFF ALL MOTORS/SERVOS

  # use thread to handle incoming OSC messages from Unity
  osc_thread = Thread(target=osc_loop,args=())
  osc_thread.start() # run in background as a thread

  # Events for multiprocessing
  recognition_ready_e = multiprocessing.Event()

  # Queues for multiprocessing
  audio_output_q = multiprocessing.Queue()
  listen_q = multiprocessing.Queue()
  recognize_q = multiprocessing.Queue()

  # launch processes
  audio_output_process = multiprocessing.Process(name='audio_output_process',
                               target=audio_output_loop,
                               args=(audio_output_q,))

  listen_process = multiprocessing.Process(name='listen_process',
                               target=listen_loop,
                               args=(listen_q,))

  recognize_process = multiprocessing.Process(name='recognize_process',
                               target=reconize_loop,
                               args=(recognize_q,recognition_ready_e,FLAGS, default_recognize_model))

  recognize_process.start()
  audio_output_process.start()
  listen_process.start()

  # wait for model init to finish before waiting for commands
  recognition_ready_e.wait()

  audio_output_q.put(("speak","pico","GB","hello"))
  print("Delft Toolkit Initialization Complete")
  # blink leds
  leds_cb("/leds", "blink", 0.1, 10, "0,0,127")

  analogin = False

  # set up OSC client
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  main(sys.argv)
  #run(main=main, argv=[sys.argv[0]] + unparsed)
