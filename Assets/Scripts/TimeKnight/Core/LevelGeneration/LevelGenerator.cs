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

    public class GenerationStep
    {
        public LevelNode? ExistingNode;
        public LevelNode OtherNode = null!;
        public RoomDefinition[]? RoomShuffle;
        public int ShuffleIndex;

        public static GenerationStep FromStart(LevelNode start)
        {
            return new GenerationStep
            {
                OtherNode = start,
            };
        }

        public static GenerationStep FromEdge(LevelNode existing, LevelNode other)
        {
            return new GenerationStep
            {
                ExistingNode = existing,
                OtherNode = other
            };
        }
    }
    
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private RoomRegistry rooms = null!;
        [SerializeField] private int seed;

        private Random _random = null!;
        
        private readonly Dictionary<LevelNode, RoomNode> _roomNodeMap = new();
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
                throw new InvalidOperationException("Unable to find a valid configuration for the level generation");
            }
            
            foreach (var (レヴェルノド, ルームノド) in _roomNodeMap)
            {
                RoomSpawn.FromNode(ルームノド, レヴェルノド.Name); 
            }
        }

        private bool GenerateGraph(LevelNode start)
        {
            var stepsToGenerate = new Stack<GenerationStep>();
            var history = new Stack<GenerationStep>();

            // Generate the starting room
            var startStep = GenerationStep.FromStart(start);
            var startSuccess = GenerateConnectedRoom(startStep);
            if (!startSuccess) return false;
            
            AddEdges(start, stepsToGenerate);
            history.Push(startStep);
            
            // Generate connections
            while (!stepsToGenerate.IsEmpty())
            {
                var step = stepsToGenerate.Pop();
                    
                // Attempt room generation
                var generationSuccess = GenerateConnectedRoom(step);
                if (!generationSuccess)
                {
                    // Get last placed history
                    if (history.IsEmpty()) return false;
                    var lastHistory = history.Pop();
                    
                    // Pop all things from stack connected to the room we are about to undo
                    while (!stepsToGenerate.IsEmpty())
                    {
                        var edgeToRemove = stepsToGenerate.Peek();
                        if (edgeToRemove.ExistingNode != lastHistory.OtherNode && 
                            edgeToRemove.OtherNode != lastHistory.OtherNode) break;
                        stepsToGenerate.Pop();
                    }
                    
                    // Undo the generation of the last room
                    var roomInstanceToRemove = _roomNodeMap[lastHistory.OtherNode];
                    roomInstanceToRemove.RemoveConnections();
                    UnregisterRoom(roomInstanceToRemove);

                    // Place the history back on the edgesToConnect stack
                    stepsToGenerate.Push(lastHistory);
                }
                else
                {
                    AddEdges(step.OtherNode, stepsToGenerate);
                    history.Push(step);
                }
            }

            return true;
        }

        private bool IsEdgeCreated(LevelNodeEdge edge)
        {
            return _roomNodeMap.ContainsKey(edge.First) && _roomNodeMap.ContainsKey(edge.Second);
        }

        private void AddEdges(LevelNode existingNode, Stack<GenerationStep> edgesToConnect)
        {
            foreach (var edge in existingNode.Edges.Where(edge => !IsEdgeCreated(edge)))
            {
                var otherNode = edge.Other(existingNode);
                edgesToConnect.Push(GenerationStep.FromEdge(existingNode, otherNode));
            }
        }

        private void RegisterRoom(RoomNode room)
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

        private void UnregisterRoom(RoomNode room)
        {
            var positions = room.GetTileWorldPositions();
            positions.ForEach(pos => _occupiedTiles.Remove(pos));
        }
        
        private bool GenerateConnectedRoom(GenerationStep step)
        {
            // Start from a new shuffle
            if (step.RoomShuffle == null)
            {
                if (rooms.RoomsOfType[step.OtherNode.RoomType].IsEmpty())
                {
                    throw new InvalidOperationException($"Rooms of type {step.OtherNode.RoomType} is empty");
                }
                step.RoomShuffle = rooms.RoomsOfType[step.OtherNode.RoomType].ToArray();
                step.RoomShuffle.ShuffleInPlace(_random);
            }
            // Resume the current shuffle and try spawning the next room
            else
            {
                step.ShuffleIndex++;
            }

            for (var i = step.ShuffleIndex; i < step.RoomShuffle.Length; i++)
            {
                // If the room doesn't support the amount of connections that we need
                if (step.RoomShuffle[i].ConnectionList.Count < step.OtherNode.Edges.Count) continue;
                
                // Generate room
                var roomGen = 
                    step.ExistingNode is not null ?
                        _roomNodeMap[step.ExistingNode].CreateConnection(step.RoomShuffle[i], _occupiedTiles, _random) :
                        RoomNode.FromStart(step.RoomShuffle[i]);
                if (roomGen == null) continue;
                
                _roomNodeMap[step.OtherNode] = roomGen;
                RegisterRoom(_roomNodeMap[step.OtherNode]);

                return true;
            }

            return false;
        }
    }
}
