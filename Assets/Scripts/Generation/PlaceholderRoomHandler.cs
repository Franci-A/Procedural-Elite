using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaceholderRoomHandler : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TMP_Text idText;

    public void Initialise(Node node)
    {
        spriteRenderer.color = node.Type switch
        {
            RoomType.START => Color.blue,
            RoomType.END => Color.red,
            RoomType.GOLDEN_PATH => Color.yellow,
            RoomType.SIDE_PATH => Color.green,
            RoomType.SECRET => Color.magenta,
            _ => Color.white
        };

        idText.text = node.NodeId.ToString();
        idText.color = node.Type switch
        {
            RoomType.START => Color.white,
            RoomType.END => Color.white,
            _ => Color.black
        };
    }
}
