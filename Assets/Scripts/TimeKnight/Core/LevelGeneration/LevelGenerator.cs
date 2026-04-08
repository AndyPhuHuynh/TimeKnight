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
        public List<LevelNodeEdge> Edges = new();

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
        
        private void Awake()
        {
            if (rooms.IsEmpty())
            {
                Debug.LogWarning("No rooms found");
            }

            var secondRoom = new LevelNode();
            var startRoom = new LevelNode();
            
            LevelNode.Connect(startRoom, secondRoom);
            
            StartGraphGeneration(startRoom);
        }

        private void StartGraphGeneration(LevelNode start)
        {
            // Generate the starting room
            _roomMap[start] = RoomInstance.FromStart(rooms[0]);
            
            // Generate connections
            foreach (var edge in start.Edges.Where(edge => !IsEdgeCreated(edge)))
            {
                // Get the other node that hasn't been created
                var otherNode = edge.First == start ? edge.Second : edge.First;
                
                // Create the room
                _roomMap[otherNode] = _roomMap[start].CreateConnection(rooms[0]);
            }
        }

        private bool IsEdgeCreated(LevelNodeEdge edge)
        {
            return _roomMap.ContainsKey(edge.First) && _roomMap.ContainsKey(edge.Second);
        }
    }
}
