
using System.Net.Http.Json;

namespace TerraRun.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    private const string BaseUrl = "http://10.0.2.2:5134/api/Auth/";

    public AuthService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<UserResponseDto> Register(string username, string email, string password)
    {
        var data = new { Username = username, Email = email, Password = password };
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}register", data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserResponseDto>();
        }
        return null;
    }

    public async Task<UserResponseDto?> Login(string username, string password)
    {
        var data = new {Username = username, Password = password};
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}login",  data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserResponseDto>();
        }
        return null;
    }
}
public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
