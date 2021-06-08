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
namespace Amazon.Lambda.SQSEvents.Extended.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using JsonSerializer = System.Text.Json.JsonSerializer;

    [ExcludeFromCodeCoverage]
    public class S3SQSEventBridgeTests
    {
        #region Constants

        private const string ExpectedReceiptHandle = "MrTestBot";

        private const string ExpectedMessageBody = "some dummy text";

        private const string ExpectedS3Key = "sqs-key-1";

        private const string ExpectedS3BucketName = "s3-bucket-name-1";

        #endregion

        #region Constructors Tests

        [Fact]
        public void Set_Implementer()
        {
            // Arrange
            var s3ClientMock = new Mock<IAmazonS3>();

            // Act
            var sut = new S3SQSEventBridge(s3ClientMock.Object);

            // Assert
            sut.Implementer.Should().Be(s3ClientMock.Object);
        }

        #endregion

        #region UpdateMessagesPayloadAsync Method Tests

        [Fact]
        public async Task GetTextFromS3()
        {
            // Arrange
            SQSEvent sqsEvent = GetTestEvent();
            var s3ClientMock = new Mock<IAmazonS3>();
            s3ClientMock
                .Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = GetStreamFromText(ExpectedMessageBody)
                });

            var sut = new S3SQSEventBridge(s3ClientMock.Object);

            // Act
            var result = await sut.UpdateMessagesPayloadAsync(sqsEvent);

            // Assert
            result.Should().Be(sqsEvent);
            var resultMessage = result.Records[0];
            resultMessage.Body.Should().Be(ExpectedMessageBody);
            resultMessage.MessageAttributes.Should().NotContainKey(S3SQSEventBridge.LargePayloadSizeAttributeName);
            resultMessage.ReceiptHandle.Should().Be(string.Concat(S3SQSEventBridge.S3BucketNameMarker,
                ExpectedS3BucketName,
                S3SQSEventBridge.S3BucketNameMarker,
                S3SQSEventBridge.S3KeyMarker,
                ExpectedS3Key,
                S3SQSEventBridge.S3KeyMarker,
                ExpectedReceiptHandle));
        }

        [Fact]
        public async Task Throw_AmazonClientException_On_AmazonClientException()
        {
            // Arrange
            var s3ClientMock = new Mock<IAmazonS3>();
            s3ClientMock
                .Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .Throws(new AmazonClientException("Some message text"));

            var sut = new S3SQSEventBridge(s3ClientMock.Object);

            // Act
            await Assert.ThrowsAnyAsync<AmazonClientException>(() => sut.UpdateMessagesPayloadAsync(GetTestEvent()));
        }

        [Fact]
        public async Task Throw_AmazonClientException_On_AmazonServiceException()
        {
            // Arrange
            var s3ClientMock = new Mock<IAmazonS3>();
            s3ClientMock
                .Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .Throws(new AmazonServiceException());

            var sut = new S3SQSEventBridge(s3ClientMock.Object);

            // Act
            await Assert.ThrowsAnyAsync<AmazonClientException>(() => sut.UpdateMessagesPayloadAsync(GetTestEvent()));
        }

        [Fact]
        public async Task Throw_AmazonClientException_On_BadMessageBody()
        {
            // Arrange
            var s3ClientMock = new Mock<IAmazonS3>();
            s3ClientMock
                .Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .Throws(new AmazonServiceException());

            var sqsEvent = GetTestEvent();
            sqsEvent.Records[0].Body = $"{nameof(MessageS3Pointer.S3Key)}{nameof(MessageS3Pointer.S3BucketName)}";

            var sut = new S3SQSEventBridge(s3ClientMock.Object);

            // Act
            await Assert.ThrowsAnyAsync<AmazonClientException>(() => sut.UpdateMessagesPayloadAsync(sqsEvent));
        }

        #endregion

        #region Private Tests Methods

        private static Stream GetStreamFromText(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static SQSEvent GetTestEvent()
        {
            var s3Pointer = new MessageS3Pointer
            {
                S3Key = ExpectedS3Key,
                S3BucketName = ExpectedS3BucketName
            };

            var sqsMessage = new SQSEvent.SQSMessage
            {
                Body = JsonSerializer.Serialize(s3Pointer),
                Attributes = new Dictionary<string, string>(),
                MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>(),
                ReceiptHandle = ExpectedReceiptHandle,
                MessageId = "message-id"

            };

            sqsMessage.MessageAttributes.Add(
                S3SQSEventBridge.LargePayloadSizeAttributeName,
                    new SQSEvent.MessageAttribute
                    {
                        StringValue = "some-value"
                    });

            return new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage> { sqsMessage
}
            };
        }

        #endregion
    }
}
