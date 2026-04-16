using System;
using UnityEngine;

namespace TimeKnight.Core.LevelGeneration
{
    public enum ConnectionType
    {
        Left,
        Right,
        Up,
        Down
    }
    
    [Serializable]
    public struct ConnectionDefinition : IEquatable<ConnectionDefinition>
    {
        public ConnectionType type;
        public Vector3 centerOffset;

        public static bool operator==(ConnectionDefinition a, ConnectionDefinition b)
        {
            return a.type == b.type  && a.centerOffset == b.centerOffset;
        }

        public static bool operator !=(ConnectionDefinition a, ConnectionDefinition b)
        {
            return !(a == b);
        }

        public bool Equals(ConnectionDefinition other)
        {
            return type == other.type && centerOffset.Equals(other.centerOffset);
        }

        public override bool Equals(object obj)
        {
            return obj is ConnectionDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)type, centerOffset);
        }
    }

    public static class ConnectionTypeExtensions
    {
        public static ConnectionType Opposite(this ConnectionType connectionType)
        {
            return connectionType switch
            {
                ConnectionType.Left  => ConnectionType.Right,
                ConnectionType.Right => ConnectionType.Left,
                ConnectionType.Up    => ConnectionType.Down,
                ConnectionType.Down  => ConnectionType.Up,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null)
            };
        }
        
        public static Vector3 GetUnitVector(this ConnectionType connectionType)
        {
            return connectionType switch
            {
                ConnectionType.Left  => Vector3.left,
                ConnectionType.Right => Vector3.right,
                ConnectionType.Up    => Vector3.up,
                ConnectionType.Down  => Vector3.down,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null)
            };
        }
    }
}