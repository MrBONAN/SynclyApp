using Infrastructure.API.ServerApi.Models.Links;
using Infrastructure.API.ServerApi.Models.User;

namespace App.UserAuthorization;

public interface IUserDataHandler
{
    Task SaveUserDataAsync(UserDto userData);
    void RemoveUserData();
    Task<UserDto?> GetUserDataAsync();
    Task<int?> GetUserIdAsync();
    Task<LinksDto?> GetUserLinksAsync();
}