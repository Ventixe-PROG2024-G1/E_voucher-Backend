namespace EVoucher.Models;

public class E_VoucherTicket
{
    public string Id { get; set; }
    public string Title { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string InvoiceNumber { get; set; } = null!;
    public string SeatNumber { get; set; } = null!;
    public string Gate { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string Date { get; set; }     
    public string Time { get; set; }

}
