using WebApi.Grpc;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("InvoiceApi", client =>
{
    client.BaseAddress = new Uri("https://invoice-ventixe-dyhxapdyaqdbcacq.swedencentral-01.azurewebsites.net/");
});
builder.Services.AddHttpClient("EventApi", client =>
{
    client.BaseAddress = new Uri("https://ventixe-events.azurewebsites.net/");
});

builder.Services.AddGrpcClient<LocationGrpcService.LocationGrpcServiceClient>(o =>
{
    o.Address = new Uri("https://ventixe-locationservice-g8f6d0fdhqc5gqd9.swedencentral-01.azurewebsites.net/");
});
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
