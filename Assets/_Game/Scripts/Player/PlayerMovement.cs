using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 4f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public bool canMove = true; // New: movement toggle

    private Rigidbody2D rb;
    private bool isGrounded;
    private AudioSource jumpSound;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        jumpSound = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!canMove) return; // Don't accept input when disabled

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpSound.Play();
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return; // Skip physics updates when disabled

        float moveX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // Flip player sprite if moving left or right
        if (moveX > 0)
        {
            spriteRenderer.flipX = false; // Face right
        }
        else if (moveX < 0)
        {
            spriteRenderer.flipX = true;  // Face left
        }
    }
}
