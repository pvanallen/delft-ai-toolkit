// /**
// * Modified by Philip van Allen
// *
// * Copyright 2015 IBM Corp. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// *      http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// *
// */
// #pragma warning disable 0649

// using UnityEngine;
// using System.Collections;
// using IBM.Cloud.SDK;
// using IBM.Cloud.SDK.Utilities;
// using IBM.Cloud.SDK.DataTypes;
// using IBM.Watson.Assistant.V2;
// using IBM.Watson.Assistant.V2.Model;

// public class WatsonAssistantV2 : MonoBehaviour
// {

//     private string _serviceUrl;
//     private string _iamApikey;
//     //The service URL (optional). This defaults to "https://gateway.watsonplatform.net/assistant/api\"
//         #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
//         [Space(10)]
//         [Tooltip("The IAM apikey.")]
//         [SerializeField]
//         private string iamApikey;
//         [Tooltip("The service URL (optional). This defaults to \"https://gateway.watsonplatform.net/assistant/api\"")]
//         [SerializeField]
//         private string serviceUrl;
//         [Tooltip("The version date with which you would like to use the service in the form YYYY-MM-DD.")]
//         [SerializeField]
//         private string versionDate;
//         [Tooltip("The assistantId to run the example.")]
//         [SerializeField]
//         private string assistantId;
//         #endregion

//     public delegate void T2sCallback(string transcription);
//     public T2sCallback m_callbackMethod;

//     private string lastTranscription = "";
//     string sessionId = "";

//     WatsonAssistantV2 _service;

//     public void StartService(T2sCallback method, string iamkey, string assId)
//     {
//         _iamApikey = iamkey;
//         assistantId = assId;
//         m_callbackMethod = method;
//         LogSystem.InstallDefaultReactors();
//         Runnable.Run(CreateService());
//     }

//     private IEnumerator CreateService()
//     {
//         if (string.IsNullOrEmpty(_iamApikey))
//         {
//             throw new IBMException("Plesae provide IAM ApiKey for the Assistant service.");
//         }

//         //  Create credential and instantiate service
//         Credentials credentials = null;

//         //  Authenticate using iamApikey
//         TokenOptions tokenOptions = new TokenOptions()
//         {
//             IamApiKey = _iamApikey
//         };

//         credentials = new Credentials(tokenOptions, _serviceUrl);

//         //  Wait for tokendata
//         while (!credentials.HasIamTokenData())
//             yield return null;

//         WatsonAssistantV2 _service = new AssistantService(versionDate, credentials);

//     }

//     public IEnumerator message(string message) {

//         var input1 = new MessageInput()
//             {
//                 Text = message,
//                 Options = new MessageInputOptions()
//                 {
//                     ReturnContext = true
//                 }
//             };


//         _service.message(OnMessage, assistantId, sessionId, input: input1, context: true);
//         yield return null;
//     }

//     // public bool Active
//     // {
//     //     get { return _service.IsListening; }
//     //     set
//     //     {
//     //         // if (value && !_service.IsListening)
//     //         // {
//     //         //     // _service.RecognizeModel = (string.IsNullOrEmpty(_recognizeModel) ? "en-US_BroadbandModel" : _recognizeModel);
//     //         //     // _service.DetectSilence = true;
//     //         //     // _service.EnableWordConfidence = true;
//     //         //     // _service.EnableTimestamps = true;
//     //         //     // _service.SilenceThreshold = 0.01f;
//     //         //     // _service.MaxAlternatives = 1;
//     //         //     // _service.EnableInterimResults = true;
//     //         //     // _service.OnError = OnError;
//     //         //     // _service.InactivityTimeout = 5; // -1 = no timeout
//     //         //     // _service.ProfanityFilter = false;
//     //         //     // _service.SmartFormatting = true;
//     //         //     // _service.SpeakerLabels = false;
//     //         //     // _service.WordAlternativesThreshold = null;
//     //         //     // _service.StartListening(OnRecognize, OnRecognizeSpeaker);
//     //         // }
//     //         // else if (!value && _service.IsListening)
//     //         // {
//     //         //     // _service.StopConversation();
//     //         // }
//     //     }
//     // }

//     public IEnumerator StartConversation()
//     {
//         lastResponse = "no response";
        
//         //Active = true;
//         // if (_recordingRoutine == 0)
//         // {
//         //     UnityObjectUtil.StartDestroyQueue();
//         //     _recordingRoutine = Runnable.Run();
//         // }

//         Log.Debug("ExampleAssistantV2.RunTest()", "Attempting to CreateSession");
//         _service.CreateSession(OnCreateSession, assistantId);

//         while (!createSessionTested)
//         {
//             yield return null;
//         }

//         // Log.Debug("ExampleAssistantV2.RunTest()", "Attempting to Message");
//         // service.Message(OnMessage0, assistantId, sessionId);
//     }

//     public void StopConversation()
//     {
//         // if (lastResponse != "") {
//         //     m_callbackMethod(lastResponse);
//         // }
//         // Active = false;
//         // if (_recordingRoutine != 0)
//         // {
//         //     // Microphone.End(_microphoneID);
//         //     // Runnable.Stop(_recordingRoutine);
//         //     // _recordingRoutine = 0;
//         // }
//     }

//     private void OnError(string error)
//     {
//         //Active = false;

//         Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
//     }

//     // private IEnumerator RecordingHandler()
//     // {
//     //     Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
//     //     //_recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
//     //     yield return null;      // let _recordingRoutine get set..

//     //     if (_recording == null)
//     //     {
//     //         //StopRecording();
//     //         yield break;
//     //     }

//     //     bool bFirstBlock = true;
//     //     //int midPoint = _recording.samples / 2;
//     //     float[] samples = null;
//     // }

//     //     while (_recordingRoutine != 0 && _recording != null)
//     //     {
//     //         //int writePos = Microphone.GetPosition(_microphoneID);
//     //         if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
//     //         {
//     //             Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

//     //             //StopRecording();
//     //             yield break;
//     //         }

//     //         if ((bFirstBlock && writePos >= midPoint)
//     //             || (!bFirstBlock && writePos < midPoint))
//     //         {
//     //             // front block is recorded, make a RecordClip and pass it onto our callback.
//     //             //samples = new float[midPoint];
//     //             //_recording.GetData(samples, bFirstBlock ? 0 : midPoint);

//     //             AudioData record = new AudioData();
//     //             // record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
//     //             // record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
//     //             // record.Clip.SetData(samples, 0);

//     //             //_service.OnListen(record);

//     //             bFirstBlock = !bFirstBlock;
//     //         }
//     //         else
//     //         {
//     //             yield return new WaitForSeconds(timeRemaining);
//     //         }

//     //     }

//     //     yield break;
//     // }

//     public void OnMessage(DetailedResponse<MessageResponse> response, IBMError error) {
//         string lastResponse = response.Result.Output.Generic[0].Text;
//         _service.m_callbackMethod(lastResponse);
//         Log.Debug("ExampleAssistantV2.OnMessage()", "response: {0}", response.Result.Output.Generic[0].Text);
//     }

//     // private void OnRecognize(SpeechRecognitionEvent result)
//     // {
//     //     if (result != null && result.results.Length > 0)
//     //     {
//     //         foreach (var res in result.results)
//     //         {
//     //             foreach (var alt in res.alternatives)
//     //             {
//     //                 if (res.final) {
//     //                     lastResponse = "";
//     //                     m_callbackMethod(alt.transcript);
//     //                 } else {
//     //                     lastTranscription = alt.transcript;
//     //                 }
//     //             }
//     //         }
//     //     }
//     // }

//     private void OnCreateSession(DetailedResponse<SessionResponse> response, IBMError error)
//         {
//             Log.Debug("AssistantV2.OnCreateSession()", "Session: {0}", response.Result.SessionId);
//             sessionId = response.Result.SessionId;
//             //createSessionTested = true;
//         }
//     }
