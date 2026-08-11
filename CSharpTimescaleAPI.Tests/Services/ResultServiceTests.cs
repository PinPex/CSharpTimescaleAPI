using CSharpTimescaleAPI.Data;
using CSharpTimescaleAPI.Models;
using CSharpTimescaleAPI.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace CSharpTimescaleAPI.Tests.Services;

public class ResultServiceTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetFilteredResultsAsync_WithFileNameFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        context.Results.AddRange(new List<ResultEntity>
    {
        new ResultEntity { FileName = "example", Id = 1 },
        new ResultEntity { FileName = "example2", Id = 2 },
        new ResultEntity { FileName = "test", Id = 3 }
    });
        await context.SaveChangesAsync();

        var service = new ResultService(context);
        var filter = new ResultFilterRequest { FileName = "example" };

        // Act
        var result = await service.GetFilteredResultsAsync(filter);

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.FileName).Should().Contain(new[] { "example", "example2" });
    }

    [Fact]
    public async Task GetRecentValuesAsync_ShouldReturnLast10Values()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var fileName = "example";

        var values = new List<ValueEntity>();
        for (int i = 1; i <= 15; i++)
        {
            values.Add(new ValueEntity
            {
                Id = i,
                FileName = fileName,
                Date = new DateTime(2024, 1, 1, 10, i, 0, DateTimeKind.Utc),
                ExecutionTime = i * 0.5,
                Value = i * 10
            });
        }
        context.Values.AddRange(values);
        await context.SaveChangesAsync();

        var service = new ResultService(context);

        // Act
        var result = await service.GetRecentValuesAsync(fileName);

        // Assert
        result.Should().HaveCount(10);
        result.Should().BeInDescendingOrder(v => v.Date);
        result.First().Date.Should().Be(new DateTime(2024, 1, 1, 10, 15, 0, DateTimeKind.Utc));
    }
}