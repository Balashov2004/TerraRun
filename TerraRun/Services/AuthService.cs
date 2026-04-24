using System.Net.Http.Json;
using TerraRun.Interfaces;
using TerraRun.Models;

namespace TerraRun.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://10.0.2.2:5134/api/Auth/")
        };
    }

    public async Task<UserResponseDto?> Register(string username, string email, string password)
    {
        try
        {
            var request = new RegisterRequest(username, email, password);
            var response = await _httpClient.PostAsJsonAsync("register", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserResponseDto>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AUTH ERROR] Register: {ex.Message}");
        }
        return null;
    }

    public async Task<UserResponseDto?> Login(string username, string password)
    {
        try
        {
            var request = new LoginRequest(username, password);
            var response = await _httpClient.PostAsJsonAsync("login", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserResponseDto>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AUTH ERROR] Login: {ex.Message}");
        }
        return null;
    }
}