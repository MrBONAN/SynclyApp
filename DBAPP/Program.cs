using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        // Строка подключения к базе данных
        string connectionString = "Server=localhost\\SQLEXPRESS;Database=MusicAppDB;Integrated Security=True;";

        // SQL-запрос для добавления записи в таблицу user_data
        string insertRecord = @"
            INSERT INTO user_data (username, email, password) 
            VALUES (@username, @email, @password);
        ";

        // Добавляем запись
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(insertRecord, connection))
                {
                    // Добавляем параметры для безопасности
                    for (var i = 0; i < 100; i++)
                    {
                        
                    }
                    command.Parameters.AddWithValue("@username", "test_user2");
                    command.Parameters.AddWithValue("@email", "test2@example.com");
                    command.Parameters.AddWithValue("@password", "2securepassword123");

                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Record inserted successfully. Rows affected: {rowsAffected}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}