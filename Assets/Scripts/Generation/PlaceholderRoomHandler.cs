using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceholderRoomHandler : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public void Initialise(RoomType roomType)
    {
        spriteRenderer.color = roomType switch
        {
            RoomType.START => Color.cyan,
            RoomType.END => Color.red,
            RoomType.GOLDEN_PATH => Color.yellow,
            RoomType.SIDE_PATH => Color.green,
            RoomType.SECRET => Color.magenta,
            _ => Color.white
        };
    }
}
