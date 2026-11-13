using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool hasBeenCollected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected) return;
        if (!other.CompareTag("Player")) return;

        hasBeenCollected = true;

        // Tell the respawn manager to save this spot
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.SetCheckpoint(transform.position);
        }

        // Destroy the collectable so it disappears
        Destroy(gameObject);
    }
}