using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attack is an hitbox script that destroy itself after a given lifetime or when triggered.
/// When hitting player or enemy, applies damages and knockback to hit entity. See Player and Enemy "OnTriggerEnter2D" method for more details.
/// </summary>
public class Attack : MonoBehaviour {

    public int damages = 1;
	public bool hasInfiniteLifetime = false;
	[HideIf("hasInfiniteLifetime")]
    public float lifetime = 0.3f;
    public float knockbackSpeed = 3;
    public float knockbackDuration = 0.5f;
	public LayerMask destroyOnHit;

	[System.NonSerialized]
    public GameObject owner;

	public bool isProjectile = false;
	[ShowIf(nameof(isProjectile))]
	public float speed = 2;
	private Vector3 direction;

	public UnityEvent onHit;

    private void Start()
    {
        direction = new Vector3(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad),0);
    }

    void Update () {
		if (hasInfiniteLifetime)
			return;

		lifetime -= Time.deltaTime;
		if (lifetime <= 0.0f)
		{
			GameObject.Destroy(gameObject);
		}
	}

    private void FixedUpdate()
    {
        if (isProjectile)
        {
			transform.position += Time.fixedDeltaTime * speed * direction;

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if(((1 << collision.gameObject.layer) & destroyOnHit) != 0)
		{
			GameObject.Destroy(gameObject);
			onHit?.Invoke();
		}
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
		if(((1 << collision.gameObject.layer) & destroyOnHit) != 0)
		{
			GameObject.Destroy(gameObject);
			onHit?.Invoke();
		}
	}
}
