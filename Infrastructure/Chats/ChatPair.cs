using System;

namespace Infrastructure.Chats;

public readonly struct ChatPair : IEquatable<ChatPair>
{
    public readonly int Id1;
    public readonly int Id2;

    public ChatPair(int id1, int id2)
    {
        if (id1 <= id2)
        {
            Id1 = id1;
            Id2 = id2;
        }
        else
        {
            Id1 = id2;
            Id2 = id1;
        }
    }

    public bool Contains(int id) => Id1 == id || Id2 == id;

    public override bool Equals(object obj) =>
        obj is ChatPair other && Equals(other);

    public bool Equals(ChatPair other) =>
        Id1 == other.Id1 && Id2 == other.Id2;

    public override int GetHashCode() =>
        HashCode.Combine(Id1, Id2);

    public static bool operator ==(ChatPair left, ChatPair right) =>
        left.Equals(right);

    public static bool operator !=(ChatPair left, ChatPair right) =>
        !(left == right);
}