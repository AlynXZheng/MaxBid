using MongoDB.Driver;
using MongoDB.Entities;
using SearchService;
using MassTransit;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMassTransit(x => {
  x.UsingRabbitMq((context, cfg) =>{
    cfg.ConfigureEndpoints(context);
  });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception e)
{
   Console.WriteLine(e);
}


app.Run();



