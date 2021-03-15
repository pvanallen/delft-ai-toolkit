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

namespace IBM.Watson.Discovery.V1.Model
{
    /// <summary>
    /// Summary of the document usage statistics for the environment.
    /// </summary>
    public class EnvironmentDocuments
    {
        /// <summary>
        /// Number of documents indexed for the environment.
        /// </summary>
        [JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
        public virtual long? Available { get; private set; }
        /// <summary>
        /// Total number of documents allowed in the environment's capacity.
        /// </summary>
        [JsonProperty("maximum_allowed", NullValueHandling = NullValueHandling.Ignore)]
        public virtual long? MaximumAllowed { get; private set; }
    }
}
