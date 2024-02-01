var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

Console.WriteLine("Hello world from application!");

app.Run();
