﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Player component. Manages inputs, character states and associated game flow.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour {

    public static Player Instance = null;

    [System.Serializable]
    public class MovementParameters
    {
        public float speedMax = 2.0f;
        public float acceleration = 12.0f;
        public float friction = 12.0f;
    }

    // Possible orientation for player aiming : 4 direction, 8 direction (for keyboards or D-pads) or free direction (for analogic joysticks)
    public enum ORIENTATION
    {
        FREE,
        DPAD_8,
        DPAD_4
    }

    // Character can only be at one state at a time. For example, he can't attack and be stunned at the same time.
    public enum STATE
    {
        IDLE = 0,
        ATTACKING = 1,
        STUNNED = 2,
        DEAD = 3,
        DASHING = 4,
    }

    [SerializeField] private SpriteRenderer renderer;
    // Life and hit related attributes
    [Header("Life")]
    public int life = 3;
    public float invincibilityDuration = 1.0f;
    public float invincibilityBlinkPeriod = 0.2f;
    public LayerMask hitLayers;
    public float knockbackSpeed = 3.0f;
    public float knockbackDuration = 0.5f;
    public Color deadColor = Color.gray;

    private float _lastHitTime = float.MinValue;
    private List<SpriteRenderer> _spriteRenderers = new List<SpriteRenderer>();
    private Coroutine _blinkCoroutine = null;


    // Movement attributes
    [Header("Movement")]
    public MovementParameters defaultMovement = new MovementParameters();
    public MovementParameters stunnedMovement = new MovementParameters();

    private Rigidbody2D _body = null;
    private Vector2 _direction = Vector2.zero;
    private MovementParameters _currentMovement = null;

    [Header("Dash")]
    [SerializeField] private float dashCooldown = 1;
    [SerializeField] private float dashDuration = 1;
    [SerializeField] private float dashInvincibleTime = .5f;
    [SerializeField] private float dashForce = 1;
    private float lastDashTime = float.MinValue;
    private Vector2 dashDirection = Vector2.right;

    // Attack attributes
    [Header("Attack")]
    public WeaponsHolder weaponsHolder = null;
    public GameObject attackSpawnPoint = null;
    public float attackCooldown = 0.3f;
    public ORIENTATION orientation = ORIENTATION.FREE;

    private float lastAttackTime = float.MinValue;
    public WeaponType GetSelectedWeaponType { get => weaponsHolder.GetSelectedType; }

    // Input attributes
    [Header("Input")]
    [Range(0.0f, 1.0f)]
    public float controllerDeadZone = 0.3f;

    // State attributes
    private STATE _state = STATE.IDLE;

    // Collectible attributes
    private int _keyCount;
    public int KeyCount { get { return _keyCount; } set { _keyCount = value; } }

	// Dungeon position
	private Room _room = null;
	public Room Room { get { return _room; } }

	private void Awake () {
        Instance = this;
        _body = GetComponent<Rigidbody2D>();
        GetComponentsInChildren<SpriteRenderer>(true, _spriteRenderers);
    }

    private void Start()
    {
        SetState(STATE.IDLE);
        OnChangeClass();
        weaponsHolder.isSpecialActive = false;
        weaponsHolder.OnClassChanged.AddListener(OnChangeClass);
    }

    private void Update () {
        UpdateState();
        UpdateInputs();
        UpdateRoom();
    }

    private void FixedUpdate()
    {
        FixedUpdateMovement();
	}

    /// <summary>
    /// Updates any room related behaviours. By default, move from one room to another when reaching 
    /// </summary>
	private void UpdateRoom()
	{
        Bounds roomBounds = _room.GetWorldBounds();
        Room nextRoom = null;
        if(transform.position.x > roomBounds.max.x)
        {
            nextRoom = _room.GetAdjacentRoom(Utils.ORIENTATION.EAST, transform.position);
        } else if(transform.position.x < roomBounds.min.x)
        {
            nextRoom = _room.GetAdjacentRoom(Utils.ORIENTATION.WEST, transform.position);
        } else if (transform.position.y > roomBounds.max.y)
        {
            nextRoom = _room.GetAdjacentRoom(Utils.ORIENTATION.NORTH, transform.position);
        }
        else if (transform.position.y < roomBounds.min.y)
        {
            nextRoom = _room.GetAdjacentRoom(Utils.ORIENTATION.SOUTH, transform.position);
        }

		if(nextRoom != null)
		{
            EnterRoom(nextRoom);
		}
	}

	/// <summary>
    /// Updates inputs
    /// </summary>
	private void UpdateInputs()
    {
        if (CanMove())
        {
            _direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_direction.magnitude < controllerDeadZone)
            {
                _direction = Vector2.zero;
            } else {
                _direction.Normalize();
                dashDirection = _direction;
            }
            if(Input.GetButtonDown("Fire1")) {
                Attack();
            }else if (Input.GetButtonDown("Dash")){
                Dash();
            }
        } else {
            _direction = Vector2.zero;
        }
    }

    /// <summary>
    /// Updates current state
    /// </summary>
    private void UpdateState()
    {
        switch(_state)
        {
			case STATE.ATTACKING:
				SpawnAttackPrefab();
				SetState(STATE.IDLE);
				break;
			default: break;
        }
    }

    /// <summary>
    /// Changes current state to a new given state. Instructions related to exiting and entering a state should be coded in the two "switch(_state){...}" of this method.
    /// </summary>    
    private void SetState(STATE state)
    {
        // Exiting previous state
        // switch (_state)
        //{
        //}

        _state = state;

        // Entering new state
        switch (_state)
        {
            case STATE.STUNNED: _currentMovement = stunnedMovement; break;
            case STATE.DEAD: 
                EndBlink();
                SetColor(deadColor);
                break;
            default: _currentMovement = defaultMovement; break;
        }

        // Reset direction if player cannot move in this state
        if (!CanMove())
        {
            _direction = Vector2.zero;
        }
    }


    /// <summary>
    /// Updates velocity and frictions
    /// </summary>
    void FixedUpdateMovement()
    {
        if (_state == STATE.DASHING)
        {
            _body.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        }
        else if (_direction.magnitude > Mathf.Epsilon) // magnitude > 0
        {
            // If direction magnitude > 0, Accelerate in direction, then clamp velocity to max speed. Do not apply friction if character is moving toward a direction.
            _body.velocity += _direction * _currentMovement.acceleration * Time.fixedDeltaTime;
            _body.velocity = Vector2.ClampMagnitude(_body.velocity, _currentMovement.speedMax);
            transform.eulerAngles = new Vector3(0.0f, 0.0f, ComputeOrientationAngle(_direction));
        } else {
            // If direction magnitude == 0, Apply friction
            float frictionMagnitude = _currentMovement.friction * Time.fixedDeltaTime;
            if (_body.velocity.magnitude > frictionMagnitude)
            {
                _body.velocity -= _body.velocity.normalized * frictionMagnitude;
            } else {
                _body.velocity = Vector2.zero;
            }
        }
    }

    /// <summary>
	/// Sets player in attack state. Attack prefab will be spawned when calling SpawnAttackPrefab method a little later.
    /// </summary>
	private void Attack()
    {
        if (Time.time - lastAttackTime < weaponsHolder.GetCurrentThreshold().attackCooldown)
            return;
        lastAttackTime = Time.time;
        SetState(STATE.ATTACKING);
    }

    /// <summary>
    /// Spawns the associated "attack" prefab on attackSpawnPoint.
    /// </summary>
    private void SpawnAttackPrefab()
    {
        if (weaponsHolder.GetWeaponPrefab() == null)
            return;

        // transform used for spawn is attackSpawnPoint.transform if attackSpawnPoint is not null. Else it's transform.
        Transform spawnTransform = attackSpawnPoint ? attackSpawnPoint.transform : transform;
        GameObject.Instantiate(weaponsHolder.GetWeaponPrefab(), spawnTransform.position, spawnTransform.rotation);
    }

    /// <summary>
    /// Called when player takes a hit (ie from enemy hitbox or spikes).
    /// </summary>
    public void ApplyHit(Attack attack)
    {
        if (Time.time - lastDashTime < dashInvincibleTime)
            return;
        
        if (Time.time - _lastHitTime < invincibilityDuration)
            return;
        _lastHitTime = Time.time;

        life -= (attack != null ? attack.damages : 1);
        if (life <= 0)
        {
            SetState(STATE.DEAD);
        } else {
            if (attack != null && attack.knockbackDuration > 0.0f)
            {
                StartCoroutine(ApplyKnockBackCoroutine(attack.knockbackDuration, attack.transform.right * attack.knockbackSpeed));
            }
            EndBlink();
            _blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }
    }

    /// <summary>
    /// Puts player in STUNNED state and sets a velocity to knockback player. It resume to IDLE state after a fixed duration. STUNNED state has his own movement parameters that allow to redefine frictions when character is knocked.
    /// </summary>
    private IEnumerator ApplyKnockBackCoroutine(float duration, Vector3 velocity)
    {
        SetState(STATE.STUNNED);
        _body.velocity = velocity;
        yield return new WaitForSeconds(duration);
        SetState(STATE.IDLE);
    }

    /// <summary>
    /// Makes all sprite renderers in the player hierarchy blink from enabled to disabled with a fixed period over a fixed time.  
    /// </summary>
    private IEnumerator BlinkCoroutine()
    {
        float invincibilityTimer = 0;
        while(invincibilityTimer < invincibilityDuration)
        {
            invincibilityTimer += Time.deltaTime;
            bool isVisible = ((int)(invincibilityTimer / invincibilityBlinkPeriod)) % 2 == 1;
            foreach(SpriteRenderer spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.enabled = isVisible;
            }
            yield return null; // wait next frame
        }
        EndBlink();
    }

    /// <summary>
    /// Stops current blink coroutine if any is started and set all sprite renderers to enabled.
    /// </summary>
    private void EndBlink()
    {
        if (_blinkCoroutine == null)
            return;
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.enabled = true;
        }
        StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = null;

    }

    /// <summary>
    /// Sets the tint color of all SpriteRenderers to a given tint color.
    /// </summary>
    private void SetColor(Color color)
    {
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// Transforms the orientation vector into a discrete angle.
    /// </summary>
    private float ComputeOrientationAngle(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        switch(orientation)
        {
            case ORIENTATION.DPAD_8: return Utils.DiscreteAngle(angle, 45); // Only 0 45 90 135 180 225 270 315
            case ORIENTATION.DPAD_4: return Utils.DiscreteAngle(angle, 90); // Only 0 90 180 270
            default: return angle;
        }
    }

    /// <summary>
    /// Returns whether or not player can moves and attack
    /// </summary>
    private bool CanMove()
    {
        return _state == STATE.IDLE;
    }

    /// <summary>
    /// Called to enter a room
    /// </summary>
	public void EnterRoom(Room room)
	{
        Room previous = _room;
        _room = room;
        room.OnEnterRoom(previous);
    }

    /// <summary>
    /// Checks if player gets hit by any attack hitbox. Applies attack data (damages, knockback, ...) to player.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( ((1 << collision.gameObject.layer) & hitLayers) != 0 )
        {
            // Collided with hitbox
            Attack attack = collision.gameObject.GetComponent<Attack>();
            ApplyHit(attack);
        }
    }

    public void Dash()
    {
        if (!weaponsHolder.GetWeapon().canDash || !weaponsHolder.isSpecialActive)
            return; 

        if (Time.time - lastDashTime < dashCooldown + dashDuration)
            return;
        lastDashTime = Time.time ;

        SetState(STATE.DASHING);
        StartCoroutine(DashReset());
    }
    
    IEnumerator DashReset()
    {
        yield return new WaitForSeconds(dashDuration);
        SetState(STATE.IDLE);
    }

    private void OnChangeClass()
    {
        renderer.sprite = weaponsHolder.GetPlayerSprite();
    }

    public void TpToBoss(GameObject bossRoom)
    {
        var newCoordonate = new Vector3(1000, 0, 0);
        Instantiate(bossRoom, newCoordonate, Quaternion.identity);
        transform.position = newCoordonate + new Vector3(7,4,0);
        Camera.main.transform.position = newCoordonate + new Vector3(7, 4, -10);
    }
}
