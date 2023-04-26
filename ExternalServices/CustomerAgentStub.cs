using SupportTicketSaga.Bus;

namespace SupportTicketSaga.ExternalServices;

public class CustomerAgentStub
{
    private readonly InMemoryBus _bus;
    
    public CustomerAgentStub(InMemoryBus bus)
    {
        _bus = bus;
        
        bus.Subscribe(msg =>
        {
            if (msg is TakeSupportTicket cmd)
                HandleTakeSupportTicketCommand(cmd);
            
            return Task.CompletedTask;
        });        
    }

    private void HandleTakeSupportTicketCommand(TakeSupportTicket cmd)
    {
        var coinFlip = Random.Shared.Next(1, 3);
        if (coinFlip == 1 || coinFlip == 2 || coinFlip == 3)
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Console.WriteLine($"TICKET #{cmd.RequestId} TAKEN BY: '{cmd.CustomerSupportAgentEmail}'");
                await _bus.Send(
                    new SupportTicketTaken(
                        cmd.Id,
                        cmd.CustomerSupportAgentEmail,
                        cmd.RequestId
                    )
                );
            });
        else
            Console.WriteLine($"TICKET #{cmd.RequestId} WILL NOT BE TAKEN");
    } 
}