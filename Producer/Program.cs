using Microsoft.EntityFrameworkCore;
using Receiver.Data;
using Receiver.Handler;
using Receiver.Repo;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddScoped<DataContext>();
builder.Services.AddScoped<ServiceBusRepo>();
builder.Services.AddTransient(sp =>
{
    var scopedServiceProvider = sp.CreateScope().ServiceProvider;

    var connectionString = builder.Configuration["ServiceBus:ConnectionString"]!;
    var queueSubscription = builder.Configuration["ServiceBus:SubscriptionQueue"]!;
    var queueConfirmation = builder.Configuration["ServiceBus:ConfirmationQueue"]!;

    var serviceBusRepo = scopedServiceProvider.GetRequiredService<ServiceBusRepo>();

    return new ServiceBusHandler(connectionString, queueSubscription, queueConfirmation, serviceBusRepo);
});

var app = builder.Build();
var serviceProvider = app.Services;

var serviceBusHandler = serviceProvider.GetRequiredService<ServiceBusHandler>();
await serviceBusHandler.StartSubscribing();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();