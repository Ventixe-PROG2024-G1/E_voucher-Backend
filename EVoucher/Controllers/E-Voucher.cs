using EVoucher.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EVoucher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EVoucherController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EVoucherController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{InvoiceId}")]
        public async Task<ActionResult<Models.EVoucher>> GetEVoucher(string InvoiceId)

        {
            if (string.IsNullOrWhiteSpace(InvoiceId))
            {
                return BadRequest("Ogiltigt Id");

            }
            var client = _httpClientFactory.CreateClient("InvoiceApi");

            var fetchedInvoice = await client.GetFromJsonAsync<Invoice>($"api/Invoice/{InvoiceId}");
            if (fetchedInvoice == null)
                return NotFound($"Invoice hittades inte.");

            var fetchedLocation = await client.GetFromJsonAsync<E_VoucherTicket>($"api/Booking/{fetchedInvoice.BookingId}/Location");
            if (fetchedLocation == null)
                return NotFound($"Location för booking hittades inte.");


            var Evoucher = new Models.EVoucher
            {
                Id = Guid.NewGuid().ToString(),
                Ticket = new E_VoucherTicket
                {
                    Id = fetchedInvoice.OriginalTicketId,
                    Title = fetchedInvoice.Title,
                    Name = fetchedInvoice.CustomerName,
                    Type = fetchedInvoice.Category,
                    InvoiceID = fetchedInvoice.InvoiceNumber,
                    SeatNumber = "B12",
                    Gate = "3",
                    Location = "fetchedLocation",
                    Date = "April 20,2029",
                    Time = "12:00 PM - 11:00 PM",
                },
                Schedule = new EventSchedule
                {
                    EventName = "Sommarkonsert",
                    StartTime = new DateTime(2025, 6, 1, 18, 30, 0),
                    EndTime = new DateTime(2025, 6, 1, 21, 0, 0)
                },
                ProhibitedItems = new ProhibitedItems
                {
                    Items = new[] { "Weapons and Dangerous Items", "Illegal Substances", "Alcohol and Beverages", "Recording Equipment", "Large or Hazardous Items", "Noise Makers and Disruptive Items", "Unauthorized Merchandise", "Pets and Animals", "Bicycles, Skateboards, or Hoverboards", "Coolers or Picnic Baskets", "Umbrellas or Large Parasols", "Camping Gear" }
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
