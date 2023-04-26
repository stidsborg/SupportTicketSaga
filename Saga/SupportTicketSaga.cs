using Cleipnir.ResilientFunctions;
using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Messaging;
using Cleipnir.ResilientFunctions.Reactive;
using SupportTicketSaga.Bus;
using SupportTicketSaga.ExternalServices;
using Timeout = Cleipnir.ResilientFunctions.Domain.Events.Timeout;

namespace SupportTicketSaga.Saga;

public class SupportTicketSaga : IRegisterRFuncOnInstantiation
{
    private readonly RAction<SupportTicketRequest, Scrapbook> _registration;
    private readonly InMemoryBus _bus;

    public SupportTicketSaga(RFunctions rFunctions, InMemoryBus bus)
    {
        _bus = bus;

        _registration = rFunctions.RegisterAction<SupportTicketRequest, Scrapbook>(
            functionTypeId: nameof(SupportTicketSaga),
            AcceptSupportTicket
        );
        
        bus.Subscribe(
            handler: async msg =>
            {
                if (msg is SupportTicketTaken supportTicketTaken)
                    await _registration.EventSourceWriters.For(supportTicketTaken.Id.ToString()).AppendEvent(supportTicketTaken);
            }
        );
    }

    public Task AcceptSupportTicket(SupportTicketRequest request)
        => _registration.Schedule(functionInstanceId: request.SupportTicketId.ToString(), request);
    
    private async Task AcceptSupportTicket(SupportTicketRequest request, Scrapbook scrapbook, Context context)
    {
        var eventSource = await context.EventSource;
        
        var agents = request.CustomerSupportAgents.Length;
        while (true)
        {
            if (!TimeoutOrResponseForTryReceived(eventSource, scrapbook.Try))
            {
                var customerSupportAgentEmail = request.CustomerSupportAgents[scrapbook.Try % agents];
                await _bus.Send(new TakeSupportTicket(request.SupportTicketId, customerSupportAgentEmail, RequestId: scrapbook.Try.ToString()));
                await eventSource.TimeoutProvider.RegisterTimeout(timeoutId: scrapbook.Try.ToString(), expiresIn: TimeSpan.FromSeconds(5));                
            }
            
            var either = await eventSource
                .OfTypes<SupportTicketTaken, Timeout>()
                .Where(e => e.Match(stt => int.Parse(stt.RequestId), t => int.Parse(t.TimeoutId)) == scrapbook.Try)
                .SuspendUntilNext();

            if (either.HasFirst)
                return;

            scrapbook.Try++;
            await scrapbook.Save();
        }
    }

    public class Scrapbook : RScrapbook
    {
        public int Try { get; set; }
    }

    private bool TimeoutOrResponseForTryReceived(EventSource eventSource, int @try)
    {
        return eventSource
            .OfTypes<SupportTicketTaken, Timeout>()
            .Where(e => e.Match(stt => int.Parse(stt.RequestId), t => int.Parse(t.TimeoutId)) == @try)
            .ExistingToList()
            .Any();
    }
}