using System;
using System.Data.SqlClient;

public class UserCreator
{
    private string connectionString;

    // Конструктор класса принимает строку подключения
    public UserCreator(string connectionString)
    {
        this.connectionString = connectionString;
    }

    // Метод для создания пользователя
    public void AddUser(string username, string pfpId, string lastLog, string location, int? lastTrack, string topArtists, string topTracks, string recentTracks, string links, string friends)
    {
        string insertUserQuery = @"
            INSERT INTO user_data 
            (username, pfp, last_log, location, last_track, messages, top_artists, top_tracks, recent_tracks, links, friends) 
            VALUES 
            (@username, @pfp, @last_log, @location, @last_track, @messages, @top_artists, @top_tracks, @recent_tracks, @links, @friends);
        ";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(insertUserQuery, connection))
                {
                    // Передача параметров для предотвращения SQL-инъекций
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@pfp", pfpId); // Айди вместо пути к файлу
                    command.Parameters.AddWithValue("@last_log", lastLog);
                    command.Parameters.AddWithValue("@location", location);
                    command.Parameters.AddWithValue("@last_track", (object)lastTrack ?? DBNull.Value);
                    command.Parameters.AddWithValue("@messages", DBNull.Value); // Пока сообщений нет
                    command.Parameters.AddWithValue("@top_artists", topArtists);
                    command.Parameters.AddWithValue("@top_tracks", topTracks);
                    command.Parameters.AddWithValue("@recent_tracks", recentTracks);
                    command.Parameters.AddWithValue("@links", links);
                    command.Parameters.AddWithValue("@friends", friends);

                    command.ExecuteNonQuery();
                }
                Console.WriteLine("User added successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
