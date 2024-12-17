using System;
using MyNamespace;

class Program
{
    static void Main()
    {
        // Строка подключения к базе данных (замените, если у вас другая СУБД или настройки)
        string connectionString = "Server=localhost\\SQLEXPRESS;Database=MusicAppDB;Integrated Security=True;";

        // 1) Создаём нового артиста и вставляем его в таблицу `artist`.
        Artist newArtist = new Artist
        {
            ArtistName = "Test_Artist",
            PfpIp = "img_Artist123"
        };
        int insertedArtistId = newArtist.Insert(connectionString);
        Console.WriteLine($"[Artist] Добавлен в БД с ID={insertedArtistId}");

        // 2) Создаём новую песню и вставляем в таблицу `song_data`.
        // В поле `artist` передаём ссылку на артиста, которого только что добавили.
        SongData newSong = new SongData
        {
            Title = "Test_Song",
            Artist = insertedArtistId,  // Ссылка на Artist.ID
            Genre = "Test_Genre"
        };
        int insertedSongId = newSong.Insert(connectionString);
        Console.WriteLine($"[SongData] Добавлена в БД с ID={insertedSongId}");

        // 3) Создаём нового пользователя и вставляем в таблицу `user_data`.
        // Поле `last_track` также ссылается на только что добавленную песню.
        UserData newUser = new UserData
        {
            Username = "Test_User",
            Pfp = "img_User001",
            LastLog = "2024-06-09 12:00",   // Формат строки, т.к. в таблице NVARCHAR(50)
            Location = "Test_Location",
            LastTrack = insertedSongId,
            Messages = "Some messages here...", 
            TopArtists = insertedArtistId.ToString(),  // Можно хранить несколько ID через запятую: "1,2,3"
            TopTracks = insertedSongId.ToString(),
            RecentTracks = insertedSongId.ToString(),
            Links = "https://t.me/test_user",
            Friends = "2,3"
        };
        int insertedUserId = newUser.Insert(connectionString);
        Console.WriteLine($"[UserData] Добавлен в БД с ID={insertedUserId}");

        // 4) Создаём сообщение и вставляем его в таблицу `messages`.
        // Для наглядности сделаем отправителем и получателем одного и того же пользователя (self-message).
        var newMsg = new Message
        {
            Sender = insertedUserId,
            Receiver = insertedUserId,
            WhenSent = DateTime.Now,  // Запишем текущее время
            Text = "Hello to myself!"
        };
        int insertedMsgId = newMsg.Insert(connectionString);
        Console.WriteLine($"[Messages] Добавлено в БД с ID={insertedMsgId}");

        Console.WriteLine("\n--- Чтение из БД для проверки ---\n");

        // Теперь считываем обратно записанные данные, используя те же методы GetById(...).
        Artist readArtist = Artist.GetById(insertedArtistId, connectionString);
        SongData readSong = SongData.GetById(insertedSongId, connectionString);
        UserData readUser = UserData.GetById(insertedUserId, connectionString);
        var readMsg = Message.GetById(insertedMsgId, connectionString);

        // Сравниваем поля «до» и «после».
        bool artistMatches = 
            (newArtist.ArtistName == readArtist.ArtistName) &&
            (newArtist.PfpIp == readArtist.PfpIp);

        bool songMatches =
            (newSong.Title == readSong.Title) &&
            (newSong.Artist == readSong.Artist) &&
            (newSong.Genre == readSong.Genre);

        bool userMatches =
            (newUser.Username == readUser.Username) &&
            (newUser.Pfp == readUser.Pfp) &&
            (newUser.LastLog == readUser.LastLog) &&
            (newUser.Location == readUser.Location) &&
            (newUser.LastTrack == readUser.LastTrack) &&
            (newUser.Messages == readUser.Messages) &&
            (newUser.TopArtists == readUser.TopArtists) &&
            (newUser.TopTracks == readUser.TopTracks) &&
            (newUser.RecentTracks == readUser.RecentTracks) &&
            (newUser.Links == readUser.Links) &&
            (newUser.Friends == readUser.Friends);

        bool messageMatches =
            (newMsg.Sender == readMsg.Sender) &&
            (newMsg.Receiver == readMsg.Receiver) &&
            (newMsg.Text == readMsg.Text) &&
            // При сравнении WhenSent иногда возможны миллисекундные расхождения — 
            // но для примера используем простое сравнение
            (newMsg.WhenSent.ToString("yyyy-MM-dd HH:mm:ss") == readMsg.WhenSent.ToString("yyyy-MM-dd HH:mm:ss"));

        // Выводим результат сравнения
        Console.WriteLine($"Artist совпадает: {artistMatches}");
        Console.WriteLine($"SongData совпадает: {songMatches}");
        Console.WriteLine($"UserData совпадает: {userMatches}");
        Console.WriteLine($"Messages совпадает: {messageMatches}");

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}
