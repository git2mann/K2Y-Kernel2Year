// ============================================
// Scripts/Player/PlayerController.cs
// ============================================
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float airControl = 0.7f; // Reduced control in air
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float doubleJumpForce = 12f;
    [SerializeField] private float variableJumpHeight = 0.5f; // How much holding jump extends it
    [SerializeField] private float maxJumpTime = 0.3f; // Max time jump can be held
    [SerializeField] private int maxJumps = 2; // Ground jump + double jump
    
    [Header("Gravity Settings")]
    [SerializeField] private float normalGravity = 3f;
    [SerializeField] private float fallGravity = 5f; // Faster falling for snappy feel
    [SerializeField] private float lowJumpGravity = 8f; // When releasing jump early
    [SerializeField] private float maxFallSpeed = 20f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask = 1;
    [SerializeField] private float coyoteTime = 0.1f; // Grace period after leaving ground
    [SerializeField] private float jumpBufferTime = 0.1f; // Grace period for early jump input
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem doubleJumpParticles;

    [Header("Particle Spawner")]
    [SerializeField] private SimpleParticleSpawner particleSpawner;
    
    [Header("Audio")]
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip landSound;

    [Header("Wall Detection")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private LayerMask wallLayerMask = 1; // Same as ground usually

    [Header("Wall Slide Settings")]
    [SerializeField] private bool canWallSlide = true;
    [SerializeField] private float wallSlideSpeed = 3f; // Medium descent speed
    [SerializeField] private ParticleSystem wallSlideParticles;

    [Header("Wall Jump Settings")]
    [SerializeField] private bool canWallJump = true;
    [SerializeField] private float wallJumpForce = 15f;
    [SerializeField] private float wallJumpDirection = 1.2f; // How far to push away from wall
    [SerializeField] private float wallJumpTime = 0.2f; // Time player can't control movement after wall jump
    [SerializeField] private int maxWallJumps = 999; // Essentially unlimited for consecutive wall jumps
    [SerializeField] private float wallJumpCooldown = 0.05f; // Brief cooldown between wall jumps

    [Header("Animation")]
    public Animator animator;
    
    // Private variables
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    
    // Movement state
    private float horizontalInput;
    private float currentSpeed;
    private bool facingRight = true;
    
    // Robust movement tracking
    private float inputHeldTime;
    private float lastMovementTime;
    
    // Smooth movement for visual consistency
    private Vector2 smoothedVelocity;
    
    // Jump state
    private bool isGrounded;
    private bool wasGrounded;
    private int jumpsRemaining;
    private bool isJumping;
    private float jumpTimeCounter;
    private bool jumpInputHeld;
    private bool jumpInputPressed;
    
    // Coyote time and jump buffering
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    
    // State tracking
    private bool canDoubleJump = true;
    
    // Wall state
    private bool isTouchingWallLeft;
    private bool isTouchingWallRight;
    private bool isWallSliding;
    private bool wasWallSliding;
    private int wallJumpsRemaining;
    private float wallJumpTimeCounter;
    private bool isWallJumping;
    private float wallJumpCooldownTimer;
    private bool lastWallWasLeft; // Track wall alternation for smart reset

    // Animation parameter name constants (prevents typos)
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");
    private static readonly int IsJumpingParam = Animator.StringToHash("IsJumping");
    private static readonly int IsFallingParam = Animator.StringToHash("IsFalling");
    private static readonly int IsWallSlidingParam = Animator.StringToHash("IsWallSliding");

    // Animation smoothing
    private float animationSpeed;
    private bool isMoving;
    private float movementTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        // Set up ground check if not assigned
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = groundCheckObj.transform;
        }

        // Set up audio source if not assigned
        if (playerAudioSource == null)
        {
            playerAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (particleSpawner == null)
        {
            particleSpawner = GetComponent<SimpleParticleSpawner>();
        }

        // Get animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Set initial gravity
        rb.gravityScale = normalGravity;
        
        // Configure Rigidbody2D for smooth movement
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        // Set up wall check points if not assigned
        if (wallCheckLeft == null)
        {
            GameObject wallCheckLeftObj = new GameObject("WallCheckLeft");
            wallCheckLeftObj.transform.SetParent(transform);
            wallCheckLeftObj.transform.localPosition = Vector3.left * 0.5f;
            wallCheckLeft = wallCheckLeftObj.transform;
        }

        if (wallCheckRight == null)
        {
            GameObject wallCheckRightObj = new GameObject("WallCheckRight");
            wallCheckRightObj.transform.SetParent(transform);
            wallCheckRightObj.transform.localPosition = Vector3.right * 0.5f;
            wallCheckRight = wallCheckRightObj.transform;
        }

        // Initialize wall jump count
        wallJumpsRemaining = maxWallJumps;
    }

    void Update()
    {
        HandleInput();
        CheckGrounded();
        CheckWalls();
        UpdateCoyoteTime();
        UpdateJumpBuffer();
        HandleJumpLogic();
        HandleWallSlide();
        UpdateWallJumpTimer();
        UpdateWallJumpCooldown();
        UpdateVisualEffects();
        // UpdateAnimations(); // Temporarily disabled until parameters are set up
        
        // Keep the simple animation parameters that might already exist
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            try
            {
                animator.SetFloat("yVelocity", rb.linearVelocity.y);
                animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
                animator.SetBool("isWallSliding", isWallSliding);
            }
            catch (System.Exception)
            {
                // Parameters don't exist - that's fine for now
            }
        }
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        HandleGravity();
        
        // Handle wall slide velocity in FixedUpdate for consistency
        if (isWallSliding)
        {
            // Force the downward movement, keeping minimal horizontal velocity
            float horizontalVel = rb.linearVelocity.x * 0.3f; // Reduce horizontal velocity significantly
            rb.linearVelocity = new Vector2(horizontalVel, -wallSlideSpeed);
        }
        
        ClampFallSpeed();
        
        // Smooth velocity for visual consistency
        smoothedVelocity = Vector2.Lerp(smoothedVelocity, rb.linearVelocity, Time.fixedDeltaTime * 15f);
    }
    
    void HandleInput()
    {
        // Horizontal movement input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Jump input
        jumpInputPressed = Input.GetKeyDown(KeyCode.Space);
        jumpInputHeld = Input.GetKey(KeyCode.Space);
        
        // Buffer jump input
        if (jumpInputPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }
    
    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        
        Collider2D groundHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
        isGrounded = groundHit != null;
        
        // Reset jumps when grounded
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
            canDoubleJump = true;
            wallJumpsRemaining = maxWallJumps; // Reset wall jumps when touching ground
            isWallSliding = false;
            isWallJumping = false;
            wallJumpCooldownTimer = 0f; // Reset cooldown
            OnLanded();
        }
        
        // Start coyote time when leaving ground
        if (wasGrounded && !isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
    }

    void CheckWalls()
    {
        // Check for walls on left and right
        isTouchingWallLeft = Physics2D.Raycast(wallCheckLeft.position, Vector2.left, wallCheckDistance, wallLayerMask);
        isTouchingWallRight = Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallCheckDistance, wallLayerMask);
        
        // Enhanced debug for wall jump troubleshooting
        if (jumpInputPressed && !isGrounded)
        {
            Debug.Log($"WALL JUMP DEBUG - TouchingLeft: {isTouchingWallLeft}, TouchingRight: {isTouchingWallRight}, " +
                     $"WallJumpsRemaining: {wallJumpsRemaining}, Cooldown: {wallJumpCooldownTimer:F2}, " +
                     $"CanWallJump: {canWallJump}, IsWallJumping: {isWallJumping}");
        }
    }

    void HandleWallSlide()
    {
        if (!canWallSlide) return;
        
        wasWallSliding = isWallSliding;
        
        // Check if player can wall slide - requires directional input toward wall
        bool canSlideLeft = isTouchingWallLeft && !isGrounded && rb.linearVelocity.y <= 0 && horizontalInput < 0;
        bool canSlideRight = isTouchingWallRight && !isGrounded && rb.linearVelocity.y <= 0 && horizontalInput > 0;
        
        isWallSliding = canSlideLeft || canSlideRight;
        
        if (isWallSliding)
        {
            // Start wall slide effects
            if (!wasWallSliding)
            {
                OnWallSlideStart();
            }
            
            // Continue wall slide effects
            UpdateWallSlideEffects();
        }
        else
        {
            // Stop wall slide effects
            if (wasWallSliding)
            {
                OnWallSlideEnd();
            }
        }
    }

    void OnWallSlideStart()
    {
        Debug.Log("Wall slide started");
        
        // Start particle trail
        if (wallSlideParticles != null)
        {
            wallSlideParticles.Play();
        }
        else if (particleSpawner != null)
        {
            // Fallback to spawning particles manually if no ParticleSystem assigned
            StartCoroutine(SpawnWallSlideParticles());
        }
    }

    void UpdateWallSlideEffects()
    {
        // Position particle system at wall contact point
        if (wallSlideParticles != null)
        {
            Vector3 wallPosition;
            if (isTouchingWallLeft)
            {
                wallPosition = wallCheckLeft.position;
            }
            else
            {
                wallPosition = wallCheckRight.position;
            }
            
            wallSlideParticles.transform.position = wallPosition;
        }
    }

    void OnWallSlideEnd()
    {
        Debug.Log("Wall slide ended");
        
        // Stop particle trail
        if (wallSlideParticles != null)
        {
            wallSlideParticles.Stop();
        }
        
        // Stop manual particle spawning
        StopCoroutine(SpawnWallSlideParticles());
    }

    // Manual particle spawning for wall slide if no ParticleSystem is assigned
    System.Collections.IEnumerator SpawnWallSlideParticles()
    {
        while (isWallSliding)
        {
            if (particleSpawner != null && particleSpawner.particlePrefab != null)
            {
                // Spawn a few particles
                Vector3 wallPosition = isTouchingWallLeft ? wallCheckLeft.position : wallCheckRight.position;
                
                for (int i = 0; i < 2; i++)
                {
                    GameObject particle = Instantiate(particleSpawner.particlePrefab);
                    // Lower the spawn position and add some vertical spread
                    particle.transform.position = wallPosition + Vector3.down * 0.3f + Vector3.up * Random.Range(-0.1f, 0.1f);
                    
                    // Push particles away from wall
                    Vector2 direction = isTouchingWallLeft ? Vector2.right : Vector2.left;
                    direction.y = Random.Range(-0.5f, 0.5f);
                    
                    Rigidbody2D rb = particle.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.AddForce(direction * 2f, ForceMode2D.Impulse);
                    }
                    
                    // Start fade coroutine
                    StartCoroutine(FadeWallParticle(particle));
                }
            }
            
            yield return new WaitForSeconds(0.1f); // Spawn particles every 0.1 seconds
        }
    }

    System.Collections.IEnumerator FadeWallParticle(GameObject particle)
    {
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        
        Vector3 originalScale = particle.transform.localScale;
        Color originalColor = sr.color;
        float lifetime = 0.8f;
        float elapsed = 0f;
        
        while (elapsed < lifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lifetime;
            
            // Shrink and fade
            particle.transform.localScale = originalScale * (1f - progress);
            sr.color = Color.Lerp(originalColor, Color.clear, progress);
            
            yield return null;
        }
        
        if (particle != null)
        {
            Destroy(particle);
        }
    }

    void UpdateWallJumpTimer()
    {
        if (isWallJumping)
        {
            wallJumpTimeCounter -= Time.deltaTime;
            if (wallJumpTimeCounter <= 0f)
            {
                isWallJumping = false;
                Debug.Log("Wall jump control lock ended - ready for next wall jump");
            }
        }
    }
    
    void UpdateWallJumpCooldown()
    {
        if (wallJumpCooldownTimer > 0f)
        {
            wallJumpCooldownTimer -= Time.deltaTime;
        }
    }
    
    void HandleJumpLogic()
    {
        // Check different types of jumps available
        bool canGroundJump = (isGrounded || coyoteTimeCounter > 0f);
        bool canAirJump = (!isGrounded && coyoteTimeCounter <= 0f && jumpsRemaining > 0 && canDoubleJump && !isWallSliding);
        
        // FIXED: More permissive wall jump logic
        bool canWallJumpNow = false;
        if (canWallJump && !isGrounded && wallJumpCooldownTimer <= 0f)
        {
            // Can wall jump if touching any wall and have jumps remaining
            if ((isTouchingWallLeft || isTouchingWallRight) && wallJumpsRemaining > 0)
            {
                canWallJumpNow = true;
            }
        }
        
        bool canJump = canGroundJump || canAirJump || canWallJumpNow;
        
        // Enhanced debug for wall jump issues
        if (jumpInputPressed)
        {
            Debug.Log($"JUMP INPUT DEBUG - CanJump: {canJump}, Ground: {canGroundJump}, Air: {canAirJump}, Wall: {canWallJumpNow}");
            Debug.Log($"WALL JUMP CONDITIONS - TouchingWall: {isTouchingWallLeft || isTouchingWallRight}, " +
                     $"WallJumpsRemaining: {wallJumpsRemaining}, Cooldown: {wallJumpCooldownTimer:F2}, " +
                     $"IsGrounded: {isGrounded}, IsJumping: {isJumping}");
        }
        
        // Debug jump attempts
        if (jumpInputPressed && !canJump)
        {
            Debug.Log($"JUMP BLOCKED - Ground: {canGroundJump}, Air: {canAirJump}, Wall: {canWallJumpNow}, " +
                     $"TouchingWall: {isTouchingWallLeft || isTouchingWallRight}, Grounded: {isGrounded}");
        }
        
        // Start jump
        if (jumpBufferCounter > 0f && canJump && !isJumping)
        {
            if (canWallJumpNow)
            {
                Debug.Log("ATTEMPTING WALL JUMP!");
                StartWallJump();
            }
            else
            {
                StartJump();
            }
        }
        
        // Continue variable height jump (but not during wall jump control lock)
        if (isJumping && jumpInputHeld && jumpTimeCounter > 0f && !isWallJumping)
        {
            ContinueJump();
        }
        
        // End jump early if button released
        if (isJumping && !jumpInputHeld)
        {
            EndJump();
        }
        
        // Update jump timer
        if (isJumping)
        {
            jumpTimeCounter -= Time.deltaTime;
            if (jumpTimeCounter <= 0f)
            {
                EndJump();
            }
        }
    }
    
    void StartJump()
    {
        // Determine jump type
        bool isDoubleJump = !isGrounded && coyoteTimeCounter <= 0f;
        
        // Set jump force
        float jumpPower = isDoubleJump ? doubleJumpForce : jumpForce;
        
        // Apply jump
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        
        // Update state
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        jumpBufferCounter = 0f; // Consume jump buffer
        coyoteTimeCounter = 0f; // Consume coyote time
        
        if (isDoubleJump)
        {
            jumpsRemaining--; // Use up the double jump
            canDoubleJump = false; // Disable further double jumps until grounded
            OnDoubleJump();
        }
        else
        {
            // Ground jump - reset jump system
            jumpsRemaining = maxJumps - 1; // Used ground jump, have 1 air jump left
            canDoubleJump = true; // Enable double jump for when we're airborne
            OnJump();
        }
        
        Debug.Log($"Jump started: {(isDoubleJump ? "Double" : "Normal")} jump, {jumpsRemaining} jumps remaining, canDoubleJump: {canDoubleJump}");
    }
    
    void ContinueJump()
    {
        // Add upward force for variable jump height
        float extraJumpForce = jumpForce * variableJumpHeight * Time.deltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + extraJumpForce);
    }
    
    void EndJump()
    {
        isJumping = false;
        jumpTimeCounter = 0f;
        
        // If moving upward, reduce velocity for short hop
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    void StartWallJump()
    {
        // Stop wall slide immediately
        isWallSliding = false;
        
        // Determine wall jump direction and track which wall we're jumping from
        Vector2 wallJumpVelocity;
        
        if (isTouchingWallLeft)
        {
            // Jump away from left wall (to the right)
            wallJumpVelocity = new Vector2(wallJumpDirection * moveSpeed, wallJumpForce);
            
            // Reset wall jumps if alternating walls (smart system)
            if (!lastWallWasLeft && wallJumpsRemaining < maxWallJumps)
            {
                wallJumpsRemaining = maxWallJumps;
                Debug.Log("Wall jump reset - alternated from right to left wall");
            }
            lastWallWasLeft = true;
            Debug.Log("Wall jump from LEFT wall → RIGHT");
        }
        else if (isTouchingWallRight)
        {
            // Jump away from right wall (to the left)
            wallJumpVelocity = new Vector2(-wallJumpDirection * moveSpeed, wallJumpForce);
            
            // Reset wall jumps if alternating walls (smart system)
            if (lastWallWasLeft && wallJumpsRemaining < maxWallJumps)
            {
                wallJumpsRemaining = maxWallJumps;
                Debug.Log("Wall jump reset - alternated from left to right wall");
            }
            lastWallWasLeft = false;
            Debug.Log("Wall jump from RIGHT wall → LEFT");
        }
        else
        {
            // Fallback (shouldn't happen)
            wallJumpVelocity = new Vector2(0, wallJumpForce);
            Debug.LogWarning("Wall jump triggered but no wall detected!");
        }
        
        // Apply wall jump velocity
        rb.linearVelocity = wallJumpVelocity;
        
        // Update state with more forgiving settings
        isJumping = false; // Don't set isJumping to allow immediate next wall jump
        isWallJumping = true;
        jumpTimeCounter = 0f; // Reset jump timer
        wallJumpTimeCounter = wallJumpTime;
        wallJumpCooldownTimer = wallJumpCooldown; // Start cooldown
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        
        // Smart wall jump consumption - only reduce when jumping from same wall repeatedly
        if (wallJumpsRemaining > 0)
        {
            wallJumpsRemaining--;
        }
        
        // Call wall jump event
        OnWallJump();
        OnWallSlideEnd(); // Ensure wall slide effects stop
        
        Debug.Log($"Wall jump SUCCESS! Velocity: {wallJumpVelocity}, Remaining: {wallJumpsRemaining}");
    }

    void OnWallJump()
    {
        // Play wall jump sound (different from regular jump)
        if (doubleJumpSound != null && playerAudioSource != null)
        {
            playerAudioSource.pitch = Random.Range(1.2f, 1.4f);
            playerAudioSource.PlayOneShot(doubleJumpSound);
        }
        
        // Spawn wall jump particles
        if (particleSpawner != null)
        {
            Vector3 wallPosition = isTouchingWallLeft ? wallCheckLeft.position : wallCheckRight.position;
            particleSpawner.SpawnJumpParticles();
        }
        
        // Visual effect - brief flash
        StartCoroutine(WallJumpFlash());
    }

    System.Collections.IEnumerator WallJumpFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    void UpdateCoyoteTime()
    {
        if (coyoteTimeCounter > 0)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }
    
    void UpdateJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    
    void HandleMovement()
    {
        // Reduce movement control during wall jump
        float movementMultiplier = isWallJumping ? 0.1f : 1f;
        
        // During wall slide, reduce horizontal movement toward the wall
        if (isWallSliding)
        {
            movementMultiplier = 0.2f; // Allow very minimal horizontal movement during wall slide
        }
        
        // Calculate target speed
        float targetSpeed = horizontalInput * moveSpeed * movementMultiplier;
        
        // Determine acceleration/deceleration rate
        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            accelRate = acceleration;
        }
        else
        {
            accelRate = deceleration;
        }
        
        // Reduce control in air (but more control during wall slide)
        if (!isGrounded)
        {
            if (isWallSliding)
            {
                accelRate *= 0.5f; // Reduced control during wall slide
            }
            else
            {
                accelRate *= airControl;
            }
        }
        
        // Smooth movement calculation with better precision
        float speedDifference = targetSpeed - currentSpeed;
        float maxChange = accelRate * Time.fixedDeltaTime;
        
        // Apply movement change with proper clamping
        if (Mathf.Abs(speedDifference) < maxChange)
        {
            // Snap to target if we're close enough to prevent accumulation errors
            currentSpeed = targetSpeed;
        }
        else
        {
            // Apply incremental change
            currentSpeed += Mathf.Sign(speedDifference) * maxChange;
        }
        
        // REMOVED THE EMERGENCY CORRECTION - it was causing the problem!
        
        // Apply smooth movement to rigidbody
        Vector2 newVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        rb.linearVelocity = newVelocity;
        
        // Optional: Much less frequent debug info (only if really needed)
        if (Time.frameCount % 300 == 0 && Mathf.Abs(horizontalInput) > 0) // Every 5 seconds when moving
        {
            Debug.Log($"Movement Debug - Input: {horizontalInput:F2}, CurrentSpeed: {currentSpeed:F2}, TargetSpeed: {targetSpeed:F2}");
        }
        
        // Handle sprite flipping (but not during wall jump control lock)
        if (!isWallJumping)
        {
            if (horizontalInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && facingRight)
            {
                Flip();
            }
        }
    }
    
    void HandleGravity()
    {
        // Special case for wall sliding - override gravity
        if (isWallSliding)
        {
            rb.gravityScale = 0f; // No gravity during wall slide
            // Set the wall slide speed directly in FixedUpdate
            return;
        }
        
        // Different gravity based on jump state
        if (rb.linearVelocity.y < 0)
        {
            // Falling - use fall gravity for snappy feel
            rb.gravityScale = fallGravity;
        }
        else if (rb.linearVelocity.y > 0 && !jumpInputHeld)
        {
            // Rising but not holding jump - use low jump gravity
            rb.gravityScale = lowJumpGravity;
        }
        else
        {
            // Normal gravity (grounded or holding jump while rising)
            rb.gravityScale = normalGravity;
        }
    }
    
    void ClampFallSpeed()
    {
        // Prevent falling too fast
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }

    void OnJump()
    {
        // Play jump sound
        if (jumpSound != null && playerAudioSource != null)
        {
            playerAudioSource.pitch = Random.Range(0.9f, 1.1f);
            playerAudioSource.PlayOneShot(jumpSound);
        }

        // Play jump particles (only if not already playing)
        if (jumpParticles != null && !jumpParticles.isPlaying)
        {
            jumpParticles.transform.position = groundCheck.position;
            jumpParticles.Stop(); // Ensure it's stopped
            jumpParticles.Play(); // Then play once
        }

        SimpleParticleSpawner spawner = GetComponent<SimpleParticleSpawner>();
        if (spawner != null)
        {
            spawner.SpawnJumpParticles();
        }

        // Trigger jump animation
        if (animator != null)
        {
            animator.SetTrigger("jump");
        }
    }

    void OnDoubleJump()
    {
        // Play double jump sound
        if (doubleJumpSound != null && playerAudioSource != null)
        {
            playerAudioSource.pitch = Random.Range(1.1f, 1.3f);
            playerAudioSource.PlayOneShot(doubleJumpSound);
        }

        // Play double jump particles (only if not already playing)
        if (doubleJumpParticles != null && !doubleJumpParticles.isPlaying)
        {
            doubleJumpParticles.transform.position = transform.position;
            doubleJumpParticles.Stop(); // Ensure it's stopped
            doubleJumpParticles.Play(); // Then play once
        }

        // Visual effect - brief sprite flash
        StartCoroutine(DoubleJumpFlash());
        
        // Trigger jump animation
        if (animator != null)
        {
            animator.SetTrigger("jump");
        }
    }

    void OnLanded()
    {
        // Play land sound
        if (landSound != null && playerAudioSource != null && rb.linearVelocity.y < -2f)
        {
            playerAudioSource.pitch = Random.Range(0.8f, 1.0f);
            playerAudioSource.PlayOneShot(landSound);
        }
        
        // Play land particles (only if landing with significant velocity)
        if (landParticles != null && rb.linearVelocity.y < -2f && !landParticles.isPlaying)
        {
            landParticles.transform.position = groundCheck.position;
            landParticles.Stop(); // Ensure it's stopped
            landParticles.Play(); // Then play once
        }
        
        Debug.Log("Player landed, jumps reset");
    }
    
    System.Collections.IEnumerator DoubleJumpFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.cyan;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    void UpdateVisualEffects()
    {
        // Add any continuous visual effects here
        // Could add dust clouds when moving, etc.
    }

    // Handle all animation state updates
    void UpdateAnimations()
    {
        // Only update animations if animator exists and has a controller
        if (animator == null || animator.runtimeAnimatorController == null) 
        {
            return;
        }

        // Determine movement state with proper buffering
        float speedThreshold = 0.3f;
        bool inputReceived = Mathf.Abs(horizontalInput) > 0.1f;
        bool hasVelocity = Mathf.Abs(currentSpeed) > speedThreshold;
        
        // Movement state logic with timer to prevent rapid switching
        if (inputReceived && hasVelocity)
        {
            movementTimer = 0.15f; // Buffer time
            isMoving = true;
        }
        else if (movementTimer > 0)
        {
            movementTimer -= Time.deltaTime;
            // Keep isMoving true until timer expires
        }
        else
        {
            isMoving = false;
        }

        // Smooth speed calculation
        float targetSpeed = isMoving ? Mathf.Abs(currentSpeed) : 0f;
        animationSpeed = Mathf.Lerp(animationSpeed, targetSpeed, Time.deltaTime * 8f);

        // Safely set animation parameters (only if they exist)
        try
        {
            // Use boolean for walk instead of speed float
            animator.SetBool("isWalking", isMoving);
            animator.SetFloat(SpeedParam, animationSpeed);
            animator.SetBool(IsGroundedParam, isGrounded);
            animator.SetBool(IsJumpingParam, isJumping && rb.linearVelocity.y > 0);
            animator.SetBool(IsFallingParam, !isGrounded && rb.linearVelocity.y < -0.1f && !isWallSliding);
            animator.SetBool(IsWallSlidingParam, isWallSliding);
            
            // Debug info (remove after fixing)
            if (Time.frameCount % 30 == 0)
            {
                Debug.Log($"Animation - isMoving: {isMoving}, Speed: {animationSpeed:F2}, Input: {horizontalInput:F2}, Velocity: {currentSpeed:F2}");
            }
        }
        catch (System.Exception)
        {
            // Parameters don't exist yet - this is normal until you create the Animator Controller
            // Debug.LogWarning("Animation parameters not found. Create Animator Controller with required parameters.");
        }
    }
    
    // Public methods for external systems
    public bool IsGrounded => isGrounded;
    public bool IsJumping => isJumping;
    public bool IsWallSliding => isWallSliding;
    public int JumpsRemaining => jumpsRemaining;
    public bool CanDoubleJump => canDoubleJump;
    public float CurrentSpeed => currentSpeed;
    public Vector2 Velocity => rb.linearVelocity;
    
    // Method to reset player state (useful for respawning)
    public void ResetPlayerState()
    {
        rb.linearVelocity = Vector2.zero;
        currentSpeed = 0f;
        isJumping = false;
        jumpTimeCounter = 0f;
        jumpsRemaining = maxJumps;
        canDoubleJump = true;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        smoothedVelocity = Vector2.zero; // Reset smoothed velocity too
        animationSpeed = 0f; // Reset animation speed
        isMoving = false; // Reset movement state
        movementTimer = 0f; // Reset movement timer
    }
    
    // Method to add external forces (useful for explosions, etc.)
    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
    {
        rb.AddForce(force, mode);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw ground check area
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // Draw wall check rays
        if (wallCheckLeft != null)
        {
            Gizmos.color = isTouchingWallLeft ? Color.cyan : Color.gray;
            Gizmos.DrawRay(wallCheckLeft.position, Vector2.left * wallCheckDistance);
        }
        
        if (wallCheckRight != null)
        {
            Gizmos.color = isTouchingWallRight ? Color.cyan : Color.gray;
            Gizmos.DrawRay(wallCheckRight.position, Vector2.right * wallCheckDistance);
        }
        
        // Draw movement info
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.right * currentSpeed);
    }
}