from picamera.array import PiRGBArray
from picamera import PiCamera
import picamera
import os
import os.path
import time
#import cv2

from adafruit_crickit import crickit
import neopixel
from adafruit_seesaw.neopixel import NeoPixel
import text_to_speech_pico as tts_pico

def init():
    global capture_home_directory
    capture_home_directory = "teachable_machine/"
    #NeoPixel
    num_pixels = 16 # 16 for old robot - change for the number your NeoPixel ring has
    pixels = NeoPixel(crickit.seesaw, 20, num_pixels, brightness=0.4, pixel_order=(1, 0, 2, 3), bpp=4)
    neo_black = (0,0,0,0) #rgbw
    blink_color = (256,256,256,256) #rgbw

def start_capture(category, interval, num_pics):
    camera = picamera.PiCamera()
    capture_home_directory = "teachable_machine/"
    #NeoPixel
    num_pixels = 16 # 16 for old robot - change for the number your NeoPixel ring has
    pixels = NeoPixel(crickit.seesaw, 20, num_pixels, brightness=0.4, pixel_order=(1, 0, 2, 3), bpp=4)
    neo_black = (0,0,0,0) #rgbw
    blink_color = (256,256,256,256) #rgbw

    cap_directory = capture_home_directory + category
    CHECK_FOLDER = os.path.isdir(cap_directory)

    # If folder doesn't exist, then create it.
    if not CHECK_FOLDER:
        os.makedirs(cap_directory)
        print("created folder : ", cap_directory)

    else:
        print(cap_directory, "folder already exists.")

    intro = "about to start capturing " + str(num_pics) + " images for " + category
    print(intro)
    tts_pico.speak(intro, "GB")
    for i in range(1,num_pics + 1, 1):
        image_name = cap_directory + "/" + category + "-" + str(i).zfill(2) + ".jpg"
        time.sleep(interval)
        pixels.fill(blink_color)
        tts_pico.speak(category + ", " + str(i) + "of " + str(num_pics) + ", in 3, 2, 1", "GB")
        # pixels.fill(neo_black)
        # time.sleep(0.2)

        print("image name: ", image_name)
        camera.capture(image_name)
        time.sleep(0.5)
        pixels.fill(neo_black)
    time.sleep(1)
    tts_pico.speak("all finished with " + category, "GB")
    camera.close()

#start_capture("screwdriver", 3, 3, (90,90))
