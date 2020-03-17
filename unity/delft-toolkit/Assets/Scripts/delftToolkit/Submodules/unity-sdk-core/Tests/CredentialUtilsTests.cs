/**
* Copyright 2019 IBM Corp. All Rights Reserved.
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

using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Utilities;
using NUnit.Framework;
using IBM.Cloud.SDK.Connection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IBM.Cloud.SDK.Tests
{
    public class CredentialUtilsTests
    {
        [Test]
        public void TestGetVcapCredentialsAsMap()
        {
            var apikey = "bogus-apikey";
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = apikey
                }
            };
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("assistant");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            vcapCredentaialsAsMap.TryGetValue(
                Authenticator.PropNameApikey,
                out string extractedKey);
            Assert.IsTrue(extractedKey == apikey);
        }

        [Test]
        public void TestGetVcapCredentialsAsMapFromInnerEntry()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "fakeapikey"
                }
            };
            vcapCredential.Name = "assistant1";

            var vcapCredential2 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "fakeapikey2"
                }
            };
            vcapCredential2.Name = "assistant2";

            var vcapCredential3 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "fakeapikey3"
                }
            };
            vcapCredential3.Name = "assistant3";
            //map to a single key
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential, vcapCredential2, vcapCredential3 });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("assistant2");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            vcapCredentaialsAsMap.TryGetValue(
                Authenticator.PropNameApikey,
                out string extractedKey);
            Assert.IsTrue(extractedKey == "fakeapikey2");
        }

        [Test]
        public void TestGetVcapCredentialsAsMapInnerEntryMultKeys()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries for first service entry
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikey"
                }
            };
            vcapCredential.Name = "assistantV1";
            var vcapCredential2 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikeyCopy"
                }
            };
            vcapCredential2.Name = "assistantV1Copy";
            //map to creds to first service
            tempVcapCredential.Add("someService", new List<VcapCredential>() { vcapCredential, vcapCredential2 });

            //create credential entries for second service entry
            var vcapCredential3 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV2apikey"
                }
            };
            vcapCredential3.Name = "assistantV2";

            var vcapCredential4 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV2apikeyCopy"
                }
            };
            vcapCredential4.Name = "assistantV2Copy";

            //map creds to second service
            tempVcapCredential.Add("someOtherService", new List<VcapCredential>() { vcapCredential3, vcapCredential4 });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            //should match with inner entry with name "assistantV1Copy"
            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("assistantV1Copy");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            vcapCredentaialsAsMap.TryGetValue(
                Authenticator.PropNameApikey,
                out string extractedKey);
            Assert.IsTrue(extractedKey == "assistantV1apikeyCopy");
        }

        [Test]
        public void TestGetVcapCredentialsAsMapDuplicateName()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries for first service entry
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikey"
                }
            };
            vcapCredential.Name = "assistantV1";
            var vcapCredential2 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikeyCopy"
                }
            };
            vcapCredential2.Name = "assistantV1Copy";
            //map to creds to first service
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential, vcapCredential2 });

            //create credential entries for second service entry
            var vcapCredential3 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV2apikey"
                }
            };
            vcapCredential3.Name = "assistantV2";

            var vcapCredential4 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV2apikeyCopy"
                }
            };
            vcapCredential4.Name = "assistantV2Copy";

            //map creds to second service
            tempVcapCredential.Add("assistantV1", new List<VcapCredential>() { vcapCredential3, vcapCredential4 });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            //should match with inner entry with name "assistantV1"
            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("assistantV1");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            vcapCredentaialsAsMap.TryGetValue(
                Authenticator.PropNameApikey,
                out string extractedKey);
            Assert.IsTrue(extractedKey == "assistantV1apikey");
        }

        [Test]
        public void TestGetVcapCredentialsAsMapNoMatchingName()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries for first service entry
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikey"
                }
            };
            vcapCredential.Name = "assistantV1";
            var vcapCredential2 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikeyCopy"
                }
            };
            vcapCredential2.Name = "assistantV1Copy";
            //map to creds to first service
            tempVcapCredential.Add("no_matching_name", new List<VcapCredential>() { vcapCredential, vcapCredential2 });

            //create credential entries for second service entry
            var vcapCredential3 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV2apikey"
                }
            };
            vcapCredential3.Name = "assistantV2";

            var vcapCredential4 = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV2apikeyCopy"
                }
            };
            vcapCredential4.Name = "assistantV2Copy";

            //map to second service
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential3, vcapCredential4 });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("no_matching_name");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            vcapCredentaialsAsMap.TryGetValue(
                Authenticator.PropNameApikey,
                out string extractedKey);
            Assert.IsTrue(extractedKey == "assistantV1apikey");
        }

        [Test]
        public void TestGetVcapCredentialsAsMapMissingNameField()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries for first service entry
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikey"
                }
            };
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("assistant");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            vcapCredentaialsAsMap.TryGetValue(
                Authenticator.PropNameApikey,
                out string extractedKey);
            Assert.IsTrue(extractedKey == "assistantV1apikey");
        }

        [Test]
        public void TestGetVcapCredentialsAsMapEntryNotFound()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries for first service entry
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikey"
                }
            };
            vcapCredential.Name = "assistantV1";
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("fake_entry");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            Assert.IsTrue(vcapCredentaialsAsMap.Count == 0);
        }

        [Test]
        public void TestGetVcapCredentialsAsMapVcapNotSet()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries for first service entry
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikey"
                }
            };
            vcapCredential.Name = "assistantV1";
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential });

            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("fake_entry");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            Assert.IsTrue(vcapCredentaialsAsMap.Count == 0);
        }

        [Test]
        public void TestGetVcapCredentialsAsMapEmptySvcName()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries for first service entry
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = "assistantV1apikey"
                }
            };
            vcapCredential.Name = "assistantV1";
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            Assert.IsTrue(vcapCredentaialsAsMap.Count == 0);
        }

        [Test]
        public void TestGetVcapCredentialsAsMapNoCreds()
        {
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            //create credential entries for first service entry
            var vcapCredential = new VcapCredential()
            {

            };
            vcapCredential.Name = "assistantV1";
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            var vcapCredentaialsAsMap = CredentialUtils.GetVcapCredentialsAsMap("no-creds");
            Assert.IsNotNull(vcapCredentaialsAsMap);
            Assert.IsTrue(vcapCredentaialsAsMap.Count == 0);
        }

        [Test]
        public void TestGetServiceProperties()
        {
            var apikey = "bogus-apikey";
            var tempVcapCredential = new Dictionary<string, List<VcapCredential>>();
            var vcapCredential = new VcapCredential()
            {
                Credentials = new Credential()
                {
                    ApiKey = apikey
                }
            };
            tempVcapCredential.Add("assistant", new List<VcapCredential>() { vcapCredential });

            var vcapString = JsonConvert.SerializeObject(tempVcapCredential);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", vcapString);
            Assert.IsNotNull(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            var serviceProperties = CredentialUtils.GetServiceProperties("assistant");

            Assert.IsNotNull(serviceProperties);
        }
    }
}
