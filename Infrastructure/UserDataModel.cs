using System;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.API.Db
{

    public class UserData
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Pfp { get; set; }
        public string? LastLog { get; set; } // или DateTime, если хранить дату как DateTime
        public string? Location { get; set; }
        public int? LastTrack { get; set; } // Может быть null (в БД FOREIGN KEY), если нет трека
        public string? Messages { get; set; } // NVARCHAR(MAX)
        public string? TopArtists { get; set; } // "1,2,3"
        public string? TopTracks { get; set; }
        public string? RecentTracks { get; set; }
        public string? Links { get; set; }
        public string? Friends { get; set; }

        public static UserData GetById(int userId, string connectionString)
        {
            UserData user = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT ID, username, pfp, last_log, location, last_track, messages, 
                       top_artists, top_tracks, recent_tracks, links, friends
                FROM user_data
                WHERE ID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new UserData
                            {
                                Id = Convert.ToInt32(reader["ID"]),
                                Username = reader["username"].ToString(),
                                Pfp = reader["pfp"].ToString(),
                                LastLog = reader["last_log"].ToString(), // или Convert.ToDateTime(...)
                                Location = reader["location"].ToString(),
                                LastTrack = reader["last_track"] == DBNull.Value ? null : (int?)reader["last_track"],
                                Messages = reader["messages"] == DBNull.Value ? null : reader["messages"].ToString(),
                                TopArtists = reader["top_artists"] == DBNull.Value
                                    ? null
                                    : reader["top_artists"].ToString(),
                                TopTracks = reader["top_tracks"] == DBNull.Value
                                    ? null
                                    : reader["top_tracks"].ToString(),
                                RecentTracks = reader["recent_tracks"] == DBNull.Value
                                    ? null
                                    : reader["recent_tracks"].ToString(),
                                Links = reader["links"] == DBNull.Value ? null : reader["links"].ToString(),
                                Friends = reader["friends"] == DBNull.Value ? null : reader["friends"].ToString()
                            };
                        }
                    }
                }
            }

            return user;
        }

        public int Insert(string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                INSERT INTO user_data 
                (username, pfp, last_log, location, last_track, messages, top_artists, top_tracks, recent_tracks, links, friends)
                VALUES
                (@username, @pfp, @last_log, @location, @last_track, @messages, @top_artists, @top_tracks, @recent_tracks, @links, @friends);
                SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", Username);
                    cmd.Parameters.AddWithValue("@pfp", Pfp);
                    cmd.Parameters.AddWithValue("@last_log", LastLog); // Или Convert.ToDateTime(...) если нужно
                    cmd.Parameters.AddWithValue("@location", Location);
                    cmd.Parameters.AddWithValue("@last_track", (object?)LastTrack ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@messages", (object?)Messages ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@top_artists", (object?)TopArtists ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@top_tracks", (object?)TopTracks ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@recent_tracks", (object?)RecentTracks ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@links", (object?)Links ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@friends", (object?)Friends ?? DBNull.Value);

                    object insertedId = cmd.ExecuteScalar();
                    this.Id = Convert.ToInt32(insertedId);
                }
            }

            return this.Id;
        }
    }
}