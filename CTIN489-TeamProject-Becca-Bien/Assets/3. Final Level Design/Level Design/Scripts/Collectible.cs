
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public string collectibleName = "Items";

    [Header("Flower Collection")]
    public bool isFlower = false;

    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || !other.CompareTag("Player"))
            return;

        isCollected = true;

        // Record collection time.
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

        // Play collection sound.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCollectableSound();
        }

        // Count flower collectibles.
        if (isFlower)
        {
            CollectableDialogue flowerTrigger =
                FindAnyObjectByType<CollectableDialogue>();

            if (flowerTrigger != null)
            {
                flowerTrigger.CollectFlower();
            }
        }

        Debug.Log("Collected: " + collectibleName);

        Destroy(gameObject);
    }
}
