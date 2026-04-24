using System.Collections.Generic;
using System.Linq;
using TimeKnight.Extensions;
using TimeKnight.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace TimeKnight.Core.LevelGeneration
{
    public class ConnectionNode
    {
        public ConnectionDefinition Definition;
        public RoomNode? ConnectedRoom;
        
        public bool IsConnected => ConnectedRoom != null;
    }
    
    // Represents the placement of a room and it's connections before it is instantiated
    public class RoomNode
    {
        private RoomDefinition _definition = null!;
        private ConnectionNode[] _connections = null!;
        public Vector3 WorldPos { get; private set; }

        private RoomNode() {}

        public Dictionary<Vector3Int, TileBase> GetTileWorldPositions()
        {
            var localPositions = _definition.GetTileLocalPositions();
            var flooredWorldPos = WorldPos.FloorToInt();
            return localPositions.ToDictionary(
                kvp => kvp.Key + flooredWorldPos,
                kvp => kvp.Value);
        }

        public IRoomBehavior[] GetAllBehaviors()
        {
            return _definition.GetComponentsInChildren<IRoomBehavior>();
        }

        public static RoomNode FromStart(RoomDefinition definition)
        {
            var connections = definition.ConnectionList.Select(connectionDef => new ConnectionNode 
            {
                Definition = connectionDef,
                ConnectedRoom = null
            }).ToArray();
            
            return new RoomNode
            {
                _definition = definition,
                _connections = connections,
                WorldPos = Vector3.zero
            };
        }

        // Checks if a valid connection exists out of this room into the other room with the connection type
        private RoomNode? PlaceConnection(RoomDefinition other, ConnectionType type, HashSet<Vector3Int> occupiedTiles, Random random)
        {
            // Check if this room even has that connection type available.
            var thisConnections = _connections.Where(c => c.Definition.type == type && !c.IsConnected).ToList();
            if (thisConnections.IsEmpty()) return null;
            
            // Check if the other room has the matching connection type
            var otherConnections = other.ConnectionList.Where(c => c.type == type.Opposite()).ToList();
            if (otherConnections.IsEmpty()) return null;

            // Shuffle the rooms
            thisConnections.ShuffleInPlace(random);
            otherConnections.ShuffleInPlace(random);
            
            // Iterate through every pair of connections
            var localTilePositions = other.GetTileLocalPositions();
            foreach (var thisConnection in thisConnections)
            {
                foreach (var otherConnection in otherConnections)
                {
                    var newCenterPos = WorldPos + 
                                       thisConnection.Definition.centerOffset +
                                       thisConnection.Definition.type.GetUnitVector() - 
                                       otherConnection.centerOffset;
                    var newCenterPosInt = newCenterPos.FloorToInt();
                    
                    // Check for tilemap collisions
                    var collisionFound = localTilePositions.Keys
                        .Select(localPos => newCenterPosInt + localPos)
                        .Where(occupiedTiles.Contains)
                        .Any();
                    if (collisionFound) continue;
                    
                    // Initialize the connections
                    var newConnections = new ConnectionNode[other.ConnectionList.Count];
                    for (var i = 0; i < other.ConnectionList.Count; i++)
                    {
                        var connection = other.ConnectionList[i];
                        newConnections[i] = new ConnectionNode
                        {
                            Definition = connection,
                            ConnectedRoom = connection == otherConnection ? this : null
                        };
                    }
                    
                    // Initialize the new room instance
                    var newInstance = new RoomNode
                    {
                        _definition = other,
                        _connections = newConnections,
                        WorldPos = newCenterPos,
                    };
                    
                    // Set the connection on this instance
                    thisConnection.ConnectedRoom = newInstance;

                    return newInstance;
                }
            }
            
            return null;
        }
        
        public RoomNode? CreateConnection(RoomDefinition other, HashSet<Vector3Int> occupiedTiles, Random random)
        {
            // Find matching connection between this room and the other new room
            var directions = EnumUtils.GetEnumValues<ConnectionType>();
            directions.ShuffleInPlace(random);

            return directions
                .Select(dir => PlaceConnection(other, dir, occupiedTiles, random))
                .FirstOrDefault(newRoom => newRoom != null);
        }
        
        public void RemoveConnections()
        {
            foreach (var connection in _connections.Where(c => c.IsConnected))
            {
                var matchingConnections = connection.ConnectedRoom!._connections.Where(c => c.ConnectedRoom == this);
                foreach (var matchingConnection in matchingConnections)
                {
                    matchingConnection.ConnectedRoom = null;
                }
                connection.ConnectedRoom = null;
            }
        }
    }
}