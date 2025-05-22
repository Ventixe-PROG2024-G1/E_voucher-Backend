namespace EVoucher.Models;

public class EventSchedule
{
    public string EventName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}