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
            // await RunBasicMessageTest();
            // await RunReconnectionTest();
            // await RunMultipleMessagesTest();
            // await RunConcurrentMessagesTest();
            // await RunLongMessageTest();
            // await RunMultipleClientsTest();
            // await RunMessageOrderTest();
            // await RunStressTest();
            // await RunDisconnectReconnectTest();
            // await RunChatHistoryTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при тестировании: {ex}");
        }
        
        Console.WriteLine("\nЗавершили тестирование чата");
    }

    static async Task StartTest()
    {
        var Fridmak = new Chats(22);
        var Mot1x = new Chats(52);
        var LexaSleep = new Chats(1488);
        
        await Fridmak.RunAsync();
        await Fridmak.SendMessageAsync(1488, "Where is DB??");
        
        await Mot1x.RunAsync();
        await Mot1x.SendMessageAsync(1488, "Idi nahoi");

        await Fridmak.SendMessageAsync(1488, "DB??? go online");
        await Fridmak.SendMessageAsync(52, "Oh, hi");
        
        await LexaSleep.RunAsync();

        await Mot1x.SendMessageAsync(22, "hello");

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
    
    static async Task RunBasicMessageTest()
    {
        Console.WriteLine("\n=== Базовый тест отправки сообщений ===\n");
        
        var client1 = new ChatsHandler(1, () => { }, "ws://localhost:8080/ws/");
        await client1.StartSetUp();
        Console.WriteLine("Клиент 1 подключен");

        await client1.SendMessage(2, "Привет клиенту 2 (оффлайн)");
        Console.WriteLine("Клиент 1 отправил сообщение клиенту 2 (оффлайн)");

        var client2 = new ChatsHandler(2, () => { }, "ws://localhost:8080/ws/");
        await client2.StartSetUp();
        Console.WriteLine("Клиент 2 подключен");

        Console.WriteLine("\nСообщения клиента 2:");
        foreach (var (message, time) in client2.GetMessages(1))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nКлиент 2 отправил ответ");
        await client2.SendMessage(1, "Привет клиенту 1 (онлайн)");

        await Task.Delay(1000); // Даем время на получение сообщения

        Console.WriteLine("\nСообщения клиента 1:");
        foreach (var (message, time) in client1.GetMessages(2))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    static async Task RunReconnectionTest()
    {
        Console.WriteLine("\n=== Тест переподключения ===\n");

        var client1 = new ChatsHandler(3, () => { }, "ws://localhost:8080/ws/");
        await client1.StartSetUp();
        Console.WriteLine("Клиент 3 подключен");

        await client1.SendMessage(4, "Сообщение до отключения");
        Console.WriteLine("Клиент 3 отправил сообщение");

        // Имитируем разрыв соединения путем создания нового экземпляра
        client1 = new ChatsHandler(3, () => { }, "ws://localhost:8080/ws/");
        await client1.StartSetUp();
        Console.WriteLine("Клиент 3 переподключен");

        await client1.SendMessage(4, "Сообщение после переподключения");
        Console.WriteLine("Клиент 3 отправил сообщение после переподключения");

        var client2 = new ChatsHandler(4, () => { }, "ws://localhost:8080/ws/");
        await client2.StartSetUp();
        Console.WriteLine("Клиент 4 подключен");

        Console.WriteLine("\nСообщения клиента 4:");
        foreach (var (message, time) in client2.GetMessages(3))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    static async Task RunMultipleMessagesTest()
    {
        Console.WriteLine("\n=== Тест множественных сообщений ===\n");

        var client1 = new ChatsHandler(5, () => { }, "ws://localhost:8080/ws/");
        var client2 = new ChatsHandler(6, () => { }, "ws://localhost:8080/ws/");

        await client1.StartSetUp();
        await client2.StartSetUp();
        Console.WriteLine("Оба клиента подключены");

        // Отправляем несколько сообщений
        for (int i = 1; i <= 5; i++)
        {
            await client1.SendMessage(6, $"Сообщение {i} от клиента 5");
            await Task.Delay(100);
        }

        await Task.Delay(1000); // Даем время на получение всех сообщений

        Console.WriteLine("\nСообщения клиента 6:");
        foreach (var (message, time) in client2.GetMessages(5))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    static async Task RunConcurrentMessagesTest()
    {
        Console.WriteLine("\n=== Тест одновременной отправки сообщений ===\n");

        var client1 = new ChatsHandler(7, () => { }, "ws://localhost:8080/ws/");
        var client2 = new ChatsHandler(8, () => { }, "ws://localhost:8080/ws/");

        await client1.StartSetUp();
        await client2.StartSetUp();
        Console.WriteLine("Оба клиента подключены");

        // Одновременно отправляем сообщения с обеих сторон
        var tasks = new List<Task>();
        for (int i = 1; i <= 3; i++)
        {
            var i1 = i;
            tasks.Add(client1.SendMessage(8, $"Сообщение {i1} от клиента 7"));
            tasks.Add(client2.SendMessage(7, $"Сообщение {i1} от клиента 8"));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(1000); // Даем время на получение всех сообщений

        Console.WriteLine("\nСообщения клиента 7:");
        foreach (var (message, time) in client1.GetMessages(8))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nСообщения клиента 8:");
        foreach (var (message, time) in client2.GetMessages(7))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    static async Task RunLongMessageTest()
    {
        Console.WriteLine("\n=== Тест длинных сообщений ===\n");

        var client1 = new ChatsHandler(9, () => { }, "ws://localhost:8080/ws/");
        var client2 = new ChatsHandler(10, () => { }, "ws://localhost:8080/ws/");

        await client1.StartSetUp();
        await client2.StartSetUp();
        Console.WriteLine("Оба клиента подключены");

        // Создаем длинное сообщение
        var longMessage = new string('A', 1000) + new string('B', 1000) + new string('C', 1000);
        await client1.SendMessage(10, longMessage);
        Console.WriteLine("Отправлено длинное сообщение");

        await Task.Delay(1000);

        var messages = client2.GetMessages(9);
        Console.WriteLine($"\nПолучено сообщение длиной {messages.First().Item1.Length} символов");
        Console.WriteLine($"Сообщение получено корректно: {messages.First().Item1 == longMessage}");

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    static async Task RunMultipleClientsTest()
    {
        Console.WriteLine("\n=== Тест множества клиентов ===\n");

        var clients = new List<ChatsHandler>();
        var clientCount = 5;
        
        // Создаем и подключаем клиентов
        for (int i = 0; i < clientCount; i++)
        {
            var client = new ChatsHandler(11 + i, () => { }, "ws://localhost:8080/ws/");
            await client.StartSetUp();
            clients.Add(client);
            Console.WriteLine($"Клиент {11 + i} подключен");
        }

        // Каждый клиент отправляет сообщение всем остальным
        foreach (var sender in clients)
        {
            foreach (var receiver in clients)
            {
                if (sender != receiver)
                {
                    await sender.SendMessage(receiver.GetType().GetField("_myId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(receiver) as int? ?? 0,
                        $"Привет от клиента {sender.GetType().GetField("_myId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(sender)}!");
                }
            }
        }

        await Task.Delay(1000);

        // Проверяем, что каждый клиент получил сообщения от всех остальных
        foreach (var receiver in clients)
        {
            Console.WriteLine($"\nСообщения для клиента {receiver.GetType().GetField("_myId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(receiver)}:");
            foreach (var sender in clients)
            {
                if (sender != receiver)
                {
                    var messages = receiver.GetMessages(sender.GetType().GetField("_myId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(sender) as int? ?? 0);
                    foreach (var (message, time) in messages)
                    {
                        Console.WriteLine($"[{time}] {message}");
                    }
                }
            }
        }

        Console.WriteLine("\nТест завершен");
        foreach (var client in clients)
        {
            await client.DisposeAsync();
        }
    }

    static async Task RunMessageOrderTest()
    {
        Console.WriteLine("\n=== Тест порядка сообщений ===\n");

        var client1 = new ChatsHandler(16, () => { }, "ws://localhost:8080/ws/");
        var client2 = new ChatsHandler(17, () => { }, "ws://localhost:8080/ws/");

        await client1.StartSetUp();
        await client2.StartSetUp();
        Console.WriteLine("Оба клиента подключены");

        // Отправляем сообщения с метками
        for (int i = 1; i <= 10; i++)
        {
            await client1.SendMessage(17, $"Сообщение {i}");
            await Task.Delay(10); // Небольшая задержка между сообщениями
        }

        await Task.Delay(1000);

        var messages = client2.GetMessages(16);
        Console.WriteLine("\nПроверяем порядок сообщений:");
        int expectedNumber = 1;
        foreach (var (message, time) in messages)
        {
            var currentNumber = int.Parse(message.Split(' ')[1]);
            Console.WriteLine($"[{time}] {message} - {(currentNumber == expectedNumber ? "OK" : "ОШИБКА ПОРЯДКА")}");
            expectedNumber++;
        }

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    static async Task RunStressTest()
    {
        Console.WriteLine("\n=== Стресс-тест ===\n");

        var client1 = new ChatsHandler(18, () => { }, "ws://localhost:8080/ws/");
        var client2 = new ChatsHandler(19, () => { }, "ws://localhost:8080/ws/");

        await client1.StartSetUp();
        await client2.StartSetUp();
        Console.WriteLine("Оба клиента подключены");

        var messageCount = 100;
        var tasks = new List<Task>();
        var sentMessages = new ConcurrentBag<string>();

        // Быстро отправляем много сообщений
        for (int i = 1; i <= messageCount; i++)
        {
            var message = $"Стресс-сообщение {i}";
            sentMessages.Add(message);
            tasks.Add(client1.SendMessage(19, message));
            if (i % 10 == 0) // Небольшая пауза каждые 10 сообщений
                await Task.Delay(10);
        }

        await Task.WhenAll(tasks);
        await Task.Delay(2000); // Даем время на получение всех сообщений

        var receivedMessages = client2.GetMessages(18).Select(m => m.Item1).ToList();
        var missingMessages = sentMessages.Except(receivedMessages).ToList();
        var unexpectedMessages = receivedMessages.Except(sentMessages).ToList();

        Console.WriteLine($"\nОтправлено сообщений: {messageCount}");
        Console.WriteLine($"Получено сообщений: {receivedMessages.Count}");
        Console.WriteLine($"Потеряно сообщений: {missingMessages.Count}");
        Console.WriteLine($"Неожиданных сообщений: {unexpectedMessages.Count}");

        if (missingMessages.Any())
        {
            Console.WriteLine("\nПотерянные сообщения:");
            foreach (var msg in missingMessages.Take(5))
            {
                Console.WriteLine(msg);
            }
        }

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    static async Task RunDisconnectReconnectTest()
    {
        Console.WriteLine("\n=== Тест отключения и переподключения ===\n");

        var client1 = new ChatsHandler(20, () => { }, "ws://localhost:8080/ws/");
        var client2 = new ChatsHandler(21, () => { }, "ws://localhost:8080/ws/");

        await client1.StartSetUp();
        await client2.StartSetUp();
        Console.WriteLine("Оба клиента подключены");

        // Отправляем сообщение до отключения
        await client1.SendMessage(21, "Сообщение до отключения");
        Console.WriteLine("Отправлено сообщение до отключения");

        // Отключаем и создаем новый экземпляр клиента
        await client1.DisposeAsync();
        client1 = new ChatsHandler(20, () => { }, "ws://localhost:8080/ws/");
        await client1.StartSetUp();
        Console.WriteLine("Клиент 1 переподключен");

        // Отправляем сообщение после переподключения
        await client1.SendMessage(21, "Сообщение после переподключения");
        Console.WriteLine("Отправлено сообщение после переподключения");

        await Task.Delay(1000);

        Console.WriteLine("\nСообщения клиента 2:");
        foreach (var (message, time) in client2.GetMessages(20))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    static async Task RunChatHistoryTest()
    {
        Console.WriteLine("\n=== Тест истории чата ===\n");

        var client1 = new ChatsHandler(22, () => { }, "ws://localhost:8080/ws/");
        var client2 = new ChatsHandler(23, () => { }, "ws://localhost:8080/ws/");

        await client1.StartSetUp();
        Console.WriteLine("Клиент 1 подключен");

        // Отправляем несколько сообщений когда второй клиент оффлайн
        for (int i = 1; i <= 3; i++)
        {
            await client1.SendMessage(23, $"Оффлайн сообщение {i}");
            await Task.Delay(100);
        }
        Console.WriteLine("Отправлены сообщения оффлайн клиенту");

        // Подключаем второго клиента
        await client2.StartSetUp();
        Console.WriteLine("Клиент 2 подключен");

        // Проверяем, получил ли второй клиент оффлайн сообщения
        Console.WriteLine("\nИстория сообщений клиента 2:");
        foreach (var (message, time) in client2.GetMessages(22))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        // Отправляем ответные сообщения
        for (int i = 1; i <= 2; i++)
        {
            await client2.SendMessage(22, $"Ответное сообщение {i}");
            await Task.Delay(100);
        }
        Console.WriteLine("\nОтправлены ответные сообщения");

        await Task.Delay(1000);

        // Проверяем полную историю у обоих клиентов
        Console.WriteLine("\nПолная история клиента 1:");
        foreach (var (message, time) in client1.GetMessages(23))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nПолная история клиента 2:");
        foreach (var (message, time) in client2.GetMessages(22))
        {
            Console.WriteLine($"[{time}] {message}");
        }

        Console.WriteLine("\nТест завершен");
        await client1.DisposeAsync();
        await client2.DisposeAsync();
    }

    // Commented out old tests
    /*
    private static async Task TestYourData()
    {
        // ...
    }

    private static async Task TestReconnection()
    {
        // ...
    }

    private static async Task TestMultipleMessages()
    {
        // ...
    }

    private static async Task TestMultipleClients()
    {
        // ...
    }

    private static async Task TestServerUnavailable()
    {
        // ...
    }

    private static async Task TestNewMessages()
    {
        // ...
    }
    */

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

    public static string FormatChatContents(Dictionary<int, List<string>> chats)
    {
        var sb = new StringBuilder();
        foreach (var chat in chats)
        {
            sb.AppendLine($"Чат с клиентом {chat.Key}:");
            foreach (var message in chat.Value)
            {
                sb.AppendLine($"  {message}");
            }
        }
        return sb.ToString();
    }
}