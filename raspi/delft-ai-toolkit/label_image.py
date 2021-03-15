# Copyright 2018 The TensorFlow Authors. All Rights Reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ==============================================================================

 # other reference
 #   https://github.com/tensorflow/tensorflow/blob/master/tensorflow/lite/examples/python/label_image.py
 #   https://github.com/tensorflow/tensorflow/tree/master/tensorflow/lite/examples/python/
 #   https://heartbeat.fritz.ai/running-tensorflow-lite-object-detection-models-in-python-8a73b77e13f8
 #   https://www.tensorflow.org/lite/models/object_detection/overview

"""label_image for tflite"""

import argparse
import time

import numpy as np

import io
import time
import picamera

from PIL import Image
from picamera.array import PiRGBArray
from picamera import PiCamera
import tflite_runtime.interpreter as tflite

current_model = None

input_mean = 127.5
input_std = 127.5 # input standard deviation

separator = "\\"

#import cv2

def change_model(model="mobilenet"):
    global current_model, labels_file, score_factor
    global interpreter,input_details,output_details
    global floating_model, labels

    model = model.lower()
    if model != current_model:
      print("loading inference model: " + model)
      start_time = time.time()
      model_file = "models/mobilenet_v2_1.0_224_quant/mobilenet_v2_1.0_224_quant.tflite"
      labels_file = "models/mobilenet_labels.txt"
      score_factor = 1

      # set up the model
      if model == "teach1": # assuming quantized
        model_file = "models/teachable1/model.tflite"
        labels_file = "models/teachable1/labels.txt"
      elif model == "teach2": # assuming quantized
          model_file = "models/teachable2/model.tflite"
          labels_file = "models/teachable2/labels.txt"
      elif model == "teach3": # assuming quantized
          model_file = "models/teachable3/model.tflite"
          labels_file = "models/teachable3/labels.txt"
      elif model == "squeezenet":
        model_file = "models/squeezenet_2018_04_27/squeezenet.tflite"
        score_factor = 100
        #labels_file = "models/squeezenet_2018_04_27/labels.txt"
      elif model == "incept1":
        model_file = "models/inception_v1_224_quant_20181026/inception_v1_224_quant.tflite"
      elif model == "incept4":
        model_file = "models/inception_v4_299_quant_20181026/inception_v4_1_default_1.tflite"
        score_factor = 100
      elif model == "coco_mobilenet":
        model_file = "models/coco_ssd_mobilenet_v1_1/detect.tflite"
        labels_file = "models/coco_ssd_mobilenet_v1_1/labelmap.txt"
        #labels_file = "models/coco_mobilenet_labels.txt"
      elif model == "mnasnet":
        model_file = "models/mnasnet_1.3_224/mnasnet_1.3_224.tflite"
        labels_file = "models/mobilenet_labels.txt"
        score_factor = 10
      elif model == "mobilenet":
        model_file = "models/mobilenet_v2_1.0_224_quant/mobilenet_v2_1.0_224_quant.tflite"
        labels_file = "models/mobilenet_labels.txt"

      current_model = model

      #print("[INFO] loading model...")
	  # load the serialized model from disk
      interpreter = tflite.Interpreter(model_path=model_file)
      interpreter.allocate_tensors()

      input_details = interpreter.get_input_details()
      output_details = interpreter.get_output_details()

      # check the type of the input tensor
      floating_model = input_details[0]['dtype'] == np.float32
      labels = load_labels(labels_file)

      print("[INFO] Loaded " + model_file + " & " + labels_file)
      stop_time = time.time()
      print('time: {:.3f}ms'.format((stop_time - start_time) * 1000))
      return
    else:
        #print("[INFO] model already loaded")
        return

def init(camera_in, model):
	"""Initializes the camera stream and the recognition model using changeModel()

	Args:
		modeltype: name of the deep learning model we will use for inference

	Returns:
		Nothing
	"""
	global camera

	# initialize the camera and grab a reference to the raw camera capture
	# camera = PiCamera()
	camera = camera_in
	change_model(model)
	return

def load_labels(filename):
  with open(filename, 'r') as f:
    return [line.strip() for line in f.readlines()]

def close():
	global camera
	camera.close()
	return

def run_inference_on_image(model, threshold, image_name = ""):
	# Captures image from the Pi Cam and runs inference
    #
	# Returns:
	# 	String: 5 top objects recognized
    global current_model, labels_file
    global interpreter,input_details,output_details
    global floating_model, labels
    global camera, score_factor

    camera = PiCamera()
    change_model(model)
    # stop_time = time.time()
    # print('time: {:.3f}ms'.format((stop_time - start_time) * 1000))
	# grab an image from the camera
    # print("[INFO] capturing image...")
    start_time = time.time()
	# NxHxWxC, H:1, W:2
    height = input_details[0]['shape'][1]
    width = input_details[0]['shape'][2]
    if image_name == "":
        image_name = "recognize_capture.jpg"
        print("[INFO] Capturing new image: " + image_name)
        # print("pause...")
        # time.sleep(1)
        camera.capture(image_name, resize=(width, height))
        img = Image.open(image_name)
        # Create the in-memory stream
        # stream = io.BytesIO()

        # camera.start_preview()
        # time.sleep(0.05)
        # camera.capture(stream, format='jpeg', resize=(width, height))
        # # "Rewind" the stream to the beginning so we can read its content
        # stream.seek(0)
        # img = Image.open(stream)


    else: # open existing image
        print("[INFO] Using existing image: " + image_name)
        img = Image.open(image_name).resize((width, height))


    #img = Image.open(image_name)
    #print("W/H=",width,height)
    print("Image loaded...")
    stop_time = time.time()
    print('time: {:.3f}ms'.format((stop_time - start_time) * 1000))
    # add N dim
    print("Running model on image...")
    start_time = time.time()
    input_data = np.expand_dims(img, axis=0)

    if floating_model:
        input_data = (np.float32(input_data) - input_mean) / input_std

    interpreter.set_tensor(input_details[0]['index'], input_data)


    interpreter.invoke()
    # stop_time = time.time()

    output_data = interpreter.get_tensor(output_details[0]['index'])
    rects = interpreter.get_tensor(output_details[0]['index'])
    results = np.squeeze(output_data) # these are the RECTS

    #print("Model is: " + model)
    #print(results)
    top_scores = results.argsort()[-5:][::-1]
    #labels = load_labels(labels_file)
    if "coco_mobilenet" in model:
        #print("rectangles = ",rects)
        scores = interpreter.get_tensor(output_details[2]['index'])
        top_scores = scores[0].argsort()[-5:][::-1]
    # print("[INFO] top scores...",top_scores)

    catetories = ""
    for i in top_scores:
      # print("top_score: " + str(i))
      if "coco_mobilenet" in model:
          label_indexes = interpreter.get_tensor(output_details[1]['index'])
          label = label_indexes.item(i)
          score = float(scores[0][i]) * score_factor
          #print(label,score,threshold)
          if score >= threshold:
              catetories += '{} {:08.6f}'.format(labels[int(label_indexes.item(i))],score) + separator
      else:
          score = float(results[i] / 255.0) * score_factor

          label = labels[i]
          #print(label,score,threshold)
          if "teach" in model:
              label = labels[i].split()[1]
              # print("teachable label: " + label)
          if score >= threshold:
              catetories += '{} {:08.6f}'.format(label,score) + separator

    stop_time = time.time()
    print('time: {:.3f}ms'.format((stop_time - start_time) * 1000))
    #print(catetories)
    camera.close()
    return catetories
