using CSharpTimescaleAPI.Data;
using CSharpTimescaleAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CSharpTimescaleAPI.Services;

public class CsvProcessingService : ICsvProcessingService
{
    private readonly AppDbContext _context;

    public CsvProcessingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UploadResponse> ProcessCsvAsync(Stream fileStream, string fileName)
    {
        var records = new List<CsvRecord>();
        using var reader = new StreamReader(fileStream);
        string? line;
        var lineNumber = 0;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(';');
            if (parts.Length != 3)
                return Error($"Строка {lineNumber}: ожидается 3 колонки, получено {parts.Length}");

            if (!DateTime.TryParseExact(parts[0], "yyyy-MM-ddTHH-mm-ss.ffffZ",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
                return Error($"Строка {lineNumber}: неверный формат даты");

            if (date > DateTime.UtcNow || date < new DateTime(2000, 1, 1))
                return Error($"Строка {lineNumber}: дата вне допустимого диапазона");

            if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var execTime) || execTime < 0)
                return Error($"Строка {lineNumber}: время выполнения должно быть >= 0");

            if (!double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var value) || value < 0)
                return Error($"Строка {lineNumber}: значение должно быть >= 0");

            records.Add(new CsvRecord { Date = date, ExecutionTime = execTime, Value = value });
        }

        if (records.Count < 1 || records.Count > 10000)
            return Error($"Количество строк {records.Count} вне диапазона 1..10000");

        var existingValues = await _context.Values.Where(v => v.FileName == fileName).ToListAsync();
        if (existingValues.Any())
        {
            _context.Values.RemoveRange(existingValues);
            var existingResult = await _context.Results.FirstOrDefaultAsync(r => r.FileName == fileName);
            if (existingResult != null)
                _context.Results.Remove(existingResult);
        }

        var entities = records.Select(r => new ValueEntity
        {
            Date = r.Date,
            ExecutionTime = r.ExecutionTime,
            Value = r.Value,
            FileName = fileName
        }).ToList();

        await _context.Values.AddRangeAsync(entities);

        var minDate = records.Min(r => r.Date);
        var maxDate = records.Max(r => r.Date);
        var delta = (maxDate - minDate).TotalSeconds;
        var avgExec = records.Average(r => r.ExecutionTime);
        var avgVal = records.Average(r => r.Value);
        var sortedValues = records.Select(r => r.Value).OrderBy(v => v).ToList();
        double median = sortedValues.Count % 2 == 0
            ? (sortedValues[sortedValues.Count / 2 - 1] + sortedValues[sortedValues.Count / 2]) / 2.0
            : sortedValues[sortedValues.Count / 2];

        var result = new ResultEntity
        {
            FileName = fileName,
            DeltaSeconds = delta,
            MinDate = minDate,
            AvgExecutionTime = avgExec,
            AvgValue = avgVal,
            MedianValue = median,
            MaxValue = records.Max(r => r.Value),
            MinValue = records.Min(r => r.Value)
        };

        await _context.Results.AddAsync(result);
        await _context.SaveChangesAsync();

        return new UploadResponse { Success = true, FileName = fileName, RecordsCount = records.Count };
    }

    private UploadResponse Error(string message)
    {
        return new UploadResponse { Success = false, ErrorMessage = message };
    }

    private record CsvRecord
    {
        public DateTime Date { get; init; }
        public double ExecutionTime { get; init; }
        public double Value { get; init; }
    }
}