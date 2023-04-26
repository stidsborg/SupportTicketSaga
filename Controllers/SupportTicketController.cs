using Microsoft.AspNetCore.Mvc;
using SupportTicketSaga.Saga;
using SupportTicketSaga.Services;

namespace SupportTicketSaga.Controllers;

[ApiController]
[Route("[controller]")]
public class SupportTicketController : ControllerBase
{
    private readonly Saga.SupportTicketSaga _supportTicketSaga;
    private readonly CustomerSupportAgentFinder _agentFinder;

    public SupportTicketController(Saga.SupportTicketSaga supportTicketSaga, CustomerSupportAgentFinder agentFinder)
    {
        _supportTicketSaga = supportTicketSaga;
        _agentFinder = agentFinder;
    }

    [HttpPost]
    public async Task<ActionResult> Post(Guid ticketId, string ticketDescription)
    {
        var potentialAgents = _agentFinder.FindPotentialAgentsForTicket(ticketId, ticketDescription);
        var request = new SupportTicketRequest(ticketId, ticketDescription, potentialAgents);
        await _supportTicketSaga.AcceptSupportTicket(request);
        return Ok();
    }
}
