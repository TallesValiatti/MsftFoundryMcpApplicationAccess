using Books.Mcp.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurations();

var app = builder.Build();

app.ConfigureApplication();

app.Run();

public partial class Program { }
