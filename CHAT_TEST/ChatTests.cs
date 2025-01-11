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
    private readonly string _serverUrl = "ws://localhost:8081/ws/";
    private readonly ConcurrentBag<string> _receivedMessages = new();

    [SetUp]
    public async Task Setup()
    {
        _client1 = new Chats(1, _serverUrl);
        _client2 = new Chats(2, _serverUrl);
        _client3 = new Chats(3, _serverUrl);
        _receivedMessages.Clear();

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
            await Task.Delay(400);
            await Task.WhenAll(
                _client1?.DisposeAsync().AsTask() ?? Task.CompletedTask,
                _client2?.DisposeAsync().AsTask() ?? Task.CompletedTask,
                _client3?.DisposeAsync().AsTask() ?? Task.CompletedTask
            );
            await Task.Delay(400);
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
        await _client2.RunAsync();
        await Task.Delay(500);

        var messageText = "Test message";
        var sent = await _client1.SendMessageAsync(2, messageText);
        await Task.Delay(500);

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
        var messageText = "Offline message";
        var sent = await _client1.SendMessageAsync(2, messageText);
        await Task.Delay(500);

        await _client2.RunAsync();
        await Task.Delay(500);

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
        await _client2.RunAsync();
        await Task.Delay(500);

        var messages = new[] { "First", "Second", "Third" };
    
        foreach (var msg in messages)
        {
            await _client1.SendMessageAsync(2, msg);
            await Task.Delay(100);
        }
        await Task.Delay(500);

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
        await _client2.RunAsync();
        await _client3.RunAsync();
        await Task.Delay(500);

        await _client1.SendMessageAsync(2, "Message for client 2");
        await _client1.SendMessageAsync(3, "Message for client 3");
        await Task.Delay(500);

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
        await _client2.RunAsync();
        await Task.Delay(500);
        _receivedMessages.Clear();

        await _client1.SendMessageAsync(2, "Message 1");
        await Task.Delay(100);
        await _client1.SendMessageAsync(2, "Message 2");
        await Task.Delay(500);

        var notifications = _receivedMessages.Count(m => m.StartsWith("Client2:"));
        Assert.That(notifications, Is.EqualTo(2), "Should receive two notification events");
    }

    [Test]
    public async Task DisconnectReconnect_MessageDelivery_Success()
    {
        await _client2.RunAsync();
        await Task.Delay(500);

        await _client1.SendMessageAsync(2, "Before disconnect");
        await Task.Delay(500);

        await _client2.DisposeAsync();
        await Task.Delay(500);

        var oldMessages = _client2.GetMessages(1);

        await _client1.SendMessageAsync(2, "During disconnect");
        await Task.Delay(500);

        _client2 = new Chats(2, _serverUrl);
        foreach (var msg in oldMessages)
        {
            await _client2.RestoreMessage(1, msg.Item1, msg.Item2);
        }
        _client2.NewMessagesReceived += (s, users) => _receivedMessages.Add($"Client2: New messages from {string.Join(",", users)}");
        await _client2.RunAsync();
        await Task.Delay(500);
        
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
        await _client2.RunAsync();
        await Task.Delay(500);

        await _client1.SendMessageAsync(2, "History message 1");
        await _client1.SendMessageAsync(2, "History message 2");
        await Task.Delay(500);

        var oldMessages = _client2.GetMessages(1);
        await _client2.DisposeAsync();
        await Task.Delay(500);

        _client2 = new Chats(2, _serverUrl);
        foreach (var msg in oldMessages)
        {
            await _client2.RestoreMessage(1, msg.Item1, msg.Item2);
        }
        await _client2.RunAsync();
        await Task.Delay(500);

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
        await _client2.RunAsync();
        await _client3.RunAsync();
        await Task.Delay(50);

        const int messageCount = 10;
        var tasks = new List<Task>();

        for (int i = 0; i < messageCount; i++)
        {
            tasks.Add(_client1.SendMessageAsync(2, $"To2_{i}"));
            tasks.Add(_client1.SendMessageAsync(3, $"To3_{i}"));
        }
        await Task.WhenAll(tasks);
        await Task.Delay(50);

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
        await _client2.RunAsync();
        await Task.Delay(500);

        var longMessage = new string('A', 1000) + new string('B', 1000) + new string('C', 1000);

        var sent = await _client1.SendMessageAsync(2, longMessage);
        await Task.Delay(500);

        Assert.Multiple(() =>
        {
            Assert.That(sent, Is.True, "Long message should be sent successfully");
            var messages = _client2.GetMessages(1);
            Assert.That(messages, Has.Count.EqualTo(1), "Should receive exactly one message");
            Assert.That(messages[0].Item1, Is.EqualTo(longMessage), "Long message content should match exactly");
            Assert.That(messages[0].Item1.Length, Is.EqualTo(3000), "Message length should be preserved");
        });
    }

    [Test]
    public async Task UnreadMessages_CountAndMarkAsRead_Success()
    {
        await _client2.RunAsync();
        await Task.Delay(500);

        await _client1.SendMessageAsync(2, "Message 1");
        await _client1.SendMessageAsync(2, "Message 2");
        await _client1.SendMessageAsync(2, "Message 3");
        await Task.Delay(500);

        Assert.Multiple(() =>
        {
            var unreadCount = _client2.GetUnreadCount(1);
            Assert.That(unreadCount, Is.EqualTo(3), "Should have 3 unread messages");

            _client2.MarkAsRead(1);
            unreadCount = _client2.GetUnreadCount(1);
            Assert.That(unreadCount, Is.EqualTo(0), "Should have 0 unread messages after marking as read");
            
            var messages = _client2.GetMessages(1);
            Assert.That(messages, Has.Count.EqualTo(3), "Should still have all messages after marking as read");
        });
    }

    [Test]
    public async Task EditMessage_Success()
    {
        await _client2.RunAsync();
        await Task.Delay(500);

        await _client1.SendMessageAsync(2, "Original message");
        await Task.Delay(500);

        var messages = _client2.GetMessages(1);
        var messageTime = messages[0].Item2;
        
        var edited = await _client1.EditMessage(2, messageTime, "Edited message");
        await Task.Delay(500);

        messages = _client2.GetMessages(1);
        Assert.Multiple(() =>
        {
            Assert.That(edited, Is.True, "Edit operation should succeed");
            Assert.That(messages, Has.Count.EqualTo(1), "Should still have one message");
            Assert.That(messages[0].Item1, Is.EqualTo("Edited message"), "Message should be updated");
            Assert.That(messages[0].Item2, Is.EqualTo(messageTime), "Message time should remain unchanged");
        });
    }

    [Test]
    public async Task DeleteMessage_Success()
    {
        await _client2.RunAsync();
        await Task.Delay(500);

        await _client1.SendMessageAsync(2, "Message to delete");
        await _client1.SendMessageAsync(2, "Message to keep");
        await Task.Delay(500);

        var messages = _client2.GetMessages(1);
        var messageToDeleteTime = messages[0].Item2;
        
        var deleted = await _client1.DeleteMessage(2, messageToDeleteTime);
        await Task.Delay(500);

        messages = _client2.GetMessages(1);
        Assert.Multiple(() =>
        {
            Assert.That(deleted, Is.True, "Delete operation should succeed");
            Assert.That(messages, Has.Count.EqualTo(1), "Should have one message left");
            Assert.That(messages[0].Item1, Is.EqualTo("Message to keep"), "Correct message should remain");
        });
    }

    [Test]
    public async Task ReconnectAsync_PreservesMessages_Success()
    {
        await _client2.RunAsync();
        await Task.Delay(500);

        await _client1.SendMessageAsync(2, "Message before reconnect");
        await Task.Delay(500);

        await _client2.ReconnectAsync();
        await Task.Delay(500);

        await _client1.SendMessageAsync(2, "Message after reconnect");
        await Task.Delay(500);

        var messages = _client2.GetMessages(1);
        Assert.Multiple(() =>
        {
            Assert.That(_client2.IsConnected, Is.True, "Should be connected after reconnect");
            Assert.That(messages, Has.Count.EqualTo(2), "Should have messages from before and after reconnect");
            Assert.That(messages[0].Item1, Is.EqualTo("Message before reconnect"), "First message should be preserved");
            Assert.That(messages[1].Item1, Is.EqualTo("Message after reconnect"), "Should receive new messages after reconnect");
        });
    }

    public async ValueTask DisposeAsync()
    {
        // ... существующий код ... ХУЙНЯ
    }
}