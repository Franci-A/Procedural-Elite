using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeProjectile : MonoBehaviour
{
    [SerializeField] private WeaponsHolder weapons;
    [SerializeField] private float startSize;
    private float endSize;
    [SerializeField] private float scaleTime = .5f;
    private float timer = 0;

    void Start()
    {
        endSize = weapons.GetCurrentThreshold().mageExplosionSize;
        transform.localScale = new Vector3(startSize, startSize);
    }

    private void Update()
    {
        if(timer < scaleTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(startSize, endSize, timer / scaleTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, startSize);
        Gizmos.DrawWireSphere(transform.position, endSize);
    }
}
