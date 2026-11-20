
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public string collectibleName = "Items";

    [Header("Collectible UI")]
    public CollectableUI collectableUI;

    [Header("Story Trigger (Optional)")]
    public SparkStoryTrigger storyTrigger;

    [Header("Flower Collection")]
    public bool isFlower = false;

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

        // Update the collectible UI.
        if (collectableUI != null)
        {
            collectableUI.SetCollected();
        }

        // Trigger dialogue for spark collectibles.
        if (storyTrigger != null)
        {
            storyTrigger.TriggerSparkDialogue();
        }

        // NEW: Track flower collection.
        if (isFlower)
        {
            CollectableDialogue flowerTrigger =
                FindAnyObjectByType<CollectableDialogue>();

            if (flowerTrigger != null)
            {
                flowerTrigger.CollectFlower();
            }
            else
            {
                Debug.LogWarning(
                    "FlowerDialogueTrigger is missing from the scene!",
                    this
                );
            }
        }

        Debug.Log(
            $"Collected: {collectibleName}"
        );

        Destroy(gameObject);
    }
}
