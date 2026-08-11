using CSharpTimescaleAPI.Models;
using CSharpTimescaleAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSharpTimescaleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    private readonly ICsvProcessingService _csvService;
    private readonly IResultService _resultService;

    public ValuesController(ICsvProcessingService csvService, IResultService resultService)
    {
        _csvService = csvService;
        _resultService = resultService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран");

        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        await using var stream = file.OpenReadStream();
        var result = await _csvService.ProcessCsvAsync(stream, fileName);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result);
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetResults([FromQuery] ResultFilterRequest filter)
    {
        var results = await _resultService.GetFilteredResultsAsync(filter);
        return Ok(results);
    }

    [HttpGet("recent/{fileName}")]
    public async Task<IActionResult> GetRecentValues(string fileName)
    {
        var values = await _resultService.GetRecentValuesAsync(fileName);
        return Ok(new RecentValuesResponse { FileName = fileName, Values = values });
    }
}