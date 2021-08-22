using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;
using Gizmos = Popcron.Gizmos;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementController : MonoBehaviour
{
    public static PlayerMovementController Instance;

    [SerializeField] public Animator animator;
    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] public ParticleSystem fallParticleSystem;
    [SerializeField] public ParticleSystem runParticleSystem;

    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] private int defaultAdditionalJumps;
    [SerializeField] private float jumpForce = 11.5f;
    [SerializeField] private float fallMultiplier = 1.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckVerticalDistance = 0.1f;
    [SerializeField] private float groundCheckHorizontalDistance = 0.256f;
    [SerializeField] private float groundedDelay = 0.1f;

    [SerializeField] private AnimationClip idleAnim;
    [SerializeField] private AnimationClip idleTransitionAnim;
    [SerializeField] private AnimationClip runAnim;
    [SerializeField] private AnimationClip jumpRiseAnim;
    [SerializeField] private AnimationClip jumpMidAnim;
    [SerializeField] private AnimationClip jumpFallAnim;
    [SerializeField] private AnimationClip landAnim;

    [EventRef]
    [SerializeField] private string hurtEvent;
    
    [EventRef]
    [SerializeField] private string jumpEvent;

    [EventRef]
    [SerializeField] private string landEvent;

    [EventRef]
    [SerializeField] private string fallingEvent;
    
    [ParamRef]
    [SerializeField] private string fallSpeedParameter;

    [Space]

    [HideInInspector] public bool facingLeft;

    [ReadOnly] [Space] public bool flyModeEnabled;
    [ReadOnly] [Space] public bool godModeEnabled;

    [ReadOnly] [SerializeField] private float stunnedTimeLeft;
    [ReadOnly] [SerializeField] private float fallHeight;
    [ReadOnly] [SerializeField] private bool stunned;
    [ReadOnly] [SerializeField] private bool canChangeAnimation = true;

    [ReadOnly] [SerializeField] private bool isGrounded;
    [ReadOnly] [SerializeField] private float lastGrounded;
    [ReadOnly] [SerializeField] private int additionalJumpsLeft;

    private Rigidbody2D rb;
    private ParticleSystem.MainModule particleSystemMainMod;

    private float lastYPos;
    private float xAxis;
    private float lastFrameXAxis;
    private float animationStunTime;

    private string currentAnimatorState;

    private Vector2 groundCheck1Origin;
    private Vector2 groundCheck1Destination;
    private Vector2 groundCheck2Origin;
    private Vector2 groundCheck2Destination;
    
    private PARAMETER_ID landParamsId;
    private EventInstance fallingInstance;


    private static readonly int FallStunTime = Animator.StringToHash("FallStunTime");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (Instance == null)
            Instance = this;

        particleSystemMainMod = fallParticleSystem.main;
        fallingInstance = RuntimeManager.CreateInstance(fallingEvent);
    }

    private void Start()
    {
        EventDescription eventDescription = RuntimeManager.GetEventDescription(landEvent);
        eventDescription.getParameterDescriptionByName("SfxStrength", out var parameterDescription);
        landParamsId = parameterDescription.id;
    }

    private void Update()
    {
        CheckGrounded();
        CheckJumping();

        ApplyAnimations();

        if (!(transform.position.y < -500)) return;
        
        transform.position = new Vector3(0, 100, 0);
        fallHeight = 0;
        Debug.LogWarning("Player fell from the map!");
    }

    private void FixedUpdate()
    {
        CheckMovement();
    }

    private void ApplyAnimations()
    {
        spriteRenderer.flipX = facingLeft;


        if (isGrounded)
        {
            if(xAxis != 0)
            {
                ChangeAnimationState(runAnim, false);

                if(!runParticleSystem.isEmitting)
                    runParticleSystem.Play();
            }
            else
            {
                if (runParticleSystem.isPlaying)
                    runParticleSystem.Stop();

                if (lastFrameXAxis != 0)
                {
                    ChangeAnimationState(idleTransitionAnim, true);
                    lastFrameXAxis = 0;
                }

                if(
                    !stunned)
                    ChangeAnimationState(idleAnim, false);
            }

            fallHeight = Mathf.Abs(fallHeight);

            if (fallHeight > 10)
            {
                if (!godModeEnabled && !flyModeEnabled)
                {
                    stunnedTimeLeft = fallHeight / 5;
                    stunned = true;
                    animationStunTime = (1f / 6f) / stunnedTimeLeft;
                    
                    FMODUnity.RuntimeManager.PlayOneShot(hurtEvent, transform.position);
                }

                particleSystemMainMod.startSizeMultiplier = 1f;
                fallParticleSystem.Play();
            }

            if (stunned)
            {
                if (stunnedTimeLeft < 0)
                    stunned = false;

                stunnedTimeLeft -= Time.deltaTime;

                animator.SetFloat(FallStunTime, animationStunTime);
                ChangeAnimationState(landAnim, true);
            }

            if(fallHeight != 0 && xAxis == 0)
            {
                animator.SetFloat(FallStunTime, 0.5f);
                ChangeAnimationState(landAnim, true, 0.334f);
            }

            if(fallHeight != 0)
            {
                particleSystemMainMod = fallParticleSystem.main;
                particleSystemMainMod.startSizeMultiplier = 0.3f;
                fallParticleSystem.Play();
                
                //FMODUnity.RuntimeManager.PlayOneShot(landEvent, transform.position);

                EventInstance land = RuntimeManager.CreateInstance(landEvent);
                
                float sfxStrength = fallHeight / 10;

                Mathf.Clamp(sfxStrength, 0, 1);
                
                land.setParameterByID(landParamsId, sfxStrength);
                land.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
                land.start();
                land.release();
            }
            
            fallingInstance.getPlaybackState(out PLAYBACK_STATE state);
            
            if(state == PLAYBACK_STATE.PLAYING)
                fallingInstance.stop(STOP_MODE.ALLOWFADEOUT);

            fallHeight = 0;
        }
        else
        {
            if (runParticleSystem.isPlaying)
                runParticleSystem.Stop();

            float lastYDistance = transform.position.y - lastYPos;

            fallHeight += lastYDistance < 0 ? lastYDistance : 0;

            // If fallen over 1 block
            if (fallHeight < -1)
            {
                ChangeAnimationState(jumpFallAnim, false);
            }
            else
            {
                //TODO: Might not toggle mid when still rising?
                // If upwards movement slowing down or we are falling, and not playing jump rise
                if (rb.velocity.y < 5 && currentAnimatorState == jumpRiseAnim.name)
                {
                    ChangeAnimationState(jumpMidAnim, false);
                }
                else
                {
                    // If in a rising motion
                    if(rb.velocity.y > 0 && currentAnimatorState != jumpMidAnim.name)
                        ChangeAnimationState(jumpRiseAnim, false);
                }
            }
            
            RuntimeManager.StudioSystem.setParameterByName(fallSpeedParameter, Mathf.Abs(rb.velocity.y / 5));

            fallingInstance.getPlaybackState(out PLAYBACK_STATE state);
            
            if(state == PLAYBACK_STATE.STOPPED)
                fallingInstance.start();
        }

        lastFrameXAxis = xAxis;
        lastYPos = transform.position.y;
    }

    private void CheckMovement()
    {
        //float horizontal = Input.GetAxis("Horizontal");
        xAxis = Input.GetAxisRaw("Horizontal");

        if (stunned)
            xAxis = 0;

        if (xAxis > 0)
            facingLeft = false;
        else if (xAxis < 0)
            facingLeft = true;

        rb.velocity = new Vector2(xAxis * movementSpeed, rb.velocity.y);
    }

    void CheckGrounded()
    {
        Vector3 position = groundCheck.position;
        groundCheck1Origin = new Vector2(position.x - groundCheckHorizontalDistance, position.y);
        groundCheck1Destination = groundCheck1Origin - new Vector2(0, groundCheckVerticalDistance);
        groundCheck2Origin = new Vector2(position.x + groundCheckHorizontalDistance, position.y);
        groundCheck2Destination = groundCheck2Origin - new Vector2(0, groundCheckVerticalDistance);

        //TODO: If later having problems with checking ground, use the ground layer overload below?
        RaycastHit2D leftHit = Physics2D.Linecast(groundCheck1Origin, groundCheck1Destination, groundLayer);
        RaycastHit2D rightHit = Physics2D.Linecast(groundCheck2Origin, groundCheck2Destination, groundLayer);

        if (leftHit.collider != null || rightHit.collider != null || (flyModeEnabled && Input.GetKeyDown(Settings.JumpKey)))
        {
            isGrounded = true;
            additionalJumpsLeft = defaultAdditionalJumps;
        }
        else
        {
            if (isGrounded)
            {
                lastGrounded = Time.time;
            }
            isGrounded = false;
        }
    }

    private void CheckJumping()
    {
        if (
            Input.GetKeyDown(Settings.JumpKey)
            && !stunned
            && (isGrounded
            || Time.time - lastGrounded <= groundedDelay
            || additionalJumpsLeft > 0)
            || Input.GetKey(Settings.JumpKey) && flyModeEnabled)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            if(isGrounded)
                RuntimeManager.PlayOneShot(jumpEvent, transform.position);
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity * (fallMultiplier) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(Settings.JumpKey))
        {
            rb.velocity += Vector2.up * Physics2D.gravity * (lowJumpMultiplier) * Time.deltaTime;
        }

        additionalJumpsLeft--;
    }

    private void ChangeAnimationState(AnimationClip newState, bool mustFinish, float customTime = -1f)
    {
        if (currentAnimatorState == newState.name)
            return;

        if (canChangeAnimation)
        {
            animator.Play(newState.name);

            //if (customTime != -1)
            //    Debug.Log("Playing " + newState.name + " for: " + customTime);
            //else
            //{
            //    Debug.Log("Playing " + newState.name + " for: " + newState.length);
            //}
            currentAnimatorState = newState.name;
        }



        if (mustFinish && canChangeAnimation)
        {
            Invoke(nameof(OnAnimationFinished), customTime != -1 ? customTime : newState.length);
        }

        if(canChangeAnimation)
            canChangeAnimation = !mustFinish;
    }

    private void Attack()
    {
        // Perform attack by querying over the attack area
        RaycastHit2D[] hits = Physics2D.BoxCastAll(new Vector2(), new Vector2(), 0, new Vector2());

        foreach (RaycastHit2D hit in hits)
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.Damage(50);
        }
    }

    private void OnAnimationFinished()
    {
        canChangeAnimation = true;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.Line(groundCheck1Origin, groundCheck1Destination, GetColorByBool(isGrounded));
        Gizmos.Line(groundCheck2Origin, groundCheck2Destination, GetColorByBool(isGrounded));

        //if(wallsCheck != null)
        //{
        //    Gizmos.color = GetColorByBool(hasSpaceLeft);
        //
        //    Gizmos.DrawLine(wallsCheck1Origin, wallsCheck1Destination);
        //    Gizmos.DrawLine(wallsCheck2Origin, wallsCheck2Destination);
        //
        //
        //    Gizmos.color = GetColorByBool(hasSpaceRight);
        //}
    }

    private static Color GetColorByBool(bool flag)
    {
        return flag ? Color.green : Color.red;
    }
}
