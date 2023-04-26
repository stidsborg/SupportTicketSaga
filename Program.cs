using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.AspNetCore.SqlServer;
using Cleipnir.ResilientFunctions.SqlServer;
using SupportTicketSaga.Bus;
using SupportTicketSaga.ExternalServices;
using SupportTicketSaga.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var bus = new InMemoryBus();

builder.Services.AddSingleton(bus);
_ = new CustomerAgentStub(bus);
builder.Services.AddSingleton<SupportTicketSaga.Saga.SupportTicketSaga>();
builder.Services.AddSingleton<CustomerSupportAgentFinder>();

const string connectionString = "Server=localhost,1434;Database=NewsletterMessaging;User Id=sa;Password=Strong_password_123!;Encrypt=True;TrustServerCertificate=True;";
//await DatabaseHelper.CreateDatabaseIfNotExists(connectionString); //todo exchange this line with the one below to keep state across restarts (like what you do in real-world)
await DatabaseHelper.RecreateDatabase(connectionString); 
builder.Services.UseResilientFunctions(
    connectionString,
    options: _ => new Options(
        unhandledExceptionHandler: Console.WriteLine,
        crashedCheckFrequency: TimeSpan.FromSeconds(5)
        )
    );

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();

app.MapControllers();

app.Run();