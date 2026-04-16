using System;
using System.Collections.Generic;
using System.Linq;
using TimeKnight.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace TimeKnight.Core.LevelGeneration
{
    public class ConnectionInstance
    {
        public ConnectionDefinition Definition;
        public RoomInstance ConnectedRoom;
        
        public bool IsConnected => ConnectedRoom != null;
    }
    
    public class RoomInstance
    {
        private RoomDefinition _definition;
        private ConnectionInstance[] _connections;
        private Vector3 _worldPos;

        private Vector3 GetConnectionPosition(ConnectionDefinition connection)
        {
            return _worldPos + connection.centerOffset;
        }

        public List<Vector3Int> GetTileWorldPositions()
        {
            var localPositions = _definition.GetTileLocalPositions();
            var flooredWorldPos = _worldPos.FloorToInt();
            return localPositions.Select(pos => flooredWorldPos + pos).ToList();
        }
        
        public static RoomInstance FromStart(RoomDefinition definition)
        {
            var connections = definition.ConnectionList.Select(connectionDef => new ConnectionInstance 
            {
                Definition = connectionDef,
                ConnectedRoom = null
            }).ToArray();
            
            return new RoomInstance
            {
                _definition = definition,
                _connections = connections,
                _worldPos = Vector3.zero
            };
        }

        // Checks if a valid connection exists out of this room into the other room with the connection type
        private RoomInstance PlaceConnection(RoomDefinition other, ConnectionType type, HashSet<Vector3Int> occupiedTiles, Random random)
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
                    var newCenterPos = GetConnectionPosition(thisConnection.Definition) + 
                                 thisConnection.Definition.type.GetUnitVector() - 
                                 otherConnection.centerOffset;
                    var newCenterPosInt = newCenterPos.FloorToInt();
                    
                    // Check for tilemap collisions
                    var collisionFound = localTilePositions
                        .Select(localPos => newCenterPosInt + localPos)
                        .Where(occupiedTiles.Contains)
                        .Any();
                    if (collisionFound) continue;
                    
                    // Initialize the connections
                    var newConnections = new ConnectionInstance[other.ConnectionList.Count];
                    for (var i = 0; i < other.ConnectionList.Count; i++)
                    {
                        var connection = other.ConnectionList[i];
                        newConnections[i] = new ConnectionInstance
                        {
                            Definition = connection,
                            ConnectedRoom = connection == otherConnection ? this : null
                        };
                    }
                    
                    // Initialize the new room instance
                    var newInstance = new RoomInstance
                    {
                        _definition = other,
                        _connections = newConnections,
                        _worldPos = newCenterPos,
                    };
                    
                    // Set the connection on this instance
                    thisConnection.ConnectedRoom = newInstance;

                    return newInstance;
                }
            }
            
            return null;
        }
        
        public RoomInstance CreateConnection(RoomDefinition other, HashSet<Vector3Int> occupiedTiles, Random random)
        {
            // Find matching connection between this room and the other new room
            var directions = Enum.GetValues(typeof(ConnectionType)) as ConnectionType[];
            directions.ShuffleInPlace(random);
            Debug.Assert(directions != null);

            return directions
                .Select(dir => PlaceConnection(other, dir, occupiedTiles, random))
                .FirstOrDefault(newRoom => newRoom != null);
        }

        public void Instantiate(string name)
        {
            _definition = Object.Instantiate(_definition, _worldPos, Quaternion.identity);
            _definition.gameObject.name = name;
        }
        
        public void RemoveConnections()
        {
            foreach (var connection in _connections.Where(c => c.IsConnected))
            {
                var matchingConnections = connection.ConnectedRoom._connections.Where(c => c.ConnectedRoom == this);
                foreach (var matchingConnection in matchingConnections)
                {
                    matchingConnection.ConnectedRoom = null;
                }
                connection.ConnectedRoom = null;
            }
        }
    }
}