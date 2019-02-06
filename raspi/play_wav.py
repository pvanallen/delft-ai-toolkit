# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com

import os
import time

# play audio

def play(filename):
  os.system("aplay -Dhw:1 audio/ui_sounds/" + filename + ".wav"{value for value in variable})

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
