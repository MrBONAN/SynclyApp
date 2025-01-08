using Infrastructure.API.ServerApi;
using Infrastructure.API.ServerApi.Models.User;

namespace ServerApiTests;

public class UserGetTests
{
    [Test]
    public async Task GetUserProfileTest()
    {
        var response = await ServerApi.GetUser(1);
        Assert.That(response.Result, Is.EqualTo(ApiResult.Ok));
        Assert.NotNull(response.Data);
        var userProfile = response.Data!;
        Assert.That(userProfile.Name, Is.EqualTo("MrB0NAN"));
        Assert.That(userProfile.Links.ExternalId, Is.EqualTo("31uvop3r54pirnbq27hhpgjh2vu4"));
    }
    
    [Test]
    public async Task GetAllUsersTest()
    {
        var response = await ServerApi.GetAllUsers();
        Assert.That(response.Result, Is.EqualTo(ApiResult.Ok));
        Assert.NotNull(response.Data);
        foreach (var userDto in response.Data)
            CheckUser(userDto);
    }

    private void CheckUser(UserDto userProfile)
    {
        Assert.IsTrue(userProfile.Name != null);
        Assert.IsTrue(userProfile.Links != null);
        Assert.IsTrue(userProfile.Links!.ExternalId != null);
        Assert.IsTrue(userProfile.Links.ExternalLink != null);
        Assert.IsTrue(userProfile.Links.ExternalImageLink != null);
    }
}