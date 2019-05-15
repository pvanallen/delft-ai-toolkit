# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com

import os
import time

# play audio

def play(filename):
  #os.system("aplay -Dhw:1 audio/ui_sounds/" + filename + ".wav")
  os.system("play -q -V1 audio/ui_sounds/" + filename + ".wav")
