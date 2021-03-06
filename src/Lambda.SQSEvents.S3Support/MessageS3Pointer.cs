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
namespace Amazon.Lambda.SQSEvents.S3Support
{
    public class MessageS3Pointer
    {
        public MessageS3Pointer()
        {
            S3Key = string.Empty;
            S3BucketName = string.Empty;
        }

        public string S3Key { get; set; }
        public string S3BucketName { get; set; }
    }
}
