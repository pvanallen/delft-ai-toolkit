# You need to install pyaudio to run this example
# pip install pyaudio

# When using a microphone, the AudioSource `input` parameter would be
# initialised as a queue. The pyaudio stream would be continuosly adding
# recordings to the queue, and the websocket client would be sending the
# recordings to the speech to text service

# https://stackoverflow.com/questions/6517953/clear-all-items-from-the-queue
# https://stackoverflow.com/questions/48653745/continuesly-streaming-audio-signal-real-time-infinitely-python

import time
import sys

import pyaudio

from ibm_watson import SpeechToTextV1
from ibm_watson.websocket import RecognizeCallback, AudioSource
from threading import Thread
from ibm_cloud_sdk_core.authenticators import IAMAuthenticator

import queue
from queue import Empty

try:
    from Queue import Queue, Full
except ImportError:
    from queue import Queue, Full

###############################################
#### Initalize queue to store the recordings ##
###############################################


class stt_watson(object):
    # modifying IBM's class so it can be restarted
    # https://github.com/watson-developer-cloud/python-sdk/blob/master/ibm_watson/websocket/audio_source.py
    class AudioSource2(AudioSource):
        def restart_recording(self):
            self.is_recording = True

    # define callback for the speech to text service
    class MyRecognizeCallback(RecognizeCallback):
        def __init__(self, q):
            RecognizeCallback.__init__(self)
            self.transcript = "no transcript"
            self.q = q
            self.keep_thread_alive = True

        def on_transcription(self, transcript):
            pass

        def on_connected(self):
            print('Watson Connection was successful')
            pass

        def on_error(self, error):
          print('Watson Error received: {}'.format(error))
          #self.q.put((True, self.transcript))
          #self.q.put((True, "timeout"))
          #self.keep_thread_alive = False

        def on_inactivity_timeout(self, error):
          print('Watson Inactivity timeout: {}'.format(error))
          #self.q.put((True, self.transcript))
          #self.keep_thread_alive = False
          #thread.exit()

        def on_listening(self):
          print('Watson STT is listening...')

        def on_hypothesis(self, hypothesis):
          # print("hypo: " + hypothesis)
          pass

        def on_data(self, data):
          print("ON_DATA:",data)
          result = data['results'][0]['alternatives'][0]['transcript']
          final = data['results'][0]['final']
          self.transcript = result
          self.q.put((final, result))
          if (final):
            print("final: " + result)
          else:
            print("interim: " + result)

        def on_close(self):
          print("Watson closed Connection")
          self.q.put((False, "connectionClosed"))
          time.sleep(2)
          self.keep_thread_alive = False
          # self.audio_paused = True
          # with self.q_soc.mutex:
          #     self.q_soc.queue.clear()
          #thread.exit()

    def __init__(self, iamkey, url, lang, timeout):
        # if url == "" or url == "default":
        #   url = "https://stream.watsonplatform.net/speech-to-text/api"
        # print("url: " + url)
        #print("key: " + iamkey)
        # self.speech_to_text = SpeechToTextV1(
        #   iam_apikey=iamkey,
        #   url=url)

        # try: self.thread_running
        # except NameError: self.thread_running = None

        print("Watson STT INIT STARTED")

        self.iamkey = iamkey
        self.streaming = None
        self.recognize_thread = None
        self.need_restart = False

        self.CHUNK = 1024
        # Note: It will discard if the websocket client can't consumme fast enough
        # So, increase the max size as per your choice
        self.BUF_MAX_SIZE = self.CHUNK * 10

        # Variables for recording the speech
        self.FORMAT = pyaudio.paInt16
        self.CHANNELS = 1
        self.RATE = 44100

        # Buffer to store audio
        self.q_aud = Queue(maxsize=int(round(self.BUF_MAX_SIZE / self.CHUNK)))

        # queue for the websocket process to send to main
        self.q_soc = Queue()

        # queue to send data from main to websoc thread
        #self.q_proc = Queue()
        authenticator = IAMAuthenticator(self.iamkey)
        # if url == "" or url == "default":
        #   url = "https://stream.watsonplatform.net/speech-to-text/api"
        self.speech_to_text = SpeechToTextV1(authenticator=authenticator)
        # Create an instance of AudioSource
        self.audio_source = self.AudioSource2(self.q_aud, True, True)
        # instantiate pyaudio
        self.audio = pyaudio.PyAudio()
        #self.FORMAT = self.audio.get_format_from_width(2)

        self.stream = self.audio.open(
            input_device_index = 1,
            format=self.FORMAT,
            channels=self.CHANNELS,
            rate=self.RATE,
            input=True,
            frames_per_buffer=self.CHUNK,
            stream_callback=self.pyaudio_callback,
            start=False
        )

        #self.audio_paused = True

        self.timeout = timeout

        langnew = self.get_lang(lang)
        #timeout = -1 # go forever
        # if hasattr(self,'thread_running'):
        #     if not self.thread_running:
        print("spawn thread")
        #self.recognize_thread = Thread(target=self.recog_thread, args=(self.q_soc,self.q_proc, self.audio, self.audio_source, self.stream, langnew, self.timeout))
        self.recognize_thread = Thread(target=self.recog_thread, args=(self.q_soc, self.audio, self.audio_source, self.stream, langnew, self.timeout))
        #recognize_thread.setDaemon(True)
        # self.test_thread = Thread(target=self.test, args=("a"))
        self.recognize_thread.start()
        # self.test_thread.start()
        #self.thread_running = True
        print("finished spawn thread")
        # self.keep_thread_alive = True

    def restart(self,langnew):
        # clean up before restarting thread

        # Buffer to store audio
        self.q_aud = Queue(maxsize=int(round(self.BUF_MAX_SIZE / self.CHUNK)))

        # queue for the websocket process to send to main
        self.q_soc = Queue()

        print("Restarting...Watson STT")
        self.stream.stop_stream()
        self.stream.close()
        self.audio.terminate()
        self.audio_source.completed_recording()

        time.sleep(0.5)

        self.audio_source = self.AudioSource2(self.q_aud, True, True)
        self.audio = pyaudio.PyAudio()

        authenticator = IAMAuthenticator(self.iamkey)
        self.speech_to_text = SpeechToTextV1(authenticator=authenticator)
        self.stream = self.audio.open(
            input_device_index = 1,
            format=self.FORMAT,
            channels=self.CHANNELS,
            rate=self.RATE,
            input=True,
            frames_per_buffer=self.CHUNK,
            stream_callback=self.pyaudio_callback,
            start=False
        )

        lang = self.get_lang(langnew)
        self.recognize_thread = Thread(target=self.recog_thread, args=(self.q_soc, self.audio, self.audio_source, self.stream, lang, self.timeout))
        #recognize_thread.setDaemon(True)
        self.recognize_thread.start()
        #self.thread_running = True

    # define callback for pyaudio to store the recording in queue
    def pyaudio_callback(self, in_data, frame_count, time_info, status):
        try:
            self.q_aud.put(in_data)
        except Full:
            pass  # discard
        return (None, pyaudio.paContinue)

    # this function will initiate the STT service and pass in the AudioSource
    def recog_thread(self, q_soc, audio, audio_source, stream, lang, timeout):
        print("starting Watson STT thread with lang: " + lang)
        #print("lang: " + lang)
        audio_source.restart_recording()
        stream.start_stream()

        mycallback = self.MyRecognizeCallback(q_soc)

        while mycallback.keep_thread_alive:
        #while True:
            print("recog_thread: starting websocket connection...")
            try:
                self.speech_to_text.recognize_using_websocket(audio=audio_source,
                                                         content_type='audio/l16; rate=44100',
                                                         model=lang,
                                                         input_device_index=1,
                                                         recognize_callback=mycallback,
                                                         interim_results=True,
                                                         inactivity_timeout=timeout)
            except:
                print("Waston disconnected")

        # shut it all down
        print("recogize_thread: shutting down...")
        self.need_restart = True
        # stream.stop_stream()
        # stream.close()
        # audio.terminate()
        # audio_source.completed_recording()


    def get_lang(self, langnew):
        # https://cloud.ibm.com/docs/services/speech-to-text?topic=speech-to-text-models
        if langnew == "" or langnew == "default" or langnew == "enUS":
          lang = "en-US_BroadbandModel"
        elif langnew == "enGB":
          lang = "en-GB_BroadbandModel"
        elif langnew == "deDE":
          lang = "de-DE_BroadbandModel"
        elif langnew == "esES":
          lang = "es-ES_BroadbandModel"
        elif langnew == "frFR":
          lang = "fr-FR_BroadbandModel"
        elif langnew == "koKR":
          lang = "ko-KR_BroadbandModel"
        else:
          lang = "en-US_BroadbandModel"

        self.lang = langnew
        return lang

      ###############################################
      #### Initiate recognition ########
      ###############################################
    def transcribe(self, lang, time_limit):

        # if self.audio_paused:
        #     print("transribe: restart")
        #     # self.stream.stop_stream()
        #     #self.stream.start_stream()
        #     # self.audio_paused = False
        #     # self.thread_running = True
        #     # self.keep_thread_alive = True
        #     self.restart(lang)

        # if lang != self.lang or not self.thread_running:
        print("restart needed: ",self.need_restart)
        if lang != self.lang or self.need_restart:
            # print("Current language is: " + self.lang + " new lang is: " + lang)
            # print("should be: restarting this process to change languages")
            self.restart(lang)
            time.sleep(2)
            self.need_restart = False
        else:
            self.stream.start_stream()



        transcript = "no transcription"
        print("starting transcription...")
        try:
          # recognize_thread = Thread(target=self.recognize_using_weboscket, args=(self.q,self.audio_source, lang, time_limit))
          # #recognize_thread.setDaemon(True)
          # recognize_thread.start()
          #self.recognize_using_weboscket(q,self.audio_source, lang, timeout)
          timeout = time.time() + time_limit
          print(time.time(),timeout)
          transcript = "no transcription"
          #status = False
          #while status == False and time.time() < timeout:
          #message = self.q_soc.get(True,1)
          self.q_soc.queue.clear()
          self.q_aud.queue.clear()
          while time.time() < timeout:
            try:
              message = self.q_soc.get(True,2)
              if message != None:
                print("Watson STT message:",message[0],message[1])
                if message[0]:
                    transcript = message[1]
                    print("got a final: " + transcript)
                    break
                else:
                    print("got an interim: " + transcript)
                    transcript = message[1]
                    if message[1] == "connectionClosed":
                        print(message[1],"exiting due to Watson closing...")
                        self.need_restart = True
                        break

                # if transcript == "timeout":
                #     status = True
                #     #transcript = "no transcription"
                #     print("recognize: TIMEOUT")
                #     #self.thread_running = False

            except Empty:
            #except:
              pass
            #time.sleep(0.001)

          # if time.time() >= timeout:
          print("Completed transcribe()...",time.time(),timeout)

        except BaseException as e:
        #except:
          print('Error: ' + str(e))
          print("all done...")
        finally:
          print("Exiting transcribe()...")
          self.stream.stop_stream()
          # self.audio_paused = True
          #self.stream.close()
          # self.audio.terminate()
          # self.audio_source.completed_recording()

        return transcript
        #sys.exit()
