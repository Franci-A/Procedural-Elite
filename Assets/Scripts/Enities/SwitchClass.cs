using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwitchClass : MonoBehaviour
{
    public UnityEvent OnClassChanged;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnClassChanged?.Invoke();
            Destroy(gameObject);
        }
    }
}
