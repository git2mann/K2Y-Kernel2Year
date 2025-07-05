using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("_Game/Scripts/Player/PlayerFallReset")]
public class PlayerFallReset : MonoBehaviour
{
    public PlayerController playerController;
    public LevelOneManager levelManager;
    public float fallTimeout = 3f;
    private float fallTimer = 0f;

    void Start()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelOneManager>();
    }

    void Update()
    {
        if (playerController == null || levelManager == null) return;
        if (!playerController.IsGrounded)
        {
            fallTimer += Time.deltaTime;
            if (fallTimer >= fallTimeout)
            {
                levelManager.RestartLevel();
            }
        }
        else
        {
            fallTimer = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "RetakeGames")
        {
            if (levelManager != null)
            {
                levelManager.RestartLevel();
            }
        }
    }
}
