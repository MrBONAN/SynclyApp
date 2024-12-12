using RestSharp;
using Infrastructure.API.SpotifyAPI.Models;

namespace Infrastructure.API.SpotifyAPI.SearchQuestionCreator;

public interface IAddAccessTokenStage
{
    public IAddQuestionStage AddAccessToken(string accessToken);
    public IAddQuestionStage AddAccessToken(AccessToken accessToken);
}

public interface IAddQuestionStage
{
    public IAddSettingsStage AddQuestion(string question);
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