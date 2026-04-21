using System;
using System.Collections.Generic;
using System.Linq;
using TimeKnight.Extensions;
using TimeKnight.Utils;
using UnityEngine;
using Random = System.Random;

namespace TimeKnight.Core.LevelGeneration
{
    public class LevelNodeEdge
    {
        public readonly LevelNode First;
        public readonly LevelNode Second;

        public LevelNodeEdge(LevelNode first, LevelNode second)
        {
            First = first;
            Second = second;
        }
        
        public LevelNode Other(LevelNode node)
        {
            return First == node ? Second : First;
        }
    }
    
    public class LevelNode
    {
        public readonly string Name;
        public readonly RoomType RoomType;
        public readonly List<LevelNodeEdge> Edges = new();

        public LevelNode(string name, RoomType roomType)
        {
            Name = name;
            RoomType = roomType;
        }
        
        public static void Connect(LevelNode first, LevelNode second)
        {
            var edge = new LevelNodeEdge(first, second);
            first.Edges.Add(edge);
            second.Edges.Add(edge);
        }
    }

    public class GenerationHistory
    {
        public LevelNode? ExistingNode;
        public LevelNode GeneratedNode = null!;
        public RoomDefinition[]? RoomShuffle;
        public int GeneratedIndex;
    }
    
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private RoomRegistry rooms = null!;
        [SerializeField] private int seed;

        private Random _random = null!;
        
        private readonly Dictionary<LevelNode, RoomInstance> _roomMap = new();
        private readonly HashSet<Vector3Int> _occupiedTiles = new();

        private void OnValidate()
        {
            Validation.NotNull(this, rooms, nameof(rooms));
        }
        
        private void Awake()
        {
            if (rooms.AllRooms.IsEmpty())
            {
                Debug.LogWarning("No rooms found");
            }

            _random = new Random(seed);
            
            var startRoom = new LevelNode("start", RoomType.Start);
            var secondRoom = new LevelNode("2", RoomType.Enemy);
            var thirdRoom = new LevelNode("3", RoomType.Enemy);
            var room4 = new LevelNode("4", RoomType.Enemy);
            var room5 = new LevelNode("5", RoomType.Enemy);
            
            LevelNode.Connect(startRoom, secondRoom);
            LevelNode.Connect(secondRoom, thirdRoom);
            LevelNode.Connect(thirdRoom, room4);
            LevelNode.Connect(startRoom, room5);
            
            var success = GenerateGraph(startRoom);
            if (!success)
            {
                Debug.LogError("Unable to find a valid configuration for the level generation");
                return;
            }

            foreach (var (node, instance) in _roomMap)
            {
                instance.Instantiate(node.Name);
            }
        }

        private bool GenerateGraph(LevelNode start)
        {
            var edgesToConnect = new Stack<GenerationHistory>();
            var history = new Stack<GenerationHistory>();

            // Generate the starting room
            var startHistory = GenerateConnectedRoom(null, start);
            if (startHistory == null) return false;
            
            AddEdges(start, edgesToConnect);
            history.Push(startHistory);
            
            // Generate connections
            while (!edgesToConnect.IsEmpty())
            {
                var edge = edgesToConnect.Pop();
                    
                // Attempt room generation
                var generationHistory = GenerateConnectedRoom(edge.ExistingNode, edge.GeneratedNode, edge.RoomShuffle, edge.GeneratedIndex);
                
                // If generation fails undo
                if (generationHistory is null)
                {
                    // Get last placed history
                    if (history.IsEmpty()) return false;
                    var lastHistory = history.Pop();
                    
                    // Pop all things from stack connected to the room we are about to undo
                    while (!edgesToConnect.IsEmpty())
                    {
                        var edgeToRemove = edgesToConnect.Peek();
                        if (edgeToRemove.ExistingNode != lastHistory.GeneratedNode && 
                            edgeToRemove.GeneratedNode != lastHistory.GeneratedNode) break;
                        edgesToConnect.Pop();
                    }
                    
                    // Undo the generation of the last room
                    var roomInstanceToRemove = _roomMap[lastHistory.GeneratedNode];
                    roomInstanceToRemove.RemoveConnections();
                    UnregisterRoom(roomInstanceToRemove);

                    // Place the history back on the edgesToConnect stack
                    edgesToConnect.Push(lastHistory);
                }
                else
                {
                    AddEdges(edge.GeneratedNode, edgesToConnect);
                    history.Push(generationHistory);
                }
            }

            return true;
        }

        private bool IsEdgeCreated(LevelNodeEdge edge)
        {
            return _roomMap.ContainsKey(edge.First) && _roomMap.ContainsKey(edge.Second);
        }

        private void AddEdges(LevelNode existingNode, Stack<GenerationHistory> edgesToConnect)
        {
            foreach (var edge in existingNode.Edges.Where(edge => !IsEdgeCreated(edge)))
            {
                var otherNode = edge.Other(existingNode);
                edgesToConnect.Push(new GenerationHistory
                {
                    ExistingNode = existingNode,
                    GeneratedNode = otherNode,
                });
            }
        }

        private void RegisterRoom(RoomInstance room)
        {
            var positions = room.GetTileWorldPositions();
            foreach (var pos in 
                     from pos in positions 
                     where !_occupiedTiles.Add(pos)
                     select pos)
            {
                throw new ArgumentException("Attempting to register overlapping room. " +
                                            $"{pos} is already in the occupied tiles.");
            }
        }

        private void UnregisterRoom(RoomInstance room)
        {
            var positions = room.GetTileWorldPositions();
            positions.ForEach(pos => _occupiedTiles.Remove(pos));
        }
        
        private GenerationHistory? GenerateConnectedRoom(
            LevelNode? existingNode,
            LevelNode otherNode,
            RoomDefinition[]? possibleRooms = null,
            int possibleRoomsIndex = 0)
        {
            // Start from a new shuffle
            if (possibleRooms == null)
            {
                if (rooms.RoomsOfType[otherNode.RoomType].IsEmpty())
                {
                    throw new InvalidOperationException($"Rooms of type {otherNode.RoomType} is empty");
                }
                possibleRooms = rooms.RoomsOfType[otherNode.RoomType].ToArray();
                possibleRooms.ShuffleInPlace(_random);
            }
            // Resume the current shuffle and try spawning the next room
            else
            {
                possibleRoomsIndex++;
            }

            for (var i = possibleRoomsIndex; i < possibleRooms.Length; i++)
            {
                // If the room doesn't support the amount of connections that we need
                if (possibleRooms[i].ConnectionList.Count < otherNode.Edges.Count) continue;
                
                // Generate room
                var roomGen = 
                    existingNode is not null ?
                        _roomMap[existingNode].CreateConnection(possibleRooms[i], _occupiedTiles, _random) :
                        RoomInstance.FromStart(possibleRooms[i]);
                if (roomGen == null) continue;
                
                _roomMap[otherNode] = roomGen;
                RegisterRoom(_roomMap[otherNode]);

                return new GenerationHistory
                {
                    ExistingNode   = existingNode,
                    GeneratedNode  = otherNode,
                    RoomShuffle    = possibleRooms,
                    GeneratedIndex = i
                };
            }

            return null;
        }
    }
}
