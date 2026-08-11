using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSharpTimescaleAPI.Models;

[Table("Values")]
public class ValueEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public double ExecutionTime { get; set; }
    public double Value { get; set; }
    public string FileName { get; set; } = string.Empty;
}