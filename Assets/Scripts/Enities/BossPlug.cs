using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossPlug : MonoBehaviour
{
    [SerializeField] private int health = 3;
    public LayerMask hitLayers;
    public UnityEvent<BossPlug> onDestroy;
    private bool isActive = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        if ((hitLayers & (1 << collision.gameObject.layer)) == (1 << collision.gameObject.layer))
        {
            // Collided with hitbox
            Attack attack = collision.gameObject.GetComponent<Attack>();
            health -= attack.damages;
            if(health <= 0) 
            { 
                isActive = false;
                onDestroy.Invoke(this);
                Destroy(gameObject);
            }
        }
    }
}
