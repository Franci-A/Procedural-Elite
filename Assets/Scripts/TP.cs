using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP : MonoBehaviour
{
    [SerializeField] private GameObject bossRoom;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Player.Instance == null)
            return;
        if (collision.attachedRigidbody.gameObject != Player.Instance.gameObject)
            return;

        Player.Instance.TpToBoss(bossRoom);
    }
}
