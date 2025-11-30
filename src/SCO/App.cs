using vNext;

var host = App.Create()
    .Startup<Startup>()
    .Build();

await host.RunAsync();
