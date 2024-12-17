using System;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.API.Db
{

    public class Message
    {
        public int ID { get; set; }
        public int Sender { get; set; } // user_data.ID
        public int Receiver { get; set; } // user_data.ID
        public DateTime WhenSent { get; set; }
        public string Text { get; set; }

        public static Message GetById(int messageId, string connectionString)
        {
            Message message = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT ID, sender, receiver, when_sent, text
                FROM messages
                WHERE ID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", messageId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            message = new Message
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Sender = Convert.ToInt32(reader["sender"]),
                                Receiver = Convert.ToInt32(reader["receiver"]),
                                WhenSent = Convert.ToDateTime(reader["when_sent"]),
                                Text = reader["text"].ToString()
                            };
                        }
                    }
                }
            }

            return message;
        }

        public int Insert(string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                INSERT INTO messages (sender, receiver, when_sent, text)
                VALUES (@sender, @receiver, @when_sent, @text);
                SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@sender", Sender);
                    cmd.Parameters.AddWithValue("@receiver", Receiver);
                    cmd.Parameters.AddWithValue("@when_sent", WhenSent);
                    cmd.Parameters.AddWithValue("@text", Text);

                    object insertedId = cmd.ExecuteScalar();
                    this.ID = Convert.ToInt32(insertedId);
                }
            }

            return this.ID;
        }
    }
}