public interface IMessageQueueService
{
    Task SendAsync<T>(string agent_uid, T message);
    Task SubscribeAsync<T>(string agent_uid, Func<T, Task> onMessage);
    Task UnsubscribeAsync<T>(string agent_uid);
}