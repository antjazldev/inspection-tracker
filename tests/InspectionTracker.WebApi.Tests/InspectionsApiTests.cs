using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using InspectionTracker.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using InspectionTracker.Domain.Enums;

namespace InspectionTracker.WebApi.Tests;

public class InspectionsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InspectionsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("demo@inspectiontracker.com", "Demo1234!"));
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }

    private void UseToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    [Fact]
    public async Task GetAll_WithoutAuth_Returns200()
    {
        var response = await _client.GetAsync("/api/inspections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/inspections",
            new InspectionRequest("Trimmer 099", InspectionStatus.Passed, null, DateTime.UtcNow.AddHours(-1)));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithAuth_Returns201()
    {
        UseToken(await LoginAsync());

        var response = await _client.PostAsJsonAsync("/api/inspections",
            new InspectionRequest("Trimmer 099", InspectionStatus.Passed, null, DateTime.UtcNow.AddHours(-1)));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<InspectionResponse>();
        created!.AssetName.Should().Be("Trimmer 099");
    }

    [Fact]
    public async Task Create_FailedWithoutNotes_Returns400()
    {
        UseToken(await LoginAsync());

        var response = await _client.PostAsJsonAsync("/api/inspections",
            new InspectionRequest("Trimmer 099", InspectionStatus.Failed, null, DateTime.UtcNow.AddHours(-1)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("demo@inspectiontracker.com", "wrong"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/inspections/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
