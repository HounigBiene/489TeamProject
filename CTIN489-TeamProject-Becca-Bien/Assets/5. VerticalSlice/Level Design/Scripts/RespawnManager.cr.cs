using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    private Vector3 currentCheckpoint;
    private bool hasCheckpoint = false;
    private GameObject player;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && !hasCheckpoint)
        {
            currentCheckpoint = player.transform.position;
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpoint = position;
        hasCheckpoint = true;
        Debug.Log("Checkpoint saved at: " + position);
    }

    public void RespawnPlayer()
    {
        if (player == null) return;

        // RECORD DEATH METRICS
        if (DeathTracker.Instance != null)
        {
            DeathTracker.Instance.RecordDeath(player.transform.position);
        }

        // Move player to checkpoint
        player.transform.position = currentCheckpoint;

        // Stop their movement
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Debug.Log("Player respawned at checkpoint.");
    }
}