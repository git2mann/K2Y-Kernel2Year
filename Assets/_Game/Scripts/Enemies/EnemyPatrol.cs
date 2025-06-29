using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;
    public float waitTime = 0.5f;
    public Transform groundCheck;
public float groundCheckRadius = 0.2f;
public LayerMask groundLayer;

    private bool isGrounded;




    private Transform currentTarget;
    private bool isWaiting = false;

    void Start()
    {
        currentTarget = pointB;
    }

    void Update()
    {
        if (isWaiting || currentTarget == null) return;

        // Move enemy toward the current target
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

if (!isGrounded)
{
    // Optional: stop moving, fall, or turn around
    Debug.Log("Enemy not grounded");
}


        // Check if we've arrived (close enough)
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.01f)
        {
            // Snap exactly to the target
            transform.position = currentTarget.position;
            StartCoroutine(SwitchTargetAfterDelay());
        }
    }

    IEnumerator SwitchTargetAfterDelay()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        // Switch direction
        currentTarget = (currentTarget == pointA) ? pointB : pointA;

        isWaiting = false;
    }
}
