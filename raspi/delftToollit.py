# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com
# created with the help of TU Delft

import time
import argparse
import os.path
import sys
import serial
import serial.tools.list_ports
from pythonosc import dispatcher
from pythonosc import osc_server
from pythonosc import osc_message_builder
from pythonosc import udp_client

from threading import Thread
import socket

FLAGS = None

events = ["move","leds","delay", "analogin"]
types = ["stop", "forward", "backward", "turnRight", "turnLeft", "set", "blink", "allOff", "pause", "start"]
easings = ["none", "easeIn", "easeOut", "easeInOut"]

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
  arduinoStr = '{},{},{},{}\n'.format(
    name_val(events, strip_adr(adr)),
    name_val(types, type),
    interval,
    port
  )
  print(arduinoStr)
  if ser != None: ser.write(arduinoStr.encode())

def main(_):
  global client
  global ser

  # set up client
  client = udp_client.SimpleUDPClient(FLAGS.server_ip, 5006)

  count = 0.0;
  print("network: " + socket.gethostname() + " " + get_ip())

  server_thread = Thread(target=osc_loop,args=())
  server_thread.start() # run in background as a thread

  while True:
    if ser != None:
      if ser.inWaiting() > 0:
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
      builder = osc_message_builder.OscMessageBuilder(address="/num/analogIn/0/")
      builder.add_arg(count)
      builder.add_arg(count + 1)
      builder.add_arg(count + 2)
      msg = builder.build()
      client.send(msg)
      time.sleep(0.1)

    time.sleep(0.01)


if __name__ == '__main__':
  parser = argparse.ArgumentParser()

  parser.add_argument(
      '--server_ip',
      type=str,
      default='127.0.0.1',
      help='IP of server to where unity is running'
  )

  parser.add_argument(
      '--usb',
      type=str,
      default='/dev/ttyACM0',
      help='port name of the arduino'
  )

  FLAGS, unparsed = parser.parse_known_args()
  # set up handlers for incoming OSC messages
  dispatcher = dispatcher.Dispatcher()
  dispatcher.map("/move/", move_cb)
  dispatcher.map("/leds/", leds_cb)
  dispatcher.map("/delay/", delay_cb)
  dispatcher.map("/analogin/", analogin_cb)

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
  main(sys.argv)
  #run(main=main, argv=[sys.argv[0]] + unparsed)
