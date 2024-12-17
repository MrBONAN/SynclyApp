using System;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.API.Db
{
    public class Artist
    {
        public int ID { get; set; }
        public string? ArtistName { get; set; }
        public string? PfpIp { get; set; }

        // Пример чтения (получения) артиста по ID
        public static Artist GetById(int artistId, string connectionString)
        {
            Artist artist = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Запрос: выбираем строку из таблицы artist по нужному ID
                string query = "SELECT ID, artist_name, pfp_ip FROM artist WHERE ID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", artistId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            artist = new Artist
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                ArtistName = reader["artist_name"].ToString(),
                                PfpIp = reader["pfp_ip"].ToString()
                            };
                        }
                    }
                }
            }

            return artist;
        }

        // Пример вставки нового артиста в таблицу.
        // Возвращает ID добавленной записи.
        public int Insert(string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // При вставке ID генерируется за счёт IDENTITY в таблице
                // Поэтому после INSERT сделаем SELECT SCOPE_IDENTITY(), чтобы вернуть ID
                string query = @"
                INSERT INTO artist (artist_name, pfp_ip) 
                VALUES (@name, @pfp_ip);
                SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", ArtistName);
                    cmd.Parameters.AddWithValue("@pfp_ip", PfpIp);

                    // Выполняем и получаем сгенерированный ключ
                    object insertedId = cmd.ExecuteScalar();
                    ID = Convert.ToInt32(insertedId);
                }
            }

            return ID;
        }
    }
}