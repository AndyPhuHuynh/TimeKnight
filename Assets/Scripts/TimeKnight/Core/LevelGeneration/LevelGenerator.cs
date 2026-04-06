using UnityEngine;

namespace TimeKnight.Core.LevelGeneration
{
    public class LevelGenerator : MonoBehaviour
    {
        // public List<Room> Rooms;
        //
        // private static Door RoomHasMatchingDoor(Room room, Door door)
        // {
        //     return room.Doors.FirstOrDefault(
        //         otherDoor => door.Direction == DoorDirection.Left && otherDoor.Direction == DoorDirection.Right || 
        //                      door.Direction == DoorDirection.Right && otherDoor.Direction == DoorDirection.Left);
        // }
        //
        // private void Awake()
        // {
        //     Instantiate(Rooms[0].gameObject);
        //
        //     foreach (var door in Rooms[0].Doors)
        //     {
        //         foreach (var room in Rooms)
        //         {
        //             var match = RoomHasMatchingDoor(room, door);
        //             if (!match) continue;
        //             
        //             // Get offset of current match to the door
        //             var offset = match.transform.position - door.transform.position;
        //             var newPos = room.transform.position - offset;
        //             Instantiate(room.gameObject, newPos, Quaternion.identity);
        //
        //             break;
        //         }
        //     }
        // }
    }
}
