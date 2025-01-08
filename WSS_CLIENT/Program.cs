using System.Text;
using Infrastructure.Chats;

namespace WSS_TEST;

class Program
{
    public static async Task Main()
    {
        Console.WriteLine("Начинаем тестирование распределенных сообщений");

        TestYourData().GetAwaiter().GetResult();
        
        Console.WriteLine("Завершили тестирование распределенных сообщений");
        while (true){}
    }

    private static async Task TestYourData()
    {
        var firstUser = new Chats(52);
        var secondUser = new Chats(1337);
        var thirdUser = new Chats(1488);

        await firstUser.RunAsync();
        await secondUser.RunAsync();

        var tasks = new List<Task>
        {
            firstUser.SendMessageAsync(1337, "Hello, 52 SPB"),
            secondUser.SendMessageAsync(52, "Hello, MemeCreator"),
            firstUser.SendMessageAsync(1488, "Heil Hitler"),
            secondUser.SendMessageAsync(1488, "Heil Hitler"),
        };

        await Task.WhenAll(tasks);

        await thirdUser.RunAsync();

        await Task.Delay(100);
        
        Console.WriteLine();
        Console.WriteLine(FormatChatContents(thirdUser.GetAllChats()));
        Console.WriteLine();

        await Task.Delay(1000);

        await Task.WhenAll(firstUser.StopChat(), secondUser.StopChat(), thirdUser.StopChat());

        await Task.Delay(100);
    }

    public static void AssertMessages(string clientName, List<string> actualMessages, List<string> expectedMessages)
    {
        foreach (var expected in expectedMessages)
        {
            if (!actualMessages.Any(m => m.StartsWith(expected)))
            {
                Console.WriteLine($"Ошибка: Сообщение для {clientName} не найдено: \"{expected}\"");
            }
            else
            {
                Console.WriteLine($"Сообщение для {clientName} успешно найдено: \"{expected}\"");
            }
        }
    }
    
    public static string FormatChatContents(Dictionary<int, List<(string message, string time)>> chatContents)
    {
        var formattedChat = new StringBuilder();

        foreach (var entry in chatContents)
        {
            int clientId = entry.Key;
            var messages = entry.Value;

            formattedChat.AppendLine($"Chat with Client ID: {clientId}");
            foreach (var (message, time) in messages)
            {
                formattedChat.AppendLine($"[{time}] {message}");
            }
            formattedChat.AppendLine();
        }

        return formattedChat.ToString().Trim();
    }
}