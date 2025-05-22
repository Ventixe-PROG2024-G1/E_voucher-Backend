namespace EVoucher.Models;

public class ProhibitedItems
{
    public int Id { get; set; }
    public string[] Items { get; set; } = Array.Empty<string>();
}