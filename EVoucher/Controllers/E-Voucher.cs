using EVoucher.Models;
using Microsoft.AspNetCore.Mvc;
using WebApi.Grpc;


namespace EVoucher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EVoucherController(IHttpClientFactory httpClientFactory, LocationGrpcService.LocationGrpcServiceClient locationClient) : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly LocationGrpcService.LocationGrpcServiceClient _locationClient = locationClient;

        [HttpGet("{InvoiceId}/{EventId}")]
        public async Task<ActionResult<Models.EVoucher>> GetEVoucher(string InvoiceId, Guid EventId)
        {
            if (string.IsNullOrWhiteSpace(InvoiceId))
                return BadRequest("Ogiltigt Id");

            var clientInv = _httpClientFactory.CreateClient("InvoiceApi");
            var clientEv = _httpClientFactory.CreateClient("EventApi");

            Invoice? fetchedInvoice = null;
            Event? fetchedEvent = null;
            string locationString = "";
            string eventDate = "";
            string eventTitle = "";
            string eventName = "";
            string eventTime = "";

            fetchedInvoice = await clientInv.GetFromJsonAsync<Invoice>($"api/Invoice/{InvoiceId}");
            if (fetchedInvoice == null)
                return NotFound("Invoice hittades inte.");

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
                        var fetchedLocation = await _locationClient.GetLocationAsync(new LocationRequest { LocationId = fetchedEvent.LocationId.ToString() });
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
                    Id = fetchedInvoice.OriginalTicketId ?? "",
                    Title = fetchedInvoice.Title ?? "",
                    Name = fetchedInvoice.CustomerName ?? "",
                    Type = fetchedInvoice.Category ?? "",
                    InvoiceNumber = fetchedInvoice.InvoiceNumber ?? "",
                    SeatNumber = "B12",
                    Gate = "3",
                    Location = locationString,
                    Date = eventDate,
                    Time = "12:00 PM - 11:00 PM"
                },
                Schedule = new EventSchedule
                {
                    EventName = eventName != "" ? eventName : "Sommarkonsert",
                    StartTime = new DateTime(2025, 6, 1, 18, 30, 0),
                    EndTime = new DateTime(2025, 6, 1, 21, 0, 0)
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
