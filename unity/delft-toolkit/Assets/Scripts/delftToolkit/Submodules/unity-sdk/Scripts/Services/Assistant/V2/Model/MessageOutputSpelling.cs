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
    /// Properties describing any spelling corrections in the user input that was received.
    /// </summary>
    public class MessageOutputSpelling
    {
        /// <summary>
        /// The user input text that was used to generate the response. If spelling autocorrection is enabled, this text
        /// reflects any spelling corrections that were applied.
        /// </summary>
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }
        /// <summary>
        /// The original user input text. This property is returned only if autocorrection is enabled and the user input
        /// was corrected.
        /// </summary>
        [JsonProperty("original_text", NullValueHandling = NullValueHandling.Ignore)]
        public string OriginalText { get; set; }
        /// <summary>
        /// Any suggested corrections of the input text. This property is returned only if spelling correction is
        /// enabled and autocorrection is disabled.
        /// </summary>
        [JsonProperty("suggested_text", NullValueHandling = NullValueHandling.Ignore)]
        public string SuggestedText { get; set; }
    }
}
