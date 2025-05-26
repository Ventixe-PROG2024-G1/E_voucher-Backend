namespace EVoucher.Models;

public class Event
{
    public Guid Id { get; set; }
    public string EventName { get; set; } = null!;
    public DateTime EventStartDate { get; set; }
    public Guid? LocationId { get; set; }
}
