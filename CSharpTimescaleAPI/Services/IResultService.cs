using CSharpTimescaleAPI.Models;

namespace CSharpTimescaleAPI.Services;

public interface IResultService
{
    Task<List<ResultEntity>> GetFilteredResultsAsync(ResultFilterRequest filter);
    Task<List<ValueEntity>> GetRecentValuesAsync(string fileName, int count = 10);
}