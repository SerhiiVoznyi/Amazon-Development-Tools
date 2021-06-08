namespace Amazon.Lambda.SQSEvents.S3Support
{
    using System;

    public static class MessageExtensions
    {
        public static bool IsMessageWithLargePayload(this SQSEvent.SQSMessage message)
        {
            return message.MessageAttributes.ContainsKey(S3SQSEventBridge.LargePayloadSizeAttributeName)
                   && message.Body.Contains(nameof(MessageS3Pointer.S3Key), StringComparison.InvariantCultureIgnoreCase)
                   && message.Body.Contains(nameof(MessageS3Pointer.S3BucketName),
                       StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
