using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SpawnRoomsList")]
public class RoomsSO : ScriptableObject
{
    public List<Room> startingRooms;
    public List<Room> rooms;
    public List<Room> keyRooms;
    public List<Room> endingRooms;
    public List<Room> secretRooms;

    public GameObject GetRoom(List<Utils.ORIENTATION> orientations, RoomType roomType)
    {
        List<Room> roomsList = new List<Room>();
        List<Room> currentRoomList = roomType switch
        {
            RoomType.START => startingRooms,
            RoomType.END => endingRooms,
            RoomType.KEY => keyRooms,
            RoomType.SECRET => secretRooms,
            _ => rooms,
        };

        foreach (var room in currentRoomList)
        {
            if(CheckRoomsOrientation(room.Orientation, orientations)) roomsList.Add(room);
        }

        if (roomsList.Count == 0) return null;
        else return roomsList[Random.Range(0, roomsList.Count)].gameObject;   
    }

    bool CheckRoomsOrientation(List<Utils.ORIENTATION> orientations, List<Utils.ORIENTATION> room)
    {
        return (orientations.Contains(Utils.ORIENTATION.NORTH) == room.Contains(Utils.ORIENTATION.NORTH) &&
                orientations.Contains(Utils.ORIENTATION.SOUTH) == room.Contains(Utils.ORIENTATION.SOUTH) &&
                orientations.Contains(Utils.ORIENTATION.EAST) == room.Contains(Utils.ORIENTATION.EAST) &&
                orientations.Contains(Utils.ORIENTATION.WEST) == room.Contains(Utils.ORIENTATION.WEST));
    }
}
