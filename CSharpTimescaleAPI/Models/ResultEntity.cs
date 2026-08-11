using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSharpTimescaleAPI.Models;

[Table("Results")]
public class ResultEntity
{
    [Key]
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public double DeltaSeconds { get; set; }
    public DateTime MinDate { get; set; }
    public double AvgExecutionTime { get; set; }
    public double AvgValue { get; set; }
    public double MedianValue { get; set; }
    public double MaxValue { get; set; }
    public double MinValue { get; set; }
}