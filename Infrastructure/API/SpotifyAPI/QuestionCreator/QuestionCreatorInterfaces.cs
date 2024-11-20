using RestSharp;

namespace Infrastructure.API.SpotifyAPI.QuestionCreator;

public interface IAddQuestionStage
{
    public IAddAccessTokenStage AddQuestion(string question);
}

public interface IAddAccessTokenStage
{
    public IAddSettingsStage AddAccessToken(string accessToken);
    public IAddSettingsStage AddAccessToken(AccessToken accessToken);
}

public interface IAddSettingsStage
{
    public IAddSettingsStage AddFilter(QuestionFilter filter, string filterValue);
    public IAddSettingsStage SetType(QuestionType type);
    public IAddSettingsStage SetLimit(int limit);
    public IAddSettingsStage SetOffset(int offset);
    public Task<RestResponse<SearchResponse>> SendRequest();
    public Task<RestResponse<T>> SendRequest<T>();
}