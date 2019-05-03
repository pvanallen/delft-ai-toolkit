/**
* Modified by Philip van Allen
*
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/
#pragma warning disable 0649

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using IBM.Watson.TextToSpeech.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using UnityEngine.Assertions;

public class WatsonTTS : MonoBehaviour
{
    // The service URL (optional). This defaults to https://stream.watsonplatform.net/text-to-speech/api
    private string _serviceUrl;
    private string _iamApikey;

    // Watons voices https://cloud.ibm.com/docs/services/text-to-speech?topic=text-to-speech-voices
    private string enAllison = "en-US_AllisonVoice";
    private string enMichael = "en-US_MichaelVoice";
    private string enMichael2 = "en-US_MichaelV2Voice";
    private string synthesizeMimeType = "audio/wav";
    private string voiceModelLanguage = "en-US";

    public delegate void watsonTtsCallback(float length);
    public watsonTtsCallback m_callbackMethod;

    private string lastTranscription = "";

    private TextToSpeechService _service;

    public void StartService(watsonTtsCallback method, string iamkey) // add service URL and voice model
    {
        _iamApikey = iamkey;
        m_callbackMethod = method;
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());
    }

    private IEnumerator CreateService()
    {
        if (string.IsNullOrEmpty(_iamApikey))
        {
            throw new IBMException("Plesae provide IAM ApiKey for the service.");
        }

        //  Create credential and instantiate service
        Credentials credentials = null;

        //  Authenticate using iamApikey
        TokenOptions tokenOptions = new TokenOptions()
        {
            IamApiKey = _iamApikey
        };

        credentials = new Credentials(tokenOptions, _serviceUrl);

        //  Wait for tokendata
        while (!credentials.HasIamTokenData())
            yield return null;

        _service = new TextToSpeechService(credentials);
    }

     public void Speak(string synthesizeText, string voiceLabel) {
         string voice;
         switch (voiceLabel) {
            case "enUS1": 
                voice = enMichael;
                break;
            case "enUS2": 
                voice = enAllison;
                break;
            default:
                voice = enMichael;
                break;
         }
         Runnable.Run(Synthesize(synthesizeText, voice));
     }

    private IEnumerator Synthesize(string synthesizeText, string voice) {
        Log.Debug("WatsonTTS", "Attempting to Synthesize...");
        byte[] synthesizeResponse = null;
        AudioClip clip = null;
        _service.Synthesize(
            callback: (DetailedResponse<byte[]> response, IBMError error) =>
            {
                synthesizeResponse = response.Result;
                Assert.IsNotNull(synthesizeResponse);
                Assert.IsNull(error);
                clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                PlayClip(clip);

            },
            text: synthesizeText,
            voice: voice,
            accept: synthesizeMimeType
        );

        while (synthesizeResponse == null)
            yield return null;

        yield return new WaitForSeconds(clip.length);
        m_callbackMethod(clip.length);
    }

    private void PlayClip(AudioClip clip) {
        if (Application.isPlaying && clip != null) {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            GameObject.Destroy(audioObject, clip.length);
        }
    }
}
