using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 4f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public bool canMove = true;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float timeSinceGrounded;
    private float maxAirTime = 2f; 
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
        if (!canMove) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            timeSinceGrounded = 0f;
        }
        else
        {
            timeSinceGrounded += Time.deltaTime;

            if (timeSinceGrounded >= maxAirTime)
            {
            
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpSound.Play();
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (moveX > 0)
            spriteRenderer.flipX = false;
        else if (moveX < 0)
            spriteRenderer.flipX = true;
    }
}
