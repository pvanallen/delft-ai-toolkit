#test_label_image
import label_image as rec
import picamera

model = "teachable1"

camera = picamera.PiCamera()
rec.init(camera, model)
match_results = rec.run_inference_on_image(model)
print(match_results)
camera.close()
