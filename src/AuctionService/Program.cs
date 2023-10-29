
using System.Text.Json;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Util.Internal.PlatformServices;
using AuctionService;
using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the service container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt => {
  opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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
  DbInitializer.InitDb(app);
}
catch (Exception e)
{
  Console.WriteLine(e);
}










app.Run();




// Code for Amazon SQS messaging
// var sqsClient = new AmazonSQSClient(RegionEndpoint.USEast1);

// var auction = new AuctionCreated{
//     Id=Guid.NewGuid(),
//     ReservePrice = 20000,
//     Seller = "bob",
//     Winner = "",
//     SoldAmount = 0,
//     CurrentHighBid = 0,
//     CreatedAt = DateTime.UtcNow,
//     UpdatedAt = DateTime.UtcNow,
//     AuctionEnd = DateTime.UtcNow
// };

// var queueUrlResponse = await sqsClient.GetQueueUrlAsync("Auctions");

// var sendMessageRequest = new SendMessageRequest{
//   QueueUrl = queueUrlResponse.QueueUrl,
//   MessageBody = JsonSerializer.Serialize(auction)
// };

// var reponse = await sqsClient.SendMessageAsync(sendMessageRequest);