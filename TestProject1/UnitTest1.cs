using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EVoucher.Controllers;
using EVoucher.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Xunit;

class SimpleHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;
    public SimpleHttpClientFactory(HttpClient client) => _client = client;
    public HttpClient CreateClient(string name) => _client;
}

public class EVoucherControllerTests
{
    const string BaseUrl = "https://invoice-ventixe-dyhxapdyaqdbcacq.swedencentral-01.azurewebsites.net/";

    private EVoucherController CreateController(params HttpResponseMessage[] responses)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var sequence = handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        foreach (var resp in responses)
            sequence.ReturnsAsync(resp);

        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(BaseUrl)
        };
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var factory = new SimpleHttpClientFactory(client);
        return new EVoucherController(factory);
    }

    [Fact]
    public async Task EmptyId_ReturnsBadRequest()
    {
        var ctrl = CreateController();                  
        var result = await ctrl.GetEVoucher("");
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task InvoiceNotFound_ReturnsNotFound()
    {
        var notFound = new HttpResponseMessage(HttpStatusCode.NotFound);
        var ctrl = CreateController(notFound); 
        var result = await ctrl.GetEVoucher("NOPE");
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task LocationNotFound_ReturnsNotFound()
    {
        var invoiceJson = @"{ ""BookingId"": ""BID"", ""OriginalTicketId"": ""TID"", ""Title"": ""X"", ""CustomerName"": ""A"", ""Category"": ""C"", ""InvoiceNumber"": ""INV"" }";
        var okInvoice = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(invoiceJson, System.Text.Encoding.UTF8, "application/json")
        };
        var notFound = new HttpResponseMessage(HttpStatusCode.NotFound);

        var ctrl = CreateController(okInvoice, notFound);
        var result = await ctrl.GetEVoucher("INV");
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ValidIds_ReturnsOk_EVoucherPopulated()
    {
        var invoiceJson = @"{ ""BookingId"": ""B123"", ""OriginalTicketId"": ""T123"", ""Title"": ""Concert"", ""CustomerName"": ""Alice"", ""Category"": ""VIP"", ""InvoiceNumber"": ""INV123"" }";
        var locationJson = @"""Stockholm Arena""";

        var okInvoice = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(invoiceJson, System.Text.Encoding.UTF8, "application/json")
        };
        var okLocation = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(locationJson, System.Text.Encoding.UTF8, "application/json")
        };

        var ctrl = CreateController(okInvoice, okLocation);
        var actionResult = await ctrl.GetEVoucher("INV123");

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var ev = Assert.IsType<EVoucher>(okResult.Value);

        Assert.Equal("T123", ev.Ticket.Id);
        Assert.Equal("Alice", ev.Ticket.Name);
        Assert.Equal("Stockholm Arena", ev.Ticket.Location);
        Assert.Equal("INV123", ev.Ticket.InvoiceID);
    }
}
