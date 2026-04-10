using System.Collections.Generic;
using System.Linq;
using TimeKnight.Extensions;
using UnityEngine;

namespace TimeKnight.Core.LevelGeneration
{
    public class LevelNodeEdge
    {
        public LevelNode First;
        public LevelNode Second;
    }
    
    public class LevelNode
    {
        public readonly List<LevelNodeEdge> Edges = new();

        public static void Connect(LevelNode first, LevelNode second)
        {
            var edge = new LevelNodeEdge
            {
                First = first,
                Second = second,
            };
            first.Edges.Add(edge);
            second.Edges.Add(edge);
        }
    }
    
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private List<RoomDefinition> rooms = new();

        private readonly Dictionary<LevelNode, RoomInstance> _roomMap = new();
        private readonly HashSet<Vector3Int> _occupiedTiles = new(); 
        
        private void Awake()
        {
            if (rooms.IsEmpty())
            {
                Debug.LogWarning("No rooms found");
            }

            
            var startRoom = new LevelNode();
            var secondRoom = new LevelNode();
            var thirdRoom = new LevelNode();
            
            LevelNode.Connect(startRoom, secondRoom);
            LevelNode.Connect(secondRoom, thirdRoom);
            
            StartGraphGeneration(startRoom);
        }

        private void StartGraphGeneration(LevelNode start)
        {
            // Generate the starting room
            // _roomMap[start] = RoomInstance.FromStart(rooms.GetRandomElement());
            _roomMap[start] = RoomInstance.FromStart(rooms[0]);
            RegisterRoom(_roomMap[start]);

            var nodesToConnect = new Queue<LevelNode>();
            nodesToConnect.Enqueue(start);
            
            // Generate connections
            while (!nodesToConnect.IsEmpty())
            {
                var node = nodesToConnect.Dequeue();
                
                // TODO: Handle more edges than we have rooms
                foreach (var edge in node.Edges.Where(edge => !IsEdgeCreated(edge)))
                {
                    // Get the other node that hasn't been created
                    var otherNode = edge.First == node ? edge.Second : edge.First;
                
                    // Create the room
                    _roomMap[otherNode] = _roomMap[node].CreateConnection(rooms.GetRandomElement(), _occupiedTiles);
                    // _roomMap[otherNode] = _roomMap[node].CreateConnection(rooms[2], _occupiedTiles);

                    if (_roomMap[otherNode] == null)
                    {
                        Debug.LogWarning("Stopped generating graph, encountered impossible to place room");
                        return;
                    }
                    RegisterRoom(_roomMap[otherNode]);
                    nodesToConnect.Enqueue(otherNode);
                }
            }
        }

        private bool IsEdgeCreated(LevelNodeEdge edge)
        {
            return _roomMap.ContainsKey(edge.First) && _roomMap.ContainsKey(edge.Second);
        }

        private void RegisterRoom(RoomInstance room)
        {
            var positions = room.Definition.GetTileWorldPositions();
            foreach (var pos in positions)
            {
                var added = _occupiedTiles.Add(pos);
                if (!added) Debug.LogError(pos + " is already in the occupied tiles");
            }
        }
    }
}
