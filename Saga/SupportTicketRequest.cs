namespace SupportTicketSaga.Saga;

public record SupportTicketRequest(Guid SupportTicketId, string TicketDescription, string[] CustomerSupportAgents);