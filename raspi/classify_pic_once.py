# USAGE


# import the necessary packages
import numpy as np
import os.path
import os
import re
import sys
import tarfile
import argparse
import time

import cv2
from picamera.array import PiRGBArray
from picamera import PiCamera

net = None
camera = None
rawCapture = None
classes = None
current_model = None

#camera = PiCamera()

def change_model(modeltype="squeezenet"):
	global net
	global classes
	global current_model

	if modeltype != current_model:
		labels="labels/synset_words.txt"
		current_model = modeltype

		# set up the model
		if modeltype == "googlenet":
			model = "models/bvlc_googlenet.caffemodel"
			prototxt = "models/bvlc_googlenet.prototxt"
		elif modeltype == "squeezenet":
			model = "models/squeezenet_v1.1.caffemodel"
			prototxt = "models/squeezenet_v1.1.prototxt"
		elif modeltype == "alexnet":
			model = "models/bvlc_alexnet.caffemodel"
			prototxt = "models/bvlc_alexnet.prototxt"
		elif modeltype == "inception":
			model = "models/Inception21k.caffemodel"
			prototxt = "models/Inception21k.prototxt"
			labels="labels/synset21k.txt"
		elif modeltype == "rcnn":
			model = "models/bvlc_reference_rcnn_ilsvrc13.caffemodel"
			prototxt = "models/bvlc_reference_rcnn_ilsvrc13.prototxt"
			labels="labels/synset_rcnn.txt"
		# elif modeltype == "rcnn-vgg16":
		# 	model = "models/faster_rcnn_vgg16.caffemodel"
		# 	prototxt = "models/faster_rcnn_vgg16.prototxt"
		# elif modeltype == "rcnn-zf":
		# 	model = "models/faster_rcnn_zf.caffemodel"
		# 	prototxt = "models/faster_rcnn_zf.prototxt"

		# load the class labels from disk
		rows = open(labels).read().strip().split("\n")
		# get the labels
		classes = [r[r.find(" ") + 1:].split(",")[0] for r in rows] # only get first label

		print("[INFO] loading model...")
		# load the serialized model from disk
		net = cv2.dnn.readNetFromCaffe(prototxt, model)
		print("[INFO] model loaded")
		return

	else:
		print("[INFO] model already loaded")
		return

def init(camera_in, modeltype):
	"""Initializes the camera stream and the recognition model using changeModel()

	Args:
		modeltype: name of the deep learning model we will use for inference

	Returns:
		Nothing
	"""
	global camera
	global rawCapture
	global classes
	# initialize the camera and grab a reference to the raw camera capture
	# camera = PiCamera()
	camera = camera_in
	change_model(modeltype)
	return

def close():
	global camera
	camera.close()
	return

def run_inference_on_image(modeltype):
	"""Captures image from the Pi Cam and runs inference

	Returns:
		String: each object recognize delimited by a forward slash
	"""

	change_model(modeltype)
	# grab an image from the camera
	print("[INFO] capturing image...")
	rawCapture = PiRGBArray(camera)
	camera.capture(rawCapture, format="bgr")
	image = rawCapture.array

	# our CNN requires fixed spatial dimensions for our input image(s)
	# so we need to ensure it is resized to 224x224 pixels while
	# performing mean subtraction (104, 117, 123) to normalize the input;
	# after executing this command our "blob" now has the shape:
	# (1, 3, 224, 224)
	blob = cv2.dnn.blobFromImage(image, 1, (224, 224), (104, 117, 123))

	# set the blob as input to the network and perform a forward-pass to
	# obtain our output classification
	net.setInput(blob)
	start = time.time()
	preds = net.forward()
	# end = time.time()
	# print("[INFO] classification took {:.5} seconds".format(end - start))

	# sort the indexes of the probabilities in descending order (higher
	# probabilitiy first) and grab the top-5 predictions
	preds = preds.reshape((1, len(classes)))
	idxs = np.argsort(preds[0])[::-1][:5]
	responseText = ""

	for (i, idx) in enumerate(idxs):
	 	# draw the top prediction on the input image
		if i == 0:
	 		text = "Label: {}, {:.2f}%".format(classes[idx],
	 			preds[0][idx] * 100)
	 		cv2.putText(image, text, (5, 25), cv2.FONT_HERSHEY_SIMPLEX,
				0.7, (0, 0, 255), 2)

		# display the predicted label + associated probability to the
		# console
		item = "{}: {:.5F}".format(classes[idx], preds[0][idx])
		if i != 0:
			item = "\\" + item

		responseText += item
		# print("[INFO] {}. label: {}, probability: {:.5}".format(i + 1,
		# 	classes[idx], preds[0][idx]))

	#print(responseText)
	cv2.imwrite('capture.png', image)
	end = time.time()
	print("[INFO] classification took {:.5} seconds".format(end - start))
	return responseText
