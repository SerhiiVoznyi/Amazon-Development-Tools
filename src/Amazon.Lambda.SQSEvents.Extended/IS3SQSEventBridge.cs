//   Copyright © 2021 Serhii Voznyi and open source community
//
//     https://www.linkedin.com/in/serhii-voznyi/
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
namespace Amazon.Lambda.SQSEvents.Extended
{
    using Amazon.S3;
    using DesignPatterns;
    using System.Threading.Tasks;

    /// <summary>
    ///     The IS3SQSEventBridge interface.
    /// </summary>
    /// <seealso cref="IAmazonS3" />
    public interface IS3SQSEventBridge : IBridge<IAmazonS3>
    {
        /// <summary>
        ///     Updates the SQSEvent messages payload asynchronously.
        ///     The messages with large payload which body content located in S3 bucket
        ///     will be updated automatically.
        /// </summary>
        /// <param name="sqsEvent">The SQS event.</param>
        /// <returns>Task from result <see cref="SQSEvent"/> with updated messages</returns>
        Task<SQSEvent> UpdateMessagesPayloadAsync(SQSEvent sqsEvent);
    }
}
