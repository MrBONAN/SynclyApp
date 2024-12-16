using System;

class Program
{
    static void Main()
    {
        // Строка подключения к базе данных
        string connectionString = "Server=localhost\\SQLEXPRESS;Database=MusicAppDB;Integrated Security=True;";

        // Создаём экземпляр класса UserCreator
        var userCreator = new UserCreator(connectionString);

        // Добавляем пользователя с идентификатором изображения
        userCreator.AddUser(
            username: "NewUser1",
            pfpId: "img_123", // Условный ID изображения
            lastLog: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            location: "SomeLocation",
            lastTrack: 1,
            topArtists: "1,2,3,4,5",
            topTracks: "10,11,12,13,14",
            recentTracks: "10,11,12,13,14",
            links: "https://t.me/newuser1",
            friends: "2,3"
        );

        userCreator.AddUser(
            username: "NewUser2",
            pfpId: "img_456", // Условный ID другого изображения
            lastLog: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            location: "AnotherLocation",
            lastTrack: 2,
            topArtists: "6,7,8,9,10",
            topTracks: "15,16,17,18,19",
            recentTracks: "15,16,17,18,19",
            links: "https://t.me/newuser2",
            friends: "1,4"
        );
    }
}