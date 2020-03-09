# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com

import os.path
import os

import time

def speak(utterance, vc):
  utterance = utterance.replace("'","")
  utterance = utterance.replace('"',"")
  # https://www.openhab.org/addons/voice/picotts/
  # German (de-DE)
  # English, US (en-US)
  # English, GB (en-GB)
  # Spanish (es-ES)
  # French (fr-FR)
  # Italian (it-IT)
  if vc == "" or vc.startswith('enUS'):
    voice = "en-US"
  elif "GB" in vc:
    voice = "en-GB"
  elif "es" in vc:
    voice = "es-ES"
  elif "FR" in vc:
    voice = "fr-FR"
  elif "IT" in vc:
    voice = "it-IT"
  elif "DE" in vc:
    voice = "de-DE"
  else:
    voice = "en-US"

  os.system("pico2wave -l " + voice + " -w audio/speaknow.wav '" + utterance + "' && play -q -V1 audio/speaknow.wav")
  #os.system("pico2wave -l " + lang + " -w audio/speaknow.wav '" + utterance + "' && sox audio/speaknow.wav -c 2 audio/speaknowstereo.wav && aplay -Dhw:1 audio/speaknowstereo.wav" )

def isAudioPlaying():

  audioPlaying = False

  #Check processes using ps
  #---------------------------------------
  cmd = 'ps -C omxplayer,mplayer,aplay,play'
  lineCounter = 0
  p = Popen(cmd, shell=True, stdin=PIPE, stdout=PIPE, stderr=STDOUT, close_fds=True)
  for ln in p.stdout:
    lineCounter = lineCounter + 1
    if lineCounter > 1:
      audioPlaying = True
      break

  return audioPlaying
