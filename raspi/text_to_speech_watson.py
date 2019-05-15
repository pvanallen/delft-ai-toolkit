# part of the delft toolkit for smart things
# by Philip van Allen, pva@philvanallen.com

from __future__ import print_function
import os
from ibm_watson import TextToSpeechV1
from ibm_watson.websocket import SynthesizeCallback

class tts_watson:

  class MySynthesizeCallback(SynthesizeCallback):
      def __init__(self):
          SynthesizeCallback.__init__(self)
          self.file_path = "audio/watson.wav"
          self.fd = open(self.file_path, 'wb+')

      def on_connected(self):
          print('Connection was successful')

      def on_error(self, error):
          print('Error received: {}'.format(error))

      def on_content_type(self, content_type):
          print('Content type: {}'.format(content_type))

      def on_timing_information(self, timing_information):
          print(timing_information)

      def on_audio_stream(self, audio_stream):
          self.fd.write(audio_stream)

      def on_close(self):
          self.fd.close()
          print('Done synthesizing. Closing the connection')


  def __init__(self, iamkey, url):
    if url == "" or url == "default":
      url = "https://stream.watsonplatform.net/text-to-speech/api"
    self.service = TextToSpeechV1(
      url=url,
      iam_apikey=iamkey)

  def speak(self, utterance, vc):
    synthesize_callback = self.MySynthesizeCallback()
    # https://cloud.ibm.com/apidocs/text-to-speech#list-voices
     # [de-DE_BirgitVoice,de-DE_BirgitV2Voice,de-DE_DieterVoice,de-DE_DieterV2Voice,
     # en-GB_KateVoice,en-US_AllisonVoice,en-US_AllisonV2Voice,en-US_LisaVoice,
     # en-US_LisaV2Voice,en-US_MichaelVoice,en-US_MichaelV2Voice,es-ES_EnriqueVoice,
     # es-ES_LauraVoice,es-LA_SofiaVoice,es-US_SofiaVoice,fr-FR_ReneeVoice,
     # it-IT_FrancescaVoice,it-IT_FrancescaV2Voice,ja-JP_EmiVoice,pt-BR_IsabelaVoice]
    if vc == "" or "enUS1" in vc:
      voice = "en-US_MichaelVoice"
    elif "enUS2" in vc:
      voice = "en-US_AllisonVoice"
    elif "GB" in vc:
      voice = "en-GB_KateVoice"
    elif "esES1" in vc:
      voice = "es-ES_EnriqueVoice"
    elif "esUS1" in vc:
      voice = "es-US_SofiaVoice"
    elif "FR" in vc:
      voice = "fr-FR_ReneeVoice"
    elif "IT" in vc:
      voice = "it-IT_FrancescaVoice"
    elif "DE1" in vc:
      voice = "de-DE_DieterVoice"
    elif "DE2" in vc:
      voice = "de-DE_BirgitVoice"
    else:
      voice = "en-US_MichaelVoice"

    self.service.synthesize_using_websocket(utterance,
      synthesize_callback,
      accept='audio/wav; rate=44100',
      voice=voice
    )
    os.system("play -q -V1 audio/watson.wav")
