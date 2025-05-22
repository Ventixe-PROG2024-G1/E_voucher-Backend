namespace EVoucher.Models;


    public class Invoice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InvoiceNumber { get; set; } = null!;
        public string BookingId { get; set; }
        public string? OriginalTicketId { get; set; }
        public string Category { get; set; } = null!;
        public string? Title { get; set; }
        public string? CustomerName { get; set; }

    }





