using System.Text;
using Infrastructure.Chats;
using System.Collections.Concurrent;

namespace WSS_TEST;

class Program
{
    public static async Task Main()
    {
        try
        {
            await StartTest();
            //await TestMessageEditing();
            //await TestMessageDeleting();
            //await TestMarkAsRead();
            //await TestChatReconnection();
            //await TestRestoringMessages();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при тестировании: {ex}");
        }

        Console.WriteLine("\nЗавершили тестирование чата");
    }

    static async Task StartTest()
    {
        var url = "ws://localhost:8081/ws/";
        var Fridmak = new Chats(22, url);
        var Mot1x = new Chats(52, url);
        var LexaSleep = new Chats(1488, url);

        await Fridmak.RunAsync();
        await Fridmak.SendMessageAsync(1488, "Where is DB??");

        await Fridmak.StopAsync();

        await Task.Delay(200);

        await Mot1x.RunAsync();
        await Mot1x.SendMessageAsync(1488, "Idi nahoi");

        await Task.Delay(200);
        
        await Fridmak.RunAsync();
        
        await Task.Delay(200);
        
        await Fridmak.SendMessageAsync(1488, "DB??? go online");
        await Fridmak.SendMessageAsync(52, "Oh, hi");

        await Task.Delay(200);

        await LexaSleep.RunAsync();

        await Mot1x.SendMessageAsync(22, "hello");

        await Task.Delay(200);

        await LexaSleep.SendMessageAsync(22, "BLYAT NET NIHUIA");
        await LexaSleep.SendMessageAsync(52, "Yes, i love you too!");

        await Task.Delay(400);

        foreach (var (id, messages) in LexaSleep.GetAllChats())
        {
            Console.WriteLine($"Chat with {id}:");
            foreach (var message in messages)
                Console.WriteLine(message);
        }
    }

    static async Task TestMessageEditing()
    {
        Console.WriteLine("\n--- Тест редактирования сообщений ---");
        var url = "ws://localhost:8081/ws/";
        var Fridmak = new Chats(22, url);
        var Mot1x = new Chats(52, url);

        await Fridmak.RunAsync();
        await Mot1x.RunAsync();

        // Fridmak отправляет сообщение Mot1x
        await Fridmak.SendMessageAsync(52, "Initial Message");
        await Task.Delay(200);

        var messages = Mot1x.GetMessages(22);
        var messageToEdit = messages.FirstOrDefault();
        if (messageToEdit != default)
        {
            // Fridmak редактирует сообщение
            await Fridmak.EditMessage(52, messageToEdit.Item2, "Edited Message");
        }

        await Task.Delay(200);
        Console.WriteLine("Сообщения у Mot1x:");
        foreach (var msg in Mot1x.GetMessages(22))
        {
            Console.WriteLine(msg);
        }
    }

    static async Task TestMessageDeleting()
    {
        Console.WriteLine("\n--- Тест удаления сообщений ---");
        var url = "ws://localhost:8081/ws/";
        var Fridmak = new Chats(22, url);
        var Mot1x = new Chats(52, url);

        await Fridmak.RunAsync();
        await Mot1x.RunAsync();

        // Fridmak отправляет сообщение
        await Fridmak.SendMessageAsync(52, "Message to Delete");
        await Task.Delay(200);

        var messages = Mot1x.GetMessages(22);
        var messageToDelete = messages.FirstOrDefault();
        if (messageToDelete != default)
        {
            // Fridmak удаляет сообщение
            await Fridmak.DeleteMessage(52, messageToDelete.Item2);
        }

        await Task.Delay(200);
        Console.WriteLine("Сообщения у Mot1x (должно быть пусто):");
        foreach (var msg in Mot1x.GetMessages(22))
        {
            Console.WriteLine(msg);
        }
    }

    static async Task TestMarkAsRead()
    {
        Console.WriteLine("\n--- Тест отметки сообщений как прочитанных ---");
        var url = "ws://localhost:8081/ws/";
        var Fridmak = new Chats(22, url);
        var Mot1x = new Chats(52, url);

        await Fridmak.RunAsync();
        await Mot1x.RunAsync();

        // Fridmak отправляет сообщение Mot1x
        await Fridmak.SendMessageAsync(52, "Unread Message");
        await Task.Delay(200);

        Console.WriteLine($"Количество непрочитанных сообщений у Mot1x: {Mot1x.GetUnreadCount(22)}");

        // Mot1x помечает сообщения как прочитанные
        Mot1x.MarkAsRead(22);
        Console.WriteLine($"Количество непрочитанных сообщений у Mot1x после отметки: {Mot1x.GetUnreadCount(22)}");
    }

    static async Task TestChatReconnection()
    {
        Console.WriteLine("\n--- Тест восстановления подключения ---");
        var url = "ws://localhost:8081/ws/";
        var Fridmak = new Chats(22, url);

        await Fridmak.RunAsync();
        Console.WriteLine($"Статус подключения Fridmak: {Fridmak.IsConnected}");

        // Принудительно разрываем соединение
        await Fridmak.StopAsync();
        Console.WriteLine($"Статус подключения Fridmak после остановки: {Fridmak.IsConnected}");

        // Восстанавливаем соединение
        await Fridmak.ReconnectAsync();
        Console.WriteLine($"Статус подключения Fridmak после восстановления: {Fridmak.IsConnected}");
    }

    static async Task TestRestoringMessages()
    {
        Console.WriteLine("\n--- Тест восстановления сообщений ---");
        var url = "ws://localhost:8081/ws/";
        var Fridmak = new Chats(22, url);
        var Mot1x = new Chats(52, url);

        await Fridmak.RunAsync();
        await Mot1x.RunAsync();

        // Fridmak отправляет сообщение
        await Fridmak.SendMessageAsync(52, "Message to Restore");
        await Task.Delay(200);

        // Получаем сообщение у Mot1x
        var messages = Mot1x.GetMessages(22);
        var messageToRestore = messages.FirstOrDefault();
        if (messageToRestore != default)
        {
            // Восстанавливаем сообщение
            await Mot1x.RestoreMessage(22, messageToRestore.Item1, messageToRestore.Item2);
        }

        Console.WriteLine("Сообщения у Mot1x после восстановления:");
        foreach (var msg in Mot1x.GetMessages(22))
        {
            Console.WriteLine(msg);
        }
    }
}
