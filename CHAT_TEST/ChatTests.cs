using NUnit.Framework;
using Infrastructure.Chats;
using System.Collections.Concurrent;

namespace CHAT_TEST;

[TestFixture]
public class ChatTests
{
    private Chats _client1;
    private Chats _client2;
    private Chats _client3;
    private readonly string _serverUrl = "ws://localhost:8080/ws/";
    private readonly ConcurrentBag<string> _receivedMessages = new();

    [SetUp]
    public async Task Setup()
    {
        _client1 = new Chats(1, _serverUrl);
        _client2 = new Chats(2, _serverUrl);
        _client3 = new Chats(3, _serverUrl);
        _receivedMessages.Clear();

        // Подписываемся на события новых сообщений
        _client1.NewMessagesReceived += (s, users) => _receivedMessages.Add($"Client1: New messages from {string.Join(",", users)}");
        _client2.NewMessagesReceived += (s, users) => _receivedMessages.Add($"Client2: New messages from {string.Join(",", users)}");
        _client3.NewMessagesReceived += (s, users) => _receivedMessages.Add($"Client3: New messages from {string.Join(",", users)}");

        await _client1.RunAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {
        try
        {
            await Task.Delay(1000);
            await Task.WhenAll(
                _client1?.DisposeAsync().AsTask() ?? Task.CompletedTask,
                _client2?.DisposeAsync().AsTask() ?? Task.CompletedTask,
                _client3?.DisposeAsync().AsTask() ?? Task.CompletedTask
            );
            await Task.Delay(1000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during cleanup: {ex}");
            throw;
        }
    }

    [Test]
    public async Task BasicMessageDelivery_OnlineUsers_Success()
    {
        // Arrange
        await _client2.RunAsync();
        await Task.Delay(500);

        // Act
        var messageText = "Test message";
        var sent = await _client1.SendMessageAsync(2, messageText);
        await Task.Delay(500);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(sent, Is.True, "Message should be sent successfully");
            var messages = _client2.GetMessages(1);
            Assert.That(messages, Has.Count.EqualTo(1), "Should receive exactly one message");
            Assert.That(messages[0].Item1, Is.EqualTo(messageText), "Message content should match");
            Assert.That(_receivedMessages, Contains.Item("Client2: New messages from 1"), "Should trigger message received event");
        });
    }

    [Test]
    public async Task MessageDelivery_OfflineUser_MessageStoredAndDelivered()
    {
        // Act
        var messageText = "Offline message";
        var sent = await _client1.SendMessageAsync(2, messageText);
        await Task.Delay(500);

        // Connect client2 and check messages
        await _client2.RunAsync();
        await Task.Delay(500);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(sent, Is.True, "Message should be sent successfully");
            var messages = _client2.GetMessages(1);
            Assert.That(messages, Has.Count.EqualTo(1), "Should receive exactly one message");
            Assert.That(messages[0].Item1, Is.EqualTo(messageText), "Message content should match");
            Assert.That(_receivedMessages, Contains.Item("Client2: New messages from 1"), "Should trigger message received event");
        });
    }

    [Test]
    public async Task MessageOrder_MultipleMessages_PreservesOrder()
    {
        // Arrange
        await _client2.RunAsync();
        await Task.Delay(500);

        var messages = new[] { "First", "Second", "Third" };
        
        // Act
        foreach (var msg in messages)
        {
            await _client1.SendMessageAsync(2, msg);
            await Task.Delay(100);
        }
        await Task.Delay(500);

        // Assert
        var receivedMessages = _client2.GetMessages(1);
        Assert.Multiple(() =>
        {
            Assert.That(receivedMessages, Has.Count.EqualTo(messages.Length), "Should receive all messages");
            for (int i = 0; i < messages.Length; i++)
            {
                Assert.That(receivedMessages[i].Item1, Is.EqualTo(messages[i]), $"Message {i + 1} should match");
            }
        });
    }

    [Test]
    public async Task MessageRouting_MultipleRecipients_CorrectDelivery()
    {
        // Arrange
        await _client2.RunAsync();
        await _client3.RunAsync();
        await Task.Delay(500);

        // Act
        await _client1.SendMessageAsync(2, "Message for client 2");
        await _client1.SendMessageAsync(3, "Message for client 3");
        await Task.Delay(500);

        // Assert
        Assert.Multiple(() =>
        {
            var messages2 = _client2.GetMessages(1);
            var messages3 = _client3.GetMessages(1);

            Assert.That(messages2, Has.Count.EqualTo(1), "Client 2 should receive one message");
            Assert.That(messages3, Has.Count.EqualTo(1), "Client 3 should receive one message");
            Assert.That(messages2[0].Item1, Is.EqualTo("Message for client 2"), "Client 2 message should match");
            Assert.That(messages3[0].Item1, Is.EqualTo("Message for client 3"), "Client 3 message should match");
        });
    }

    [Test]
    public async Task NewMessageNotification_MultipleMessages_CorrectEventCounts()
    {
        // Arrange
        await _client2.RunAsync();
        await Task.Delay(500);
        _receivedMessages.Clear();

        // Act
        await _client1.SendMessageAsync(2, "Message 1");
        await Task.Delay(100);
        await _client1.SendMessageAsync(2, "Message 2");
        await Task.Delay(500);

        // Assert
        var notifications = _receivedMessages.Count(m => m.StartsWith("Client2:"));
        Assert.That(notifications, Is.EqualTo(2), "Should receive two notification events");
    }

    [Test]
    public async Task DisconnectReconnect_MessageDelivery_Success()
    {
        // Arrange
        await _client2.RunAsync();
        await Task.Delay(500);

        // Send message before disconnect
        await _client1.SendMessageAsync(2, "Before disconnect");
        await Task.Delay(500);

        // Disconnect and send message
        await _client2.DisposeAsync();
        await Task.Delay(500);

        // Store messages from old client
        var oldMessages = _client2.GetMessages(1);

        await _client1.SendMessageAsync(2, "During disconnect");
        await Task.Delay(500);

        // Reconnect and restore messages
        _client2 = new Chats(2, _serverUrl);
        foreach (var msg in oldMessages)
        {
            await _client2.RestoreMessage(1, msg.Item1, msg.Item2);
        }
        _client2.NewMessagesReceived += (s, users) => _receivedMessages.Add($"Client2: New messages from {string.Join(",", users)}");
        await _client2.RunAsync();
        await Task.Delay(500);

        // Assert
        var messages = _client2.GetMessages(1);
        Assert.Multiple(() =>
        {
            Assert.That(messages, Has.Count.EqualTo(2), "Should receive both messages");
            Assert.That(messages[0].Item1, Is.EqualTo("Before disconnect"), "First message should match");
            Assert.That(messages[1].Item1, Is.EqualTo("During disconnect"), "Second message should match");
        });
    }

    [Test]
    public async Task MessageHistory_PersistsAcrossConnections()
    {
        // Arrange
        await _client2.RunAsync();
        await Task.Delay(500);

        // Send messages and disconnect
        await _client1.SendMessageAsync(2, "History message 1");
        await _client1.SendMessageAsync(2, "History message 2");
        await Task.Delay(500);

        // Store messages before disconnect
        var oldMessages = _client2.GetMessages(1);
        await _client2.DisposeAsync();
        await Task.Delay(500);

        // Reconnect with new instance and restore messages
        _client2 = new Chats(2, _serverUrl);
        foreach (var msg in oldMessages)
        {
            await _client2.RestoreMessage(1, msg.Item1, msg.Item2);
        }
        await _client2.RunAsync();
        await Task.Delay(500);

        // Assert
        var messages = _client2.GetMessages(1);
        Assert.Multiple(() =>
        {
            Assert.That(messages, Has.Count.EqualTo(2), "Should retain message history");
            Assert.That(messages[0].Item1, Is.EqualTo("History message 1"), "First history message should match");
            Assert.That(messages[1].Item1, Is.EqualTo("History message 2"), "Second history message should match");
        });
    }

    [Test]
    public async Task ConcurrentMessages_MultipleClients_AllDelivered()
    {
        // Arrange
        await _client2.RunAsync();
        await _client3.RunAsync();
        await Task.Delay(500);

        const int messageCount = 10;
        var tasks = new List<Task>();

        // Act - Send messages concurrently
        for (int i = 0; i < messageCount; i++)
        {
            tasks.Add(_client1.SendMessageAsync(2, $"To2_{i}"));
            tasks.Add(_client1.SendMessageAsync(3, $"To3_{i}"));
        }
        await Task.WhenAll(tasks);
        await Task.Delay(1000);

        // Assert
        Assert.Multiple(() =>
        {
            var messages2 = _client2.GetMessages(1);
            var messages3 = _client3.GetMessages(1);

            Assert.That(messages2, Has.Count.EqualTo(messageCount), "Client 2 should receive all messages");
            Assert.That(messages3, Has.Count.EqualTo(messageCount), "Client 3 should receive all messages");

            for (int i = 0; i < messageCount; i++)
            {
                Assert.That(messages2.Any(m => m.Item1 == $"To2_{i}"), $"Message To2_{i} should be delivered");
                Assert.That(messages3.Any(m => m.Item1 == $"To3_{i}"), $"Message To3_{i} should be delivered");
            }
        });
    }

    [Test]
    public async Task LongMessage_Delivery_Success()
    {
        // Arrange
        await _client2.RunAsync();
        await Task.Delay(500);

        // Create a long message
        var longMessage = new string('A', 1000) + new string('B', 1000) + new string('C', 1000);

        // Act
        var sent = await _client1.SendMessageAsync(2, longMessage);
        await Task.Delay(500);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(sent, Is.True, "Long message should be sent successfully");
            var messages = _client2.GetMessages(1);
            Assert.That(messages, Has.Count.EqualTo(1), "Should receive exactly one message");
            Assert.That(messages[0].Item1, Is.EqualTo(longMessage), "Long message content should match exactly");
            Assert.That(messages[0].Item1.Length, Is.EqualTo(3000), "Message length should be preserved");
        });
    }
}