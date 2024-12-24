namespace Domain;

public interface IUserService
{
    Task<User> GetUserAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task RegisterUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}