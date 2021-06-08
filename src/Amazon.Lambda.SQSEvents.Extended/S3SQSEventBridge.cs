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
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;

    public class S3SQSEventBridge : IS3SQSEventBridge
    {
        #region Constants

        public const string LargePayloadSizeAttributeName =
            "SQSLargePayloadSize";

        public const string S3BucketNameMarker = "-..s3BucketName..-";

        public const string S3KeyMarker = "-..s3Key..-";

        public const string ExceptionText =
            "Failed to get the S3 object which contains the message payload. Message was not received.";

        #endregion

        #region Constructors

        public S3SQSEventBridge(IAmazonS3 s3Client)
        {
            Implementer = s3Client;
        }

        #endregion

        #region Properties

        public IAmazonS3 Implementer { get; }

        #endregion

        #region Public Methods

        public async Task<SQSEvent> UpdateMessagesPayloadAsync(SQSEvent sqsEvent)
        {
            foreach (SQSEvent.SQSMessage message in sqsEvent.Records.Where(IsLargePayloadMessage))
            {
                var messageS3Pointer = ReadMessageS3PointerFromJson(message.Body);

                var originalMessageBody = await GetTextFromS3Async(messageS3Pointer.S3BucketName, messageS3Pointer.S3Key)
                    .ConfigureAwait(false);

                message.Body = originalMessageBody;
                message.ReceiptHandle = EmbedS3PointerInReceiptHandle(message.ReceiptHandle, messageS3Pointer.S3BucketName, messageS3Pointer.S3Key);
                message.MessageAttributes.Remove(LargePayloadSizeAttributeName);
            }

            return sqsEvent;
        }

        #endregion

        #region Private Methods

        private bool IsLargePayloadMessage(SQSEvent.SQSMessage message)
        {
            return message.MessageAttributes.ContainsKey(LargePayloadSizeAttributeName)
                   && message.Body.Contains(nameof(MessageS3Pointer.S3Key), StringComparison.InvariantCultureIgnoreCase)
                   && message.Body.Contains(nameof(MessageS3Pointer.S3BucketName),
                       StringComparison.InvariantCultureIgnoreCase);
        }

        private string EmbedS3PointerInReceiptHandle(string receiptHandle, string s3BucketName, string s3Key)
        {
            return string.Concat(S3BucketNameMarker,
                s3BucketName,
                S3BucketNameMarker,
                S3KeyMarker,
                s3Key,
                S3KeyMarker,
                receiptHandle);
        }

        private async Task<string> GetTextFromS3Async(string s3BucketName, string s3Key)
        {
            var getObjectRequest = new GetObjectRequest { BucketName = s3BucketName, Key = s3Key };
            try
            {
                using var getObjectResponse = await Implementer
                    .GetObjectAsync(getObjectRequest)
                    .ConfigureAwait(false);

                using var streamReader = new StreamReader(getObjectResponse.ResponseStream);
                var text = await streamReader.ReadToEndAsync();

                return text;
            }
            catch (AmazonServiceException e)
            {
                throw new AmazonClientException(ExceptionText, e);
            }
            catch (AmazonClientException e)
            {
                throw new AmazonClientException(ExceptionText, e);
            }
        }

        private MessageS3Pointer ReadMessageS3PointerFromJson(string messageBody)
        {
            try
            {
                return JsonSerializer.Deserialize<MessageS3Pointer>(messageBody) ?? new MessageS3Pointer();
            }
            catch (Exception e)
            {
                throw new AmazonClientException(ExceptionText, e);
            }
        }

        #endregion
    }
}
