using EVoucher.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using WebApi.Grpc;


namespace EVoucher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EVoucherController(IHttpClientFactory httpClientFactory, LocationGrpcService.LocationGrpcServiceClient locationClient, IConfiguration config, ILogger<EVoucherController> logger) : ControllerBase
    {
        private readonly ILogger<EVoucherController> _logger = logger;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly LocationGrpcService.LocationGrpcServiceClient _locationClient = locationClient;
        private readonly IConfiguration _config = config;

        [HttpGet("{InvoiceId}/{EventId}")]
        public async Task<ActionResult<Models.EVoucher>> GetEVoucher(string InvoiceId, Guid EventId)
        {
            var clientInv = _httpClientFactory.CreateClient("InvoiceApi");
            var clientEv = _httpClientFactory.CreateClient("EventApi");

            Invoice? fetchedInvoice = null;
            Event? fetchedEvent = null;
            string locationString = "";
            string eventDate = "";
            string eventTitle = "";
            string eventName = "";
            string eventTime = "";

            if (!string.IsNullOrWhiteSpace(InvoiceId))
            {
                try
                {
                    fetchedInvoice = await clientInv.GetFromJsonAsync<Invoice>($"api/Invoice/{InvoiceId}");
                }
                catch
                {
                    fetchedInvoice = null;
                    if (fetchedInvoice == null)
                    {
                        _logger.LogWarning("Faktura kunde inte hämtas för InvoiceId: {InvoiceId}", InvoiceId);
                    }
                }
            }

            try
            {
                fetchedEvent = await clientEv.GetFromJsonAsync<Event>($"api/event/{EventId}");
            }
            catch
            {
                fetchedEvent = null;
            }

            if (fetchedEvent != null)
            {
                eventTitle = fetchedEvent.EventName ?? "";
                eventDate = fetchedEvent.EventStartDate.ToString("yyyy-MM-dd HH:mm");
                eventName = fetchedEvent.EventName ?? "";
                if (fetchedEvent.LocationId.HasValue)
                {
                    try
                    {
                        var fetchedLocation = await _locationClient.GetLocationAsync(new LocationRequest { LocationId = fetchedEvent.LocationId.ToString() }, new CallOptions(new Metadata { { "location-api-key", _config["SecretKey:location-api-key"] } }));
                        var loc = fetchedLocation.Location;
                        if (loc != null)
                            locationString = $"{loc.LocationName}, {loc.CityName}";
                    }
                    catch
                    {
                    }
                }
            }

            var Evoucher = new Models.EVoucher
            {
                Id = Guid.NewGuid().ToString(),
                Ticket = new E_VoucherTicket
                {
                    Id = fetchedInvoice?.OriginalTicketId ?? "",
                    Title = eventTitle,
                    Name = fetchedInvoice?.CustomerName ?? "",
                    Type = fetchedInvoice?.Category ?? "",
                    InvoiceNumber = fetchedInvoice?.InvoiceNumber ?? "",
                    SeatNumber = "B12",
                    Gate = "3",
                    Location = locationString,
                    Date = eventDate,
                    Time = "12:00 PM - 11:00 PM"
                },
                Schedule = new EventSchedule
                {
                },
                ProhibitedItems = new ProhibitedItems
                {
                    Items = new[]
                    {
                "Weapons and Dangerous Items", "Illegal Substances",
                "Alcohol and Beverages", "Recording Equipment",
                "Large or Hazardous Items", "Noise Makers and Disruptive Items",
                "Unauthorized Merchandise", "Pets and Animals",
                "Bicycles, Skateboards, or Hoverboards", "Coolers or Picnic Baskets",
                "Umbrellas or Large Parasols", "Camping Gear"
            }
                },
                TermsConditions = new TermsConditions
                {
                    Text = "Biljetten är personlig och får ej överlåtas."
                },
                Map = new VenueMap
                {
                    Url = ""
                }
            };

            return Ok(Evoucher);
        }

    }
}
