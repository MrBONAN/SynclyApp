using Infrastructure.API.ServerApi.Models.Links;
using Infrastructure.API.ServerApi.Models.User;
using System.Text.Json;

namespace App.UserAuthorization;

public class UserDataHandler : IUserDataHandler
{
    private const string UserDataKey = "user_data";
    
    public async Task SaveUserDataAsync(UserDto userData)
    {
        var jsonUserData = JsonSerializer.Serialize(userData);
        await SecureStorage.Default.SetAsync(UserDataKey, jsonUserData);
    }

    public void RemoveUserData()
    {
        SecureStorage.Default.Remove(UserDataKey);
    }

    public async Task<UserDto?> GetUserDataAsync()
    {
        var jsonUserData = await SecureStorage.Default.GetAsync(UserDataKey);
        if (jsonUserData is null)
            return null;
        return JsonSerializer.Deserialize<UserDto>(jsonUserData);
    }

    public async Task<int?> GetUserIdAsync()
    {
        return (await GetUserDataAsync())?.Id;
    }

    public async Task<LinksDto?> GetUserLinksAsync()
    {
        return (await GetUserDataAsync())?.Links;
    }
}