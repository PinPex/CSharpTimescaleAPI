using CSharpTimescaleAPI.Data;
using CSharpTimescaleAPI.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Json;
using System.Text;
using Testcontainers.PostgreSql;
using Xunit;
using FluentAssertions;

namespace CSharpTimescaleAPI.Tests.Integration;

public class ValuesControllerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public ValuesControllerIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:16")
            .WithPassword("mysecretpassword")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<AppDbContext>();
                    services.RemoveAll<DbContextOptions<AppDbContext>>();

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(_postgresContainer.GetConnectionString()));
                });
            });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task UploadCsv_ValidFile_ShouldReturnSuccess()
    {
        // Arrange
        var csvContent = "2024-01-15T10-30-45.1234Z;1.5;100.2\n" +
                         "2024-01-15T10-31-45.5678Z;2.3;150.7";
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "test.csv");

        // Act
        var response = await _client!.PostAsync("/api/Values/upload", content);
        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.RecordsCount.Should().Be(2);
    }

    [Fact]
    public async Task GetResults_ShouldReturnAllResults()
    {
        // Arrange
        var csvContent = "2024-01-15T10-30-45.1234Z;1.5;100.2\n" +
                         "2024-01-15T10-31-45.5678Z;2.3;150.7";
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "test.csv");
        await _client!.PostAsync("/api/Values/upload", content);

        // Act
        var response = await _client!.GetAsync("/api/Values/results");
        var results = await response.Content.ReadFromJsonAsync<List<ResultEntity>>();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        results.Should().NotBeEmpty();
        results!.First().FileName.Should().Be("test");
    }

    [Fact]
    public async Task GetRecentValues_ShouldReturnLast10Values()
    {
        // Arrange
        var csvContent = "2024-01-15T10-30-45.1234Z;1.5;100.2\n" +
                         "2024-01-15T10-31-45.5678Z;2.3;150.7\n" +
                         "2024-01-15T10-32-45.9012Z;1.8;120.5\n" +
                         "2024-01-15T10-33-45.3456Z;2.1;130.1\n" +
                         "2024-01-15T10-34-45.7890Z;1.9;110.3\n" +
                         "2024-01-15T10-35-45.2345Z;2.0;140.8\n" +
                         "2024-01-15T10-36-45.6789Z;1.7;115.6\n" +
                         "2024-01-15T10-37-45.0123Z;2.5;160.2\n" +
                         "2024-01-15T10-38-45.4567Z;1.6;105.9\n" +
                         "2024-01-15T10-39-45.8901Z;2.4;155.4\n" +
                         "2024-01-15T10-40-45.2345Z;1.3;95.7";
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "test.csv");
        await _client!.PostAsync("/api/Values/upload", content);

        // Act
        var response = await _client!.GetAsync("/api/Values/recent/test");
        var result = await response.Content.ReadFromJsonAsync<RecentValuesResponse>();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Values.Should().HaveCount(10);
        result.Values.Should().BeInDescendingOrder(v => v.Date);
        result.Values.First().Value.Should().Be(95.7);
    }

    public async Task DisposeAsync()
    {
        if (_factory != null)
            await _factory.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}