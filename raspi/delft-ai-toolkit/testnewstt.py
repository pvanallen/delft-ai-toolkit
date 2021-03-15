import time
import speech_to_text_watson as stt_watson
iamkey = "Y7zRSrJxaoOr-4wqUUeEGmxvrzTpabEmTBG-AvrNSPnn"
url = ""
lang = "enUS"
timeout = -1
timelimit = 10

stt = stt_watson.stt_watson(iamkey, url, lang, 600)
time.sleep(5)
print("speak 1")
transcription = stt.transcribe(lang, timelimit)
print("FINAL1: " + transcription)
time.sleep(10)

print("")
print("speak 2")
transcription = stt.transcribe(lang, timelimit)
print("FINAL2: " + transcription)
#stt.restart("frFR")

time.sleep(timelimit)
print("waiting...")
time.sleep(timelimit)
print("waiting...")
time.sleep(timelimit)
print("waiting...")
time.sleep(timelimit)
print("waiting...")
time.sleep(timelimit)
print("waiting...")
time.sleep(timelimit)
print("waiting...")
#print("frFR")
print("speak 3")
transcription = stt.transcribe(lang, timelimit)
print("FINAL3: " + transcription)
