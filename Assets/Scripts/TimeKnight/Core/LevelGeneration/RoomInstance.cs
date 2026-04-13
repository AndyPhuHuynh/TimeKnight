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
        public RoomDefinition Definition;
        private ConnectionInstance[] _connections;

        private Vector3 GetConnectionPosition(ConnectionDefinition connection)
        {
            return Definition.gameObject.transform.position + connection.centerOffset;
        }
        
        public static RoomInstance FromStart(RoomDefinition definition)
        {
            var def = Object.Instantiate(definition);
            var connections = def.ConnectionList.Select(connectionDef => new ConnectionInstance 
            {
                Definition = connectionDef,
                ConnectedRoom = null
            }).ToArray();
            
            return new RoomInstance
            {
                Definition = def,
                _connections = connections
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
                    
                    // Instantiate the room definition
                    var newRoomObject = Object.Instantiate(other, newCenterPos, Quaternion.identity);
                    
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
                        Definition = newRoomObject,
                        _connections = newConnections
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
    }
}