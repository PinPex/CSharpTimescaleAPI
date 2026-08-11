namespace CSharpTimescaleAPI.Models;

public class UploadResponse
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public int RecordsCount { get; set; }
    public string? ErrorMessage { get; set; }
}