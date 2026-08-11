namespace CSharpTimescaleAPI.Models;


public class CsvRecord
{
    public DateTime Date { get; init; }
    public double ExecutionTime { get; init; }
    public double Value { get; init; }
}