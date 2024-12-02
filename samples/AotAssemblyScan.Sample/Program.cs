using AotAssemblyScan.Sample;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => AssemblyUtils
   .GetMarkedTypes()
   .Select(x => x.Name)
   .ToList());

app.Run();
