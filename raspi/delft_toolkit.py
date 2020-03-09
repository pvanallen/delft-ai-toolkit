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
import classify_pic_once as rec
import text_to_speech_pico as tts_pico
import text_to_speech_watson as tts_watson
import speech_to_text_watson as stt_watson
import play_wav as pw

FLAGS = None

events = ["move","leds","delay", "analogin", "servo", "speak", "listen", "chat"]
types = ["stop", "forward", "backward", "turnRight", "turnLeft", "set", "blink", "allOff", "pause", "start", "immediate", "varspeed", "male", "female", "timed", "auto", "standard"]
easings = ["none", "easeIn", "easeOut", "easeInOut"]

speak_task = False
speak_phrase = "hello world"
listen_task = False
listen_duration = 2

default_recognize_model = "squeezenet"

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
      print("TTS initializing " + command[1])
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
      pw.play(filename)
      print("Playing sound... " + filename)

def listen_loop(q):
  stt = None
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  while True:
    command = q.get()
    #time.sleep(0.1)
    if command[0] == "init":
      print("STT initializing " + command[1])
      if command[1] == "watson":
        if stt == None:
          iamkey, url = command[2:4]
          stt = stt_watson.stt_watson(iamkey, url)
        else:
          print("Watson STT Already Initialized ")
    elif command[0] == "transcribe":
      model, lang, time_limit = command[1:4]
      if model == "watson":
        if stt != None:
          #print("Watson Transcribing... ")
          transcription = stt.transcribe(lang, time_limit)
        else:
          print("Can't transcribe, Watson not initialized...")
          transcription = "Watson STT not initialized"
        # transcription = sp.speech2text(duration).replace("'","")
        print("final transcription: " + transcription) 
        if (transcription != ""):
          client.send_message("/str/speech2text/", transcription)
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

def move_cb(adr, type, time, speed, easing):
  time = '%.5f'%(time) # Unity sends very long floats that upset the Arduino
  print("move: " + type + " " + str(time) + " " + str(speed) + " " +  easing)
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
  time = '%.5f'%(time) # Unity sends very long floats that upset the Arduino
  print("leds: " + type + " " +  str(time) + " " + str(num) + " " +  color)
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
  time = '%.5f'%(time) # Unity sends very long floats that upset the Arduino
  arduinoStr = '{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    time
  )
  if ser != None: ser.write(arduinoStr.encode())

def analogin_cb(adr, type, interval, port):
  print("analogin: " + type + " interval: " +  str(interval) + " port: " + str(port))
  arduinoStr = '{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    interval,
    port
  )
  #print(arduinoStr)
  if ser != None: ser.write(arduinoStr.encode())

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
  #print("servo: " + arduinoStr)
  if ser != None: ser.write(arduinoStr.encode())

def listen_cb(adr, model, lang, duration):
  listen_q.put(("transcribe", model, lang, duration))

def initstt_cb(adr, model, iamkey, url):
  listen_q.put(("init", model, iamkey, url))

def play_sound_cb(adr, filename, time):
  audio_output_q.put(("playsound",filename))

def speak_cb(adr, model, voice, utterance):
  print("speak: " + utterance)
  audio_output_q.put(("speak", model, voice, utterance))

def inittts_cb(adr, model, iamkey, url):
  audio_output_q.put(("init", model, iamkey, url))

def recognize_cb(adr, type, model):
  print("received cmd recognize: " + adr + " " + type + " " + model)
  recognize_q.put(model)

def main(_):
  global ser
  count = 0.0;
  # handle incoming messages from Arduino
  while True:
    if ser != None:
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
    else:
      # send fake numbers
      count += 1
      if count > 100: count = 0
      builder = osc_message_builder.OscMessageBuilder(address="/num/analogin/0/")
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

  FLAGS, unparsed = parser.parse_known_args()

  # set up handlers for incoming OSC messages
  dispatcher = dispatcher.Dispatcher()
  dispatcher.map("/move/", move_cb)
  dispatcher.map("/leds/", leds_cb)
  dispatcher.map("/delay/", delay_cb)
  dispatcher.map("/analogin/", analogin_cb)
  dispatcher.map("/servo/", servo_cb)
  dispatcher.map("/textToSpeech/", speak_cb)
  dispatcher.map("/inittts/", inittts_cb)
  dispatcher.map("/speechToText/", listen_cb)
  dispatcher.map("/initstt/", initstt_cb)
  dispatcher.map("/recognize/", recognize_cb)
  dispatcher.map("/playSound/", play_sound_cb)


  # setup USB Port for connection to Arduino
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

  # turn off all analog ports
  analogin_cb("/analogin/", "stop", 50, -1)
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

  print("Delft Toolkit Initialization Complete")
  audio_output_q.put(("speak","pico","enUS1","Hello"))

  # set up OSC client
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)
  main(sys.argv)
  #run(main=main, argv=[sys.argv[0]] + unparsed)
