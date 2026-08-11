using CSharpTimescaleAPI.Models;

namespace CSharpTimescaleAPI.Services;

public interface ICsvProcessingService
{
    Task<UploadResponse> ProcessCsvAsync(Stream fileStream, string fileName);
}