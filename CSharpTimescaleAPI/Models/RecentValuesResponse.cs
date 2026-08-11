namespace CSharpTimescaleAPI.Models;

public class RecentValuesResponse
{
    public string FileName { get; set; } = string.Empty;
    public List<ValueEntity> Values { get; set; } = new();
}