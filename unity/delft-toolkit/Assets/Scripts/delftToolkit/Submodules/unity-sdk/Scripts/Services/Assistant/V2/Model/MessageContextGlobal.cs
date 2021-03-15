/**
* (C) Copyright IBM Corp. 2018, 2020.
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

using Newtonsoft.Json;

namespace IBM.Watson.Assistant.V2.Model
{
    /// <summary>
    /// Session context data that is shared by all skills used by the Assistant.
    /// </summary>
    public class MessageContextGlobal
    {
        /// <summary>
        /// Built-in system properties that apply to all skills used by the assistant.
        /// </summary>
        [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
        public MessageContextGlobalSystem System { get; set; }
        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string SessionId { get; private set; }
    }
}
