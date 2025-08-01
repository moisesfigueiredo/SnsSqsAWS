namespace AWS.Dto
{
    public class AwsSettings
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string ServiceURL { get; set; }
        public string SnsTopicName { get; set; }
        public string SqsQueueName { get; set; }
    }
}
