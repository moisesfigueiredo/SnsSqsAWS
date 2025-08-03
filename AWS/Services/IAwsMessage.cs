namespace AWS.Services
{
    public interface ISnsPublisher
    {
        Task PublishAsync<T>(T message);
    }

    public interface ISqsConsumer
    {
        Task ReceiveMessagesAsync();
    }
}
