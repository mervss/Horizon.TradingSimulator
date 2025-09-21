using Horizon.Core.Domain;
using Microsoft.AspNetCore.SignalR.Client;

var conn = new HubConnectionBuilder()
    .WithUrl("http://localhost:5176/hubs/prices") 
    //.WithUrl("https://localhost:7228/hubs/prices")
    .WithAutomaticReconnect()
    .Build();

conn.On<PriceTickV1>("price", t =>
    Console.WriteLine($"{t.Symbol} -> {t.Price} @ {t.TimestampUtc:HH:mm:ss} (seq:{t.SequenceId})"));

await conn.StartAsync();
Console.WriteLine("SignalR listening... ENTER to quit");
Console.ReadLine();

