namespace Domain;

public class User : IUserService
{
    public string Name { get; set; }
    public Dictionary<string, string> MusicAppLinks { get; set; }
    public readonly string Id;
    public Location Location { get; set; }
    public IEnumerable<Track> TopTracks { get; set; }
    public IEnumerable<Artist> TopArtists { get; set; }

    public User(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public void AddApp(string app, string musicAppLink)
    {
        MusicAppLinks[app] = musicAppLink;
    }

    public Task<User> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUserAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        throw new NotImplementedException();
    }

    public Task RegisterUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUserAsync(int id)
    {
        throw new NotImplementedException();
    }
}