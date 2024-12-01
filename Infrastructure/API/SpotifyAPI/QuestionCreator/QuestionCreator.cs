using RestSharp;

namespace Infrastructure.API.SpotifyAPI.QuestionCreator;

public class QuestionCreator : IAddQuestionStage, IAddAccessTokenStage, IAddSettingsStage
{
    private string AccessToken { get; set; } = "";
    private string Question { get; set; } = "";
    private Dictionary<QuestionFilter, string> QuestionFilters { get; set; } = new();
    private QuestionType? QuestionType { get; set; }
    private int QuestionLimit { get; set; } = 20;
    private int QuestionOffset { get; set; }

    public IAddQuestionStage AddAccessToken(string accessToken)
    {
        AccessToken = accessToken;
        return this;
    }

    public IAddQuestionStage AddAccessToken(AccessToken accessToken)
    {
        AccessToken = accessToken.TokenValue ?? throw new NullReferenceException("AccessToken without token");
        return this;
    }

    public IAddSettingsStage AddQuestion(string question)
    {
        Question = question;
        return this;
    }

    public IAddSettingsStage AddFilter(QuestionFilter filter, string filterValue)
    {
        QuestionFilters[filter] = filterValue;
        return this;
    }

    public IAddSettingsStage SetType(QuestionType type)
    {
        QuestionType = type;
        return this;
    }

    public IAddSettingsStage SetLimit(int limit)
    {
        QuestionLimit = limit;
        return this;
    }

    public IAddSettingsStage SetOffset(int offset)
    {
        QuestionOffset = offset;
        return this;
    }

    private RestRequest CreateRequest()
    {
        var questionFilters = String.Join(" ", QuestionFilters.Select(pair => $"{pair.Key.ToString()}:{pair.Value}"));
        var request = new RestRequest()
            .AddHeader("Authorization", $"Bearer {AccessToken}")
            .AddQueryParameter("q", Question + " " + questionFilters)
            .AddQueryParameter("limit", QuestionLimit)
            .AddQueryParameter("Offset", QuestionOffset);
        if (QuestionType is not null)
            request.AddQueryParameter("type", QuestionType.ToString()!.ToLower());

        return request;
    }

    public async Task<RestResponse<SearchResponse>> SendRequest()
    {
        var client = new RestClient("https://api.spotify.com/v1/search");
        var request = CreateRequest();
        return await client.ExecuteGetAsync<SearchResponse>(request);
    }

    public async Task<RestResponse<T>> SendRequest<T>()
    {
        var client = new RestClient("https://api.spotify.com/v1/search");
        var request = CreateRequest();
        return await client.ExecuteGetAsync<T>(request);
    }
}