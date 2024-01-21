using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectProjectiles : MonoBehaviour
{
    [SerializeField] private WeaponsHolder weaponsHolder;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private GameObject fireBall;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (weaponsHolder.isSpecialActive)
        {
            if ((hitLayers & (1 << collision.gameObject.layer)) == (1 << collision.gameObject.layer))
            {
                Instantiate(fireBall, transform.position, this.transform.rotation);
                Destroy(collision.gameObject);
            }
        }
    }
}
