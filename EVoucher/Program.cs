var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("InvoiceApi", client =>
{
    client.BaseAddress = new Uri("https://invoice-ventixe-dyhxapdyaqdbcacq.swedencentral-01.azurewebsites.net/");
});
builder.Services.AddHttpClient("LocationApi", client =>
{
    client.BaseAddress = new Uri("https://location");
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
