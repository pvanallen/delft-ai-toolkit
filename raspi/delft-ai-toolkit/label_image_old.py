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

"""label_image for tflite."""

import argparse
import time

import numpy as np
from PIL import Image
import tflite_runtime.interpreter as tflite

#import cv2


def load_labels(filename):
  with open(filename, 'r') as f:
    return [line.strip() for line in f.readlines()]

# def draw_rect(image, box):
#     y_min = int(max(1, (box[0] * image.height)))
#     x_min = int(max(1, (box[1] * image.width)))
#     y_max = int(min(image.height, (box[2] * image.height)))
#     x_max = int(min(image.width, (box[3] * image.width)))
#
#     # draw a rectangle on the image
#     cv2.rectangle(image, (x_min, y_min), (x_max, y_max), (255, 255, 255), 2)

if __name__ == '__main__':
  parser = argparse.ArgumentParser()
  parser.add_argument(
      '-i',
      '--image',
      default='image.jpg',
      help='image to be classified')
  parser.add_argument(
      '-m',
      '--model',
      default='teachable',
      help='.tflite model to be executed')
  parser.add_argument(
      '--input_mean',
      default=127.5, type=float,
      help='input_mean')
  parser.add_argument(
      '--input_std',
      default=127.5, type=float,
      help='input standard deviation')
  parser.add_argument(
      '--num_threads', default=None, type=int, help='number of threads')
  args = parser.parse_args()

  model = args.model.lower()
  # print("model = " + model)
  model_file = "models/teachable/model_unquant.tflite"
  labels_file = "models/mobilenet_labels.txt"

  # set up the model
  if model == "teachable":
    model_file = "models/teachable/model_unquant.tflite"
    labels_file = "models/teachable/labels.txt"
  elif model == "squeezenet":
    model_file = "models/squeezenet_2018_04_27/squeezenet.tflite"
    #labels_file = "models/squeezenet_2018_04_27/labels.txt"
  elif model == "inceptionv1":
    model_file = "models/inception_v1_224_quant_20181026/inception_v1_224_quant.tflite"
  elif model == "inceptionv4":
    model_file = "models/inception_v4_299_quant_20181026/inception_v4_1_default_1.tflite"
  elif model == "coco_mobilenet":
    model_file = "models/coco_ssd_mobilenet_v1_1/detect.tflite"
    labels_file = "models/coco_ssd_mobilenet_v1_1/labelmap.txt"
    #labels_file = "models/coco_mobilenet_labels.txt"
  elif model == "mnasnet":
    model_file = "models/mnasnet_1.3_224/mnasnet_1.3_224.tflite"
    labels_file = "models/mobilenet_labels.txt"
  elif model == "mobilenet":
    model_file = "models/mobilenet_v2_1.0_224_quant/mobilenet_v2_1.0_224_quant.tflite"
    labels_file = "models/mobilenet_labels.txt"

  print("[INFO] loading model...")
  interpreter = tflite.Interpreter(model_path=model_file)
  interpreter.allocate_tensors()

  input_details = interpreter.get_input_details()
  output_details = interpreter.get_output_details()

  # check the type of the input tensor
  floating_model = input_details[0]['dtype'] == np.float32

  # NxHxWxC, H:1, W:2
  height = input_details[0]['shape'][1]
  width = input_details[0]['shape'][2]
  img = Image.open(args.image).resize((width, height))

  # add N dim
  input_data = np.expand_dims(img, axis=0)

  if floating_model:
    input_data = (np.float32(input_data) - args.input_mean) / args.input_std

  interpreter.set_tensor(input_details[0]['index'], input_data)

  start_time = time.time()
  interpreter.invoke()
  # stop_time = time.time()

  output_data = interpreter.get_tensor(output_details[0]['index'])
  rects = interpreter.get_tensor(output_details[0]['index'])
  results = np.squeeze(output_data) # these are the RECTS

  #if model == "teachable" or model == "squeezenet" or model == "inceptionv1":

  print("Model is: " + model_file)
  #print(results)
  top_scores = results.argsort()[-5:][::-1]
  labels = load_labels(labels_file)
  if "coco_mobilenet" in model:
      print("rectangles = ",rects)
      scores = interpreter.get_tensor(output_details[2]['index'])
      top_scores = scores[0].argsort()[-5:][::-1]
  print("[INFO] tensors setup...")
  # print("======DATA")
  # print(results, labels)

  # if "coco_mobilenet" in model:
  #       # print("model: " + model)
  #       #print(len(results))
  #
  #       scores = interpreter.get_tensor(output_details[2]['index'])
  #       scores_ordered = scores.argsort()[-5:][::-1]
  #       print("scores: ", scores, scores_ordered)
  #       label_indexes = interpreter.get_tensor(output_details[1]['index'])
  #       print(label_indexes)
  #       #indexes = label_indexes.split()
  #       #print(results, label_indexes)
  #       for i in range(len(results)):
  #           print("i: " + str(i))
  #           print(scores[0][i],labels[int(label_indexes.item(i))])
  #           if floating_model:
  #             print('{:08.6f}: {}'.format(float(scores[i]), label_indexes.item(i)))
            # else:
            #   print('{:08.6f}: {}'.format(float(results[i] / 255.0), label_indexes[i]))
  # else:
      #label_indexes = interpreter.get_tensor(output_details[1]['index'])
  # print(scores)
  for i in top_scores:
    # print("top_score: " + str(i))
    if "coco_mobilenet" in model:
      #print(scores)
      #print('{:08.6f}: {}'.format(float(results[i]), labels[i]))
      #print('{:08.6f}: {}'.format(float(scores[i]), label_indexes.item(i)))
      label_indexes = interpreter.get_tensor(output_details[1]['index'])
      print(rects[0][i])
      print(scores[0][i],labels[int(label_indexes.item(i))])
    else:
      #print(results)
      #print(rects[0][i])
      print('{:08.6f}: {}'.format(float(results[i] / 255.0), labels[i]))

  # else if model == "coco_mobilenet":
  #     print(len(output_details))
  #     #scores = interpreter.get_tensor(output_details[2]['index'])
  #     label_indexes = interpreter.get_tensor(output_details[1]['index'])
  #     print(len(results))
  #     print(results, label_indexes)
  #draw_rect(img,(6.0333145, 4.6928284, 9.1061532, 5.7609767))
  # img = cv2.imread(args.image)
  # new_img = cv2.rectangle(img, (60, 47, 91, 58), (255, 255, 255), 2)
  #
  # cv2.imshow("image", new_img)
  #
  # #waits for user to press any key
  # #(this is necessary to avoid Python kernel form crashing)
  # cv2.waitKey(1)
  #
  # #closing all open windows
  # cv2.destroyAllWindows()
  stop_time = time.time()
  print('time: {:.3f}ms'.format((stop_time - start_time) * 1000))
