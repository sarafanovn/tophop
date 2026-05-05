using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float rotateSpeed = 720f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Double Jump")]
    [SerializeField] private float doubleJumpForce = 7f;

    [Header("Jump Feel")]
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float lowJumpMultiplier = 2.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Mobile Input")]
    [SerializeField] private FixedJoystick joystick;
    [SerializeField] private JumpButton jumpButton;

    // ── refs ───────────────────────────────────────────────
    private Rigidbody _rb;
    [SerializeField] private Animator _anim;
    [SerializeField] private Transform startPosition;

    // ── state ──────────────────────────────────────────────
    private bool _grounded;
    private bool _wasGrounded;
    private bool _wasMoving;
    private bool _wasFalling;
    private bool _doubleJumpActive;
    private bool _doubleJumpUsed;
    private Coroutine _doubleJumpTimer;

    // all animation params in this controller are Triggers,
    // except "blend feeling" which is Float
    private static readonly int TrigIdle = Animator.StringToHash("idle");
    private static readonly int TrigRun = Animator.StringToHash("run");
    private static readonly int TrigJump = Animator.StringToHash("jump");
    private static readonly int TrigFall = Animator.StringToHash("fall");
    private static readonly int TrigGetup = Animator.StringToHash("getup");
    private static readonly int FloatBlend = Animator.StringToHash("blend feeling");

    public int CoinCount { get; private set; }

    public UnityEvent OnDoubleJump;

    // ── lifecycle ──────────────────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Animator may live on a child mesh object
        _anim = GetComponentInChildren<Animator>();
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        if (transform.position.y < -5) transform.position = startPosition.position;

        LayerMask mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask;
        _grounded = Physics.CheckSphere(groundCheck.position, groundRadius, mask, QueryTriggerInteraction.Ignore)
                 || Physics.Raycast(groundCheck.position, Vector3.down, groundRadius + 0.05f, mask, QueryTriggerInteraction.Ignore);

        if (_grounded && !_wasGrounded)
        {
            _doubleJumpUsed = false;
            Fire(TrigGetup);
        }
        _wasGrounded = _grounded;

        bool jumpPressed = Input.GetButtonDown("Jump")
                        || (jumpButton != null && jumpButton.ConsumePress());
        if (jumpPressed) TryJump();

        UpdateAnimator();

    }

    private void FixedUpdate()
    {
        Vector2 stick = GetStickInput();
        Vector3 dir = new Vector3(stick.x, 0f, stick.y).normalized;

        _rb.linearVelocity = new Vector3(dir.x * moveSpeed, _rb.linearVelocity.y, dir.z * moveSpeed);

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotateSpeed * Time.fixedDeltaTime);
        }

        ApplyJumpGravity();
    }

    private void ApplyJumpGravity()
    {
        float vy = _rb.linearVelocity.y;

        if (vy < 0f)
        {
            // heavier fall
            _rb.linearVelocity += Vector3.up * (Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime);
        }
        else if (vy > 0f && !JumpHeld())
        {
            // cut apex short when button released
            _rb.linearVelocity += Vector3.up * (Physics.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime);
        }
    }

    private bool JumpHeld() =>
        Input.GetButton("Jump") || (jumpButton != null && jumpButton.IsHeld);

    // ── input ──────────────────────────────────────────────

    private Vector2 GetStickInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h != 0f || v != 0f)
            return new Vector2(h, v);

        return joystick != null ? joystick.Direction : Vector2.zero;
    }

    // ── jump ───────────────────────────────────────────────

    public void TryJump()
    {
        if (_grounded)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, jumpForce, _rb.linearVelocity.z);
            Fire(TrigJump);
            PlaySfx(jumpSound);
        }
        else if (_doubleJumpActive && !_doubleJumpUsed)
        {
            OnDoubleJump.Invoke();
            _doubleJumpUsed = true;
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, doubleJumpForce, _rb.linearVelocity.z);
            Fire(TrigJump);
            PlaySfx(doubleJumpSound);
        }
    }

    // ── animator ───────────────────────────────────────────

    private void UpdateAnimator()
    {
        if (_anim == null) return;

        float speed = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z).magnitude;
        bool moving = speed > 0.1f && _grounded;
        bool falling = !_grounded && _rb.linearVelocity.y < -0.5f;

        if (moving && !_wasMoving) Fire(TrigRun);
        if (!moving && _grounded && _wasMoving) Fire(TrigIdle);
        if (falling && !_wasFalling) Fire(TrigFall);

        _wasMoving = moving;
        _wasFalling = falling;

        _anim.SetFloat(FloatBlend, speed / moveSpeed);
    }

    private void Fire(int hash)
    {
        if (_anim != null) _anim.SetTrigger(hash);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position, sfxVolume);
    }

    // ── coins ──────────────────────────────────────────────

    public void OnCoinCollected() => CoinCount++;

    // ── double jump upgrade ────────────────────────────────

    public void ActivateDoubleJump(float duration)
    {
        if (_doubleJumpTimer != null)
            StopCoroutine(_doubleJumpTimer);
        _doubleJumpTimer = StartCoroutine(DoubleJumpCountdown(duration));
    }

    private IEnumerator DoubleJumpCountdown(float duration)
    {
        _doubleJumpActive = true;
        DoubleJumpDuration = duration;
        DoubleJumpTimeLeft = duration;

        while (DoubleJumpTimeLeft > 0f)
        {
            DoubleJumpTimeLeft -= Time.deltaTime;
            yield return null;
        }

        DoubleJumpTimeLeft = 0f;
        _doubleJumpActive = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }

    public bool IsDoubleJumpActive => _doubleJumpActive;
    public float DoubleJumpTimeLeft { get; private set; }
    public float DoubleJumpDuration { get; private set; }
    public float MoveSpeed => moveSpeed;
}
