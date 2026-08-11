using CSharpTimescaleAPI.Data;
using CSharpTimescaleAPI.Models;
using CSharpTimescaleAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using FluentAssertions;
using Xunit;

namespace CSharpTimescaleAPI.Tests.Services;

public class CsvProcessingServiceTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ProcessCsvAsync_ValidFile_ShouldSaveDataAndReturnSuccess()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CsvProcessingService(context);

        var csvContent = "2024-01-15T10-30-45.1234Z;1.5;100.2\n" +
                         "2024-01-15T10-31-45.5678Z;2.3;150.7";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var fileName = "test";

        // Act
        var result = await service.ProcessCsvAsync(stream, fileName);

        // Assert
        result.Success.Should().BeTrue();
        result.RecordsCount.Should().Be(2);
        result.FileName.Should().Be(fileName);

        var savedValues = await context.Values.ToListAsync();
        savedValues.Should().HaveCount(2);
        savedValues.All(v => v.FileName == fileName).Should().BeTrue();

        var savedResults = await context.Results.ToListAsync();
        savedResults.Should().HaveCount(1);
        savedResults.First().FileName.Should().Be(fileName);
    }

    [Fact]
    public async Task ProcessCsvAsync_InvalidDateFormat_ShouldReturnError()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CsvProcessingService(context);

        var csvContent = "2024/01/15 10:30:45;1.5;100.2\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await service.ProcessCsvAsync(stream, "test");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("неверный формат даты");
    }

    [Fact]
    public async Task ProcessCsvAsync_ValueLessThanZero_ShouldReturnError()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CsvProcessingService(context);

        var csvContent = "2024-01-15T10-30-45.1234Z;1.5;-100.2\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await service.ProcessCsvAsync(stream, "test");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("значение должно быть >= 0");
    }

    [Fact]
    public async Task ProcessCsvAsync_EmptyFile_ShouldReturnError()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CsvProcessingService(context);

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));

        // Act
        var result = await service.ProcessCsvAsync(stream, "test");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("вне диапазона 1..10000");
    }
}