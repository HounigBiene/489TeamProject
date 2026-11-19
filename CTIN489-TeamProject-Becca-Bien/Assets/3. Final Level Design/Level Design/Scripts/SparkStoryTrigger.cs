using UnityEngine;

public class SparkStoryTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public Sprite characterProfile;

    // A static variable means it remembers if we've seen it, 
    // even if the object is destroyed!
    private static bool hasSeenSparkDialogue = false;

    void Start()
    {
        // Reset the static variable so we can test it again!
        hasSeenSparkDialogue = false; 
    }

    public void TriggerSparkDialogue()
    {
        // If we've already seen it, don't do anything
        if (hasSeenSparkDialogue) return;

        hasSeenSparkDialogue = true;

        if (DialogueManager.Instance != null)
        {
            string[] lines = {
                "Wow... my creator's drawings used to be so colorful!",
                "That spark just now...",
                "Maybe I can take them back home along with my flowers."
            };

            DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines);
        }
    }
}