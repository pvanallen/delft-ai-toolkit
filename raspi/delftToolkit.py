# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com
# created with the help of TU Delft

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
import pirecognize as obj
import pispeech as sp

FLAGS = None

events = ["move","leds","delay", "analogin", "servo", "speak", "listen", "chat"]
types = ["stop", "forward", "backward", "turnRight", "turnLeft", "set", "blink", "allOff", "pause", "start", "immediate", "varspeed", "male", "female", "timed", "auto", "standard"]
easings = ["none", "easeIn", "easeOut", "easeInOut"]

speak_task = False
speak_phrase = "hello world"
listen_task = False
listen_duration = 2



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

def speak_loop(q):
  while True:
    speak_phrase = q.get()
    print(speak_phrase)
    sp.speak(speak_phrase)

def listen_loop(q):
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  while True:
    duration = q.get()
    sp.speak("Speak")
    time.sleep(0.1)
    transcription = sp.speech2text(duration).replace("'","")
    if (transcription != ""):
      #sp.speak(transcription)
      client.send_message("/str/speech2text/", transcription)
    else:
      print("no transcription")
      #sp.speak("no transcription")
      client.send_message("/str/speech2text/", "no transcription")

def reconize_loop(q, e, flags):

  FLAGS = flags
  camera = picamera.PiCamera()
  obj.take_picture_recognize.picture_being_taken= False
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  obj.obj_init(FLAGS)
  e.set() # notify main process that model intialization is done
  while True:
    option = q.get()
    match_results = obj.take_picture_recognize(FLAGS, camera)
    #time.sleep(0.1)
    client.send_message("/str/recognize/", match_results)
    print("Obj recognition: " + match_results)

def reconize_loop_opencv(q, e, flags):

  FLAGS = flags
  camera = picamera.PiCamera()
  obj.take_picture_recognize.picture_being_taken= False
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  obj.obj_init(FLAGS)
  e.set() # notify main process that model intialization is done
  while True:
    option = q.get()
    match_results = obj.take_picture_recognize(FLAGS, camera)
    #time.sleep(0.1)
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

def move_cb(adr, type, time, speed, easing):
  #print("move: " + type + " " + str(time) + " " + str(speed) + " " +  easing)
  arduinoStr = '{},{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    time,
    speed,
    name_val(easings, easing)
  )
  #print(arduinoStr)
  if ser != None: ser.write(arduinoStr.encode())

def leds_cb(adr, type, time, num, color):
  #print("leds: " + type + " " +  str(time) + " " + str(num) + " " +  color)
  arduinoStr = '{},{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    time,
    num,
    color
  )
  #print(arduinoStr)
  if ser != None: ser.write(arduinoStr.encode())

def delay_cb(adr, type, time):
  #print("delay: " + type + " " +  str(time))
  arduinoStr = '{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    time
  )
  if ser != None: ser.write(arduinoStr.encode())

def analogin_cb(adr, type, interval, port):
  #print("delay: " + type + " " +  str(time))
  arduinoStr = '{},{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    interval,
    port,
    name_val(easings, easing)
  )
  print(arduinoStr)
  if ser != None: ser.write(arduinoStr.encode())

def servo_cb(adr, type, angle, port, varspeed, easing):
  #print("servo: " + type + " " +  str(angle) + "port: " + str(port))
  arduinoStr = '{},{},{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    angle,
    port,
    varspeed,
    name_val(easings, easing)
  )
  #print("servo: " + arduinoStr)
  if ser != None: ser.write(arduinoStr.encode())

def listen_cb(adr, type, duration):
  listen_q.put(duration)

def speak_cb(adr, type, utterance):
  speak_q.put(utterance)

def recognize_cb(adr, type):
  recognize_q.put(type)

def main(_):
  global ser
  count = 0.0;
  #recognize_q.put("test")
  # handle incoming messages from Arduino
  while True:
    if ser != None:
      if ser.inWaiting() > 0:
        # messages are in pseudo OSC format
        line = ser.readline().decode("utf-8")
        vals = line.split(' ')
        if vals[0] == "/num/analogIn/0/":
          builder = osc_message_builder.OscMessageBuilder(address=vals[0])
          builder.add_arg(float(vals[1]))
          builder.add_arg(float(vals[2]))
          builder.add_arg(float(vals[3]))
          msg = builder.build()
          client.send(msg)
        else:
          print (line)
    else:
      # send fake numbers
      count += 1
      if count > 100: count = 0
      builder = osc_message_builder.OscMessageBuilder(address="/num/analogIn/0")
      builder.add_arg(count)
      builder.add_arg(count + 1)
      builder.add_arg(count + 2)
      msg = builder.build()
      client.send(msg)
      time.sleep(0.2)

    time.sleep(0.01)


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

  parser.add_argument(
      '--model_dir',
      type=str,
      default='/home/pi/inception',
      help="""\
      Path to classify_image_graph_def.pb,
      imagenet_synset_to_human_label_map.txt, and
      imagenet_2012_challenge_label_map_proto.pbtxt.\
      """
  )

  parser.add_argument(
      '--num_top_predictions',
      type=int,
      default=5,
      help='Display this many predictions.'
  )

  FLAGS, unparsed = parser.parse_known_args()

  # set up handlers for incoming OSC messages
  dispatcher = dispatcher.Dispatcher()
  dispatcher.map("/move/", move_cb)
  dispatcher.map("/leds/", leds_cb)
  dispatcher.map("/delay/", delay_cb)
  dispatcher.map("/analogin/", analogin_cb)
  dispatcher.map("/servo/", servo_cb)
  dispatcher.map("/speak/", speak_cb)
  dispatcher.map("/listen/", listen_cb)
  dispatcher.map("/recognize/", recognize_cb)

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
    print("Can't connect to USB port: " + FLAGS.usb)
    comlist = serial.tools.list_ports.comports()
    connected = []
    for element in comlist:
        connected.append(element.device)
    print("Available USB ports: " + str(connected))
  # set up camera
  #camera = picamera.PiCamera()
  # picture_ready = False
  # picture_being_taken = False

  # use thread to handle incoming OSC messages from Unity
  osc_thread = Thread(target=osc_loop,args=())
  osc_thread.start() # run in background as a thread

  # Events for multiprocessing
  recognition_ready_e = multiprocessing.Event()

  # Queues for multiprocessing
  speak_q = multiprocessing.Queue()
  listen_q = multiprocessing.Queue()
  recognize_q = multiprocessing.Queue()

  # launch processes
  speak_process = multiprocessing.Process(name='speak_process',
                               target=speak_loop,
                               args=(speak_q,))

  listen_process = multiprocessing.Process(name='listen_process',
                               target=listen_loop,
                               args=(listen_q,))

  recognize_process = multiprocessing.Process(name='listen_process',
                               target=reconize_loop,
                               args=(recognize_q,recognition_ready_e,FLAGS))

  recognize_process.start()
  speak_process.start()
  listen_process.start()

  # wait for tensorflow init to finish before waiting for commands
  recognition_ready_e.wait()

  print("Delft Toolkit Initializaiton Complete")
  speak_q.put("Hello")

  # set up OSC client
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  main(sys.argv)
  #run(main=main, argv=[sys.argv[0]] + unparsed)
