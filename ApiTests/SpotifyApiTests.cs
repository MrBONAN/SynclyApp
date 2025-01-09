// using Infrastructure.API;
// using Infrastructure.API.SpotifyAPI;
// using Infrastructure.API.SpotifyAPI.Models;
//
// namespace ApiTests;
//
// public class Tests
// {
//     private readonly AccessTokenHandler accessTokenHandler = new AccessTokenHandler();
//
//     [Fact]
//     public void TestGetUserProfileAsync()
//     {
//         var userAccessToken = accessTokenHandler.GetAccessToken();
//         var userProfile = SpotifyApi.GetUserProfileAsync(userAccessToken).Result;
//
//         Assert.Equal(ApiResult.Success, userProfile.Result);
//         Assert.NotNull(userProfile.Data);
//         Assert.Equal("MrB0NAN", userProfile.Data.DisplayName);
//         Assert.Equal("31uvop3r54pirnbq27hhpgjh2vu4", userProfile.Data.Id);
//     }
//
//
//     [Fact]
//     public void TestSearchTrackAsync()
//     {
//         var userAccessToken = accessTokenHandler.GetAccessToken();
//         var query = "Maybe man";
//         var searchResponse = SpotifyApi.SearchFor()
//             .AddAccessToken(userAccessToken)
//             .AddQuestion(query)
//             .SetType(QuestionType.Track)
//             .AddFilter(QuestionFilter.Artist, "AJR")
//             .SetLimit(1)
//             .SendRequest().Result;
//
//         Assert.NotNull(searchResponse.Data);
//         Assert.NotEmpty(searchResponse.Data.Tracks!.Items!);
//         Assert.Equal("7fhiGdj0nn0ZCmIAocG8G0", searchResponse.Data.Tracks.Items.First().Id);
//     }
//
//     [Fact]
//     public void TestGetUserTopTracksAsync()
//     {
//         var userAccessToken = accessTokenHandler.GetAccessToken();
//         var requestedTrackCount = 5;
//         var topTracks = SpotifyApi.GetUserTopItemsAsync<Track>(userAccessToken, requestedTrackCount).Result;
//         Assert.NotNull(topTracks);
//         Assert.Equal(ApiResult.Success, topTracks.Result);
//         Assert.NotNull(topTracks.Data);
//         Assert.Equal(requestedTrackCount, topTracks.Data.Count);
//
//         foreach (var track in topTracks.Data)
//             AssertTrackFields(track);
//     }
//
//     [Fact]
//     public void TestGetSeveralTracksAsync()
//     {
//         var userAccessToken = accessTokenHandler.GetAccessToken();
//         var trackIds = new[] { "26wLOs3ZuHJa2Ihhx6QIE6", "5flerg6aEao2VayZezVlgu", "7LHAKF7pBqHch8o6Yo0ad5" };
//         var severalTracks = SpotifyApi.GetSeveralEntitiesById<Track>(userAccessToken, trackIds).Result;
//
//         Assert.NotNull(severalTracks);
//         Assert.Equal(ApiResult.Success, severalTracks.Result);
//         Assert.NotNull(severalTracks.Data);
//         Assert.Equal(trackIds.Length, severalTracks.Data.Count);
//
//         foreach (var track in severalTracks.Data)
//             AssertTrackFields(track);
//     }
//
//
//     private void AssertTrackFields(Track track)
//     {
//         Assert.NotNull(track.Id);
//         Assert.NotNull(track.Name);
//         Assert.NotNull(track.ExternalUrls);
//         Assert.False(string.IsNullOrEmpty(track.ExternalUrls?.Spotify));
//
//         Assert.NotNull(track.Artists);
//         Assert.NotEmpty(track.Artists);
//
//         foreach (var artist in track.Artists)
//             AssertArtistFields(artist);
//     }
//
//     private static void AssertArtistFields(Artist artist)
//     {
//         Assert.NotNull(artist.Id);
//         Assert.NotNull(artist.Name);
//         Assert.False(string.IsNullOrEmpty(artist.ExternalUrls?.Spotify));
//     }
// }