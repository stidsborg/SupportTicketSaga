namespace SupportTicketSaga.Services;

public class CustomerSupportAgentFinder
{
    public string[] FindPotentialAgentsForTicket(Guid ticketId, string ticketDescription)
    {
        return new[] { "peter@google.com", "ole@hotmail.com", "ulla@netscape.com" }; //some awesome logic here in real world
    }
}