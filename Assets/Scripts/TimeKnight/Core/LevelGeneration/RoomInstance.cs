using System;
using System.Linq;
using TimeKnight.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

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
        public ConnectionInstance[] Connections;

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
                Connections = connections
            };
        }

        // Checks if a valid connection exists out of this room into the other room with the connection type
        private RoomInstance PlaceConnection(RoomDefinition other, ConnectionType type)
        {
            // Check if this room even has that connection type available.
            var thisConnections = Connections.Where(c => c.Definition.type == type && !c.IsConnected).ToList();
            if (thisConnections.IsEmpty()) return null;
            
            // Check if the other room has the matching connection type
            var otherConnections = other.ConnectionList.Where(c => c.type == type.Opposite()).ToList();
            if (otherConnections.IsEmpty()) return null;

            // Shuffle the rooms
            thisConnections.ShuffleInPlace();
            otherConnections.ShuffleInPlace();
            
            // Iterate through every pair of connections
            // TODO: Check for collisions that make the tilemap generation impossible
            foreach (var thisConnection in thisConnections)
            {
                foreach (var otherConnection in otherConnections)
                {
                    var newPos = GetConnectionPosition(thisConnection.Definition) + 
                                 thisConnection.Definition.type.GetUnitVector() - 
                                 otherConnection.centerOffset;
                    
                    // Instantiate the room definition
                    var newRoomObject = Object.Instantiate(other, newPos, Quaternion.identity);
                    
                    // Initialize the connections
                    var newConnections = new ConnectionInstance[other.ConnectionList.Count];
                    for (var i = 0; i < other.ConnectionList.Count; i++)
                    {
                        var connection = other.ConnectionList[i];
                        if (connection == otherConnection)
                        {
                            newConnections[i] = new ConnectionInstance
                            {
                                Definition = connection,
                                ConnectedRoom = this
                            };
                        }
                        else
                        {
                            newConnections[i] = new ConnectionInstance
                            {
                                Definition = connection,
                                ConnectedRoom = null
                            };
                        }
                    }
                    
                    // Initialize the new room instance
                    var newInstance = new RoomInstance
                    {
                        Definition = newRoomObject,
                        Connections = newConnections
                    };
                    
                    // Set the connection on this instance
                    thisConnection.ConnectedRoom = newInstance;

                    return newInstance;
                }
            }
            
            return null;
        }
        
        public RoomInstance CreateConnection(RoomDefinition other)
        {
            // Find matching connection between this room and the other new room
            var directions = Enum.GetValues(typeof(ConnectionType)) as ConnectionType[];
            directions.ShuffleInPlace();
            Debug.Assert(directions != null);
            
            foreach (var dir in directions)
            {
                var newRoom = PlaceConnection(other, dir);
                if (newRoom != null) return newRoom;
            }
            
            Debug.LogWarning("Error, unable to create connection");
            return null;
        }
    }
}