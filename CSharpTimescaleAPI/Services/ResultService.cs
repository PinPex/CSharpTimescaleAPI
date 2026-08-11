using CSharpTimescaleAPI.Data;
using CSharpTimescaleAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpTimescaleAPI.Services;

public class ResultService : IResultService
{
    private readonly AppDbContext _context;

    public ResultService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResultEntity>> GetFilteredResultsAsync(ResultFilterRequest filter)
    {
        var query = _context.Results.AsQueryable();

        if (!string.IsNullOrEmpty(filter.FileName))
            query = query.Where(r => r.FileName.Contains(filter.FileName));

        if (filter.MinDateFrom.HasValue)
            query = query.Where(r => r.MinDate >= filter.MinDateFrom.Value);
        if (filter.MinDateTo.HasValue)
            query = query.Where(r => r.MinDate <= filter.MinDateTo.Value);

        if (filter.AvgValueFrom.HasValue)
            query = query.Where(r => r.AvgValue >= filter.AvgValueFrom.Value);
        if (filter.AvgValueTo.HasValue)
            query = query.Where(r => r.AvgValue <= filter.AvgValueTo.Value);

        if (filter.AvgExecTimeFrom.HasValue)
            query = query.Where(r => r.AvgExecutionTime >= filter.AvgExecTimeFrom.Value);
        if (filter.AvgExecTimeTo.HasValue)
            query = query.Where(r => r.AvgExecutionTime <= filter.AvgExecTimeTo.Value);

        return await query.ToListAsync();
    }

    public async Task<List<ValueEntity>> GetRecentValuesAsync(string fileName, int count = 10)
    {
        return await _context.Values
            .Where(v => v.FileName == fileName)
            .OrderByDescending(v => v.Date)
            .Take(count)
            .ToListAsync();
    }
}