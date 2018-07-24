# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com

import os.path
import os
import re
import sys

from six.moves import urllib

import time
from pythonosc import dispatcher
from pythonosc import osc_server
from pythonosc import osc_message_builder
from pythonosc import udp_client

from threading import Thread
import socket

# speech to text
import sys
import json
import urllib
import urllib.request
import subprocess
import pycurl
#import StringIO
import io
from io import BytesIO
import os.path
import base64
import time
import RPi.GPIO as GPIO
import subprocess
from subprocess import Popen, PIPE, STDOUT

from google.cloud import speech
from google.cloud.speech import enums
from google.cloud.speech import types

os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = "google-credentials.json"

def speak(phrase):
  print("Speaking...")
  os.system("pico2wave -w speaknow.wav '" + phrase + "' && sox speaknow.wav -c 2 speaknowstereo.wav && aplay -Dhw:1 speaknowstereo.wav" )

def isAudioPlaying():

  audioPlaying = False

  #Check processes using ps
  #---------------------------------------
  cmd = 'ps -C omxplayer,mplayer'
  lineCounter = 0
  p = Popen(cmd, shell=True, stdin=PIPE, stdout=PIPE, stderr=STDOUT, close_fds=True)
  for ln in p.stdout:
    lineCounter = lineCounter + 1
    if lineCounter > 1:
      audioPlaying = True
      break

  return audioPlaying ;


def speech2text(duration):

  speech_file = 'speech2text.flac'

  #Do nothing if audio is playing
  #------------------------------------
  if isAudioPlaying():
    print (time.strftime("%Y-%m-%d %H:%M:%S ") + "Audio is playing")
    return ""

  #Record sound
  #----------------
  print ("listening for " + str(duration) + " seconds...")
  os.system('arecord -D plughw:1 -f cd -c 1 -t wav -d ' + str(duration) + '  -q -r 16000 | flac - -s -f --best -o ' + speech_file)

  #Check if the amplitude is high enough
  #---------------------------------------
  cmd = 'sox ' + speech_file + ' -n stat'
  p = Popen(cmd, shell=True, stdin=PIPE, stdout=PIPE, stderr=STDOUT, close_fds=True)
  soxOutput = p.stdout.read()
  #print "Popen output" + soxOutput

  maxAmpStart = soxOutput.find(b"Maximum amplitude")+24
  maxAmpEnd = maxAmpStart + 7

  #print "Max Amp Start: " + str(maxAmpStart)
  #print "Max Amop Endp: " + str(maxAmpEnd)

  maxAmpValueText = soxOutput[maxAmpStart:maxAmpEnd]

  #print "Max Amp: " + maxAmpValueText

  maxAmpValue = float(maxAmpValueText)

  if maxAmpValue < 0.1 :
    print ("Audio too quiet, not sending to Google")
    #Exit if sound below minimum amplitude
    return ""

  #Send sound  to Google Cloud Speech Api to interpret
  #----------------------------------------------------
  print (time.strftime("%Y-%m-%d %H:%M:%S ")  + "Sending to google api")

  # https://cloud.google.com/speech-to-text/docs/sync-recognize
  # https://googlecloudplatform.github.io/google-cloud-python/latest/speech/index.html
  # send the file to google speech api for transcription
  client = speech.SpeechClient()

  with io.open(speech_file, 'rb') as audio_file:
      content = audio_file.read()

  audio = types.RecognitionAudio(content=content)
  config = types.RecognitionConfig(
      encoding=enums.RecognitionConfig.AudioEncoding.FLAC,
      sample_rate_hertz=16000,
      language_code='en-US')

  response = client.recognize(config, audio)
  # Each result is for a consecutive portion of the audio. Iterate through
  # them to get the transcripts for the entire audio file.
  final_result = ""

  for result in response.results:
      # The first alternative is the most likely one for this portion.
      #print(u'Transcript: {}'.format(result.alternatives[0].transcript))
      final_result += u'{} '.format(result.alternatives[0].transcript)

  print (time.strftime("%Y-%m-%d %H:%M:%S ") + " transcription: " + final_result)
  return final_result
