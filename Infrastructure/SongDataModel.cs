using System;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.API.Db
{
    public class SongData
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public int Artist { get; set; } // Ссылка на artist.ID
        public string Genre { get; set; }

        public static SongData GetById(int songId, string connectionString)
        {
            SongData songData = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ID, title, artist, genre FROM song_data WHERE ID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", songId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            songData = new SongData
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Title = reader["title"].ToString(),
                                Artist = Convert.ToInt32(reader["artist"]),
                                Genre = reader["genre"].ToString()
                            };
                        }
                    }
                }
            }

            return songData;
        }

        public int Insert(string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = @"
                INSERT INTO song_data (title, artist, genre) 
                VALUES (@title, @artist, @genre);
                SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", Title);
                    cmd.Parameters.AddWithValue("@artist", Artist);
                    cmd.Parameters.AddWithValue("@genre", Genre);

                    var insertedId = cmd.ExecuteScalar();
                    this.ID = Convert.ToInt32(insertedId);
                }
            }

            return this.ID;
        }
    }
}