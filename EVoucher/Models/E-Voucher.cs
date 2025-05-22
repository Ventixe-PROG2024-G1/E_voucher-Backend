namespace EVoucher.Models;

public class EVoucher
{
    public string Id { get; set; }
    public string TicketId { get; set; } = null!;
    public E_VoucherTicket Ticket { get; set; } = null!;
    public EventSchedule Schedule { get; set; } = null!;
    public ProhibitedItems ProhibitedItems { get; set; } = null!;
    public TermsConditions TermsConditions { get; set; } = null!;
    public VenueMap? Map { get; set; }
}