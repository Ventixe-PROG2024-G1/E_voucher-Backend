using EVoucher.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;

public class EVoucherControllerTests
{
    private readonly Mock<IHttpClientFactory> _httpFactoryMock;
    private readonly Mock<HttpMessageHandler> _invHandler;
    private readonly Mock<HttpMessageHandler> _evtHandler;

    private readonly object _fakeGrpcClient = null!;

    public EVoucherControllerTests()
    {
        _invHandler = new Mock<HttpMessageHandler>();
        _evtHandler = new Mock<HttpMessageHandler>();

        var invClient = new HttpClient(_invHandler.Object) { BaseAddress = new Uri("http://fake-invoice/") };
        var evtClient = new HttpClient(_evtHandler.Object) { BaseAddress = new Uri("http://fake-event/") };

        _httpFactoryMock = new Mock<IHttpClientFactory>();
        _httpFactoryMock.Setup(f => f.CreateClient("InvoiceApi")).Returns(invClient);
        _httpFactoryMock.Setup(f => f.CreateClient("EventApi")).Returns(evtClient);
    }

    [Fact]
    public async Task Returns_BadRequest_If_InvoiceId_Is_Empty()
    {
        var controller = new EVoucherController(_httpFactoryMock.Object, null!); 
        var result = await controller.GetEVoucher("", Guid.NewGuid());
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Returns_NotFound_If_Invoice_Not_Found()
    {
        _invHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        var controller = new EVoucherController(_httpFactoryMock.Object, null!);

        var result = await controller.GetEVoucher("INV1", Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Returns_OK_And_EVoucher_If_All_Data_Found()
    {
        var invJson = @"{
          ""OriginalTicketId"": ""TID123"",
          ""Title"": ""Concert"",
          ""CustomerName"": ""Alice"",
          ""Category"": ""VIP"",
          ""InvoiceNumber"": ""INV123""
        }";
        _invHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(invJson, System.Text.Encoding.UTF8, "application/json")
            });

        var evJson = @"{
          ""id"": ""d35a555d-41ea-4a8b-a331-ccf9802a4e97"",
          ""eventName"": ""Rockfest"",
          ""eventStartDate"": ""2025-07-01T18:00:00"",
          ""locationId"": ""4e5ed618-c16c-495e-9d97-6d03ec246962""
        }";
        _evtHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(evJson, System.Text.Encoding.UTF8, "application/json")
            });

        var controller = new EVoucherController(_httpFactoryMock.Object, null!);

        var result = await controller.GetEVoucher("INV123", Guid.NewGuid());
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var evoucher = Assert.IsType<EVoucher.Models.EVoucher>(okResult.Value);

        Assert.Equal("TID123", evoucher.Ticket.Id);
        Assert.Equal("Alice", evoucher.Ticket.Name);
        Assert.Equal("", evoucher.Ticket.Location);
    }

    [Fact]
    public async Task Handles_Event_Not_Found_And_Returns_EmptyFields()
    {
        var invJson = @"{
          ""OriginalTicketId"": ""TID123"",
          ""Title"": ""Concert"",
          ""CustomerName"": ""Alice"",
          ""Category"": ""VIP"",
          ""InvoiceNumber"": ""INV123""
        }";
        _invHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(invJson, System.Text.Encoding.UTF8, "application/json")
            });

        _evtHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("API error"));

        var controller = new EVoucherController(_httpFactoryMock.Object, null!);

        var result = await controller.GetEVoucher("INV123", Guid.NewGuid());
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var evoucher = Assert.IsType<EVoucher.Models.EVoucher>(okResult.Value);

        Assert.Equal("TID123", evoucher.Ticket.Id);
        Assert.Equal("", evoucher.Ticket.Location);
    }
}
