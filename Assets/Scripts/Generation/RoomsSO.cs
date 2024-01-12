using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(menuName = "ScriptableObjects/SpawnRoomsList")]
public class RoomsSO : ScriptableObject
{
    public List<Room> rooms;

    public GameObject GetRoom(List<Utils.ORIENTATION> orientations)
    {
        List<Room> roomsList = new List<Room>();

        foreach (var room in rooms)
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
