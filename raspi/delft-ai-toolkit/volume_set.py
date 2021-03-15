# volume set percentage
import alsaaudio
import sys
# https://stackoverflow.com/questions/41592431/changing-volume-in-python-program-on-raspbery-pi

def setvol(percent):
    m = alsaaudio.Mixer('PCM')
    current_volume = m.getvolume() # Get the current Volume
    m.setvolume(percent)

pct_vol = sys.argv[1]
print("setting volume to: " + pct_vol + " percent")
setvol(int(pct_vol))
