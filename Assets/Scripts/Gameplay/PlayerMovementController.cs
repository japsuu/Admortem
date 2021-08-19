using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementController : MonoBehaviour
{
    public static PlayerMovementController Instance;

    public StudioEventEmitter hurtEmitter;

    [SerializeField] public Animator animator;
    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] public ParticleSystem fallParticleSystem;
    [SerializeField] public ParticleSystem runParticleSystem;

    [SerializeField] float movementSpeed = 5f;
    [SerializeField] int defaultAdditionalJumps = 0;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckVerticalDistance = 0.5f;
    [SerializeField] float groundCheckHorizontalDistance = 0.25f;
    [SerializeField] float groundedDelay = 0.1f;

    [Space]

    [HideInInspector] public bool facingLeft = false;

    [Space] [ReadOnly] public bool flyModeEnabled = false;
    [Space] [ReadOnly] public bool godModeEnabled = false;

    [ReadOnly] [SerializeField] private float stunnedTimeLeft = 0;
    [ReadOnly] [SerializeField] private float fallHeight = 0;
    [ReadOnly] [SerializeField] private bool stunned = false;
    [ReadOnly] [SerializeField] private bool canChangeAnimation = true;

    [ReadOnly] [SerializeField] private bool isGrounded = false;
    [ReadOnly] [SerializeField] private float lastGrounded = 0;
    [ReadOnly] [SerializeField] private int additionalJumpsLeft = 0;

    private Rigidbody2D rb = null;
    private ParticleSystem.MainModule particleSystemMainMod;

    private float lastYPos = 0;
    private float xAxis = 0;
    private float lastFrameXaxis = 0;
    private float animationStunTime = 0;

    private string currentAnimatorState;

    private Vector2 groundCheck1Origin;
    private Vector2 groundCheck1Destination;
    private Vector2 groundCheck2Origin;
    private Vector2 groundCheck2Destination;



    [SerializeField] private AnimationClip idleAnim;
    [SerializeField] private AnimationClip idleTransitionAnim;
    [SerializeField] private AnimationClip runAnim;
    [SerializeField] private AnimationClip jumpRiseAnim;
    [SerializeField] private AnimationClip jumpMidAnim;
    [SerializeField] private AnimationClip jumpFallAnim;
    [SerializeField] private AnimationClip landAnim;
    private static readonly int FallStunTime = Animator.StringToHash("FallStunTime");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (Instance == null)
            Instance = this;

        particleSystemMainMod = fallParticleSystem.main;
    }

    private void Update()
    {
        CheckGrounded();
        CheckJumping();

        ApplyAnimations();
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

                if (lastFrameXaxis != 0)
                {
                    ChangeAnimationState(idleTransitionAnim, true);
                    lastFrameXaxis = 0;
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
                    
                    hurtEmitter.Play();
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
            }

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
        }

        lastFrameXaxis = xAxis;
        lastYPos = transform.position.y;
    }

    void CheckMovement()
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
        groundCheck1Origin = new Vector2(groundCheck.position.x - groundCheckHorizontalDistance, groundCheck.position.y);
        groundCheck1Destination = groundCheck1Origin - new Vector2(0, groundCheckVerticalDistance);
        groundCheck2Origin = new Vector2(groundCheck.position.x + groundCheckHorizontalDistance, groundCheck.position.y);
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

    void CheckJumping()
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

    void ChangeAnimationState(AnimationClip newState, bool mustFinish, float customTime = -1f)
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
            if(customTime != -1)
                Invoke("OnAnimationFinished", customTime);
            else
                Invoke("OnAnimationFinished", newState.length);
        }

        if(canChangeAnimation)
            canChangeAnimation = !mustFinish;
    }

    void Attack()
    {
        // Perform attack by querying over the attack area
        RaycastHit2D[] hits = Physics2D.BoxCastAll(new Vector2(), new Vector2(), 0, new Vector2());

        foreach (RaycastHit2D hit in hits)
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if(damageable != null)
            {
                damageable.Damage(50);
            }
        }
    }

    void OnAnimationFinished()
    {
        canChangeAnimation = true;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = GetColorByBool(isGrounded);

            Gizmos.DrawLine(groundCheck1Origin, groundCheck1Destination);
            Gizmos.DrawLine(groundCheck2Origin, groundCheck2Destination);
        }

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

    Color GetColorByBool(bool flag)
    {
        if (flag)
            return Color.green;
        else
            return Color.red;
    }
}
