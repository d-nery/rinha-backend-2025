using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using rinha;
[module:DapperAot]

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddHostedService<PaymentHandler>();

var app = builder.Build();

app.MapPost("/payments", async ([FromBody] Payment payment) => { await PaymentService.EnqueuePayment(payment); });
app.MapGet("/payments-summary", async (
    [FromQuery] DateTimeOffset? from,
    [FromQuery] DateTimeOffset? to) => await SummaryService.GetPaymentSummary(from, to));

app.Run();

