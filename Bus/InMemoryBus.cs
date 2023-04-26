using SupportTicketSaga.ExternalServices;

namespace SupportTicketSaga.Bus;

public class InMemoryBus
{
    private readonly List<Func<CommandAndEvents, Task>> _subscribers = new();
    private readonly object _lock = new();

    public void Subscribe(Func<CommandAndEvents, Task> handler)
    {
        lock (_lock)
            _subscribers.Add(handler);
    }
    
    public Task Send(CommandAndEvents msg)
    {
        Console.WriteLine("MESSAGE_QUEUE SENDING: " + msg.GetType());
        Task.Run(async () =>
        {
            List<Func<CommandAndEvents, Task>> subscribers;
            lock (_lock)
                subscribers = _subscribers.ToList();

            foreach (var subscriber in subscribers)
                await subscriber(msg);
        });
        
        return Task.CompletedTask;
    }
}