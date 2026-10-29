using UnityEngine;

public class Collectible : MonoBehaviour
{
    public string collectibleName = "Items";

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only let the player collect this item once.
        if (isCollected || !other.CompareTag("Player"))
            return;

        isCollected = true;

        // Record the time since gameplay started.
        if (CollectibleTimeTracker.Instance != null)
        {
            CollectibleTimeTracker.Instance.RecordCollection(
                collectibleName
            );
        }
        else
        {
            Debug.LogWarning(
                "CollectibleTimeTracker is missing from the scene!",
                this
            );
        }

        // Play collect sound.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCollectableSound();
        }

        Debug.Log(
            $"Collected: {collectibleName}"
        );

        // TODO: Add to score, play particle effect, etc.

        Destroy(gameObject);
    }
}