# USAGE
# python real_time_object_detection.py --prototxt MobileNetSSD_deploy.prototxt.txt --model MobileNetSSD_deploy.caffemodel

# import the necessary packages
from imutils.video import VideoStream
from imutils.video import FPS
import numpy as np
import argparse
import imutils
import time
import cv2

# import the necessary packages
from picamera.array import PiRGBArray
from picamera import PiCamera

camera = None
net = None
rawCapture = None
running = False

# initialize the list of class labels MobileNet SSD was trained to
# detect, then generate a set of bounding box colors for each class
CLASSES = ["background", "aeroplane", "bicycle", "bird", "boat",
		"bottle", "bus", "car", "cat", "chair", "cow", "diningtable",
		"dog", "horse", "motorbike", "person", "pottedplant", "sheep",
		"sofa", "train", "tvmonitor"]

def init():
	"""Initializes the camera stream

	Args:
		Nothing

	Returns:
		Nothing
	"""
	global camera
	global net
	global rawCapture
	global running

	# COLORS = np.random.uniform(0, 255, size=(len(CLASSES), 3))

	# load our serialized model from disk
	model = "models/MobileNetSSD_deploy.caffemodel"
	prototxt = "models/MobileNetSSD_deploy.prototxt"
	print("[INFO] loading model...")
	net = cv2.dnn.readNetFromCaffe(prototxt, model)

	# initialize the camera and grab a reference to the raw camera capture
	camera = PiCamera()
	camera.resolution = (640, 480)
	camera.framerate = 32
	rawCapture = PiRGBArray(camera, size=(640, 480))

	running = True

	print("[INFO] model loaded, camera setup")

def continuous_classifier():
	fps = FPS().start()
	fps_counter = 0 # number of frames to get FPS on
	# capture frames from the camera
	for frame in camera.capture_continuous(rawCapture, format="bgr", use_video_port=True):
		# grab the raw NumPy array representing the image, then initialize the timestamp
		# and occupied/unoccupied text
		image = frame.array
		image = imutils.resize(image, width=300)

		# grab the frame dimensions and convert it to a blob
		(h, w) = image.shape[:2]
		blob = cv2.dnn.blobFromImage(cv2.resize(image, (300, 300)),
								0.007843, (300, 300), 127.5)

		# pass the blob through the network and obtain the detections and
		# predictions
		net.setInput(blob)
		detections = net.forward()

		# loop over the detections
		label = ""
		for i in np.arange(0, detections.shape[2]):
			# extract the confidence (i.e., probability) associated with
			# the prediction
			confidence = detections[0, 0, i, 2]

			# filter out weak detections by ensuring the `confidence` is
			# greater than the minimum confidence
			if confidence > 0.2:
				# extract the index of the class label from the
				# `detections`, then compute the (x, y)-coordinates of
				# the bounding box for the object
				idx = int(detections[0, 0, i, 1])
				# box = detections[0, 0, i, 3:7] * np.array([w, h, w, h])
				# (startX, startY, endX, endY) = box.astype("int")

				# # draw the prediction on the frame
				label += "{}: {:.2f}%".format(CLASSES[idx],
				 					confidence * 100)
				# cv2.rectangle(frame, (startX, startY), (endX, endY),
				# 				COLORS[idx], 2)
				# y = startY - 15 if startY - 15 > 15 else startY + 15
				# cv2.putText(frame, label, (startX, y),
				# 				cv2.FONT_HERSHEY_SIMPLEX, 0.5, COLORS[idx], 2)
		print(label)

		# clear the stream in preparation for the next frame
		rawCapture.truncate(0)
		# update the FPS counter
		fps.update()
		fps_counter += 1

		if fps_counter >= 10:
			fps.stop()
			print("[INFO] approx. FPS: {:.2f}".format(fps.fps()))
			fps_counter = 0
			fps = FPS().start()

def classifier():
	# fps = FPS().start()
	# fps_counter = 0 # number of frames to get FPS on
	# capture frames from the camera
	camera.capture(rawCapture, format="bgr", use_video_port=True)
	# grab the raw NumPy array representing the image, then initialize the timestamp
	# and occupied/unoccupied text
	image = rawCapture.array
	image = imutils.resize(image, width=300)

	# grab the frame dimensions and convert it to a blob
	(h, w) = image.shape[:2]
	blob = cv2.dnn.blobFromImage(cv2.resize(image, (300, 300)), 0.007843, (300, 300), 127.5)

	# pass the blob through the network and obtain the detections and
	# predictions
	net.setInput(blob)
	detections = net.forward()

	# loop over the detections
	label = ""
	for i in np.arange(0, detections.shape[2]):
		# extract the confidence (i.e., probability) associated with
		# the prediction
		confidence = detections[0, 0, i, 2]

		# filter out weak detections by ensuring the `confidence` is
		# greater than the minimum confidence
		if confidence > 0.2:
			# extract the index of the class label from the
			# `detections`, then compute the (x, y)-coordinates of
			# the bounding box for the object
			idx = int(detections[0, 0, i, 1])
			# box = detections[0, 0, i, 3:7] * np.array([w, h, w, h])
			# (startX, startY, endX, endY) = box.astype("int")

			# # draw the prediction on the frame
			label += "{}: {:.2f}% ".format(CLASSES[idx], confidence * 100)
			# cv2.rectangle(frame, (startX, startY), (endX, endY),
			# 				COLORS[idx], 2)
			# y = startY - 15 if startY - 15 > 15 else startY + 15
			# cv2.putText(frame, label, (startX, y),
			# 				cv2.FONT_HERSHEY_SIMPLEX, 0.5, COLORS[idx], 2)
	print(label)

	# clear the stream in preparation for the next frame
	rawCapture.truncate(0)
	return label
	# update the FPS counter
	# fps.update()
	# fps_counter += 1

	# if fps_counter >= 10:
	# 	fps.stop()
	# 	print("[INFO] approx. FPS: {:.2f}".format(fps.fps()))
	# 	fps_counter = 0
	# 	fps = FPS().start()

def close():
	global camera
	camera.close()
