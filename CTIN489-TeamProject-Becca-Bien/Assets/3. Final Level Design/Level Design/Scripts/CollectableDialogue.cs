
using UnityEngine;

public class CollectableDialogue: MonoBehaviour
{
    [Header("Dialogue Settings")]
    public Sprite characterProfile;

    [Header("Flower Collection")]
    public int totalFlowers = 3;

    [Header("Ending Scene")]
    public GameObject endingScene;


    private int collectedFlowers = 0;
    private bool hasShownDialogue = false;

    public void CollectFlower()
    {
        if (hasShownDialogue)
            return;

        collectedFlowers++;

        Debug.Log(
            "Flowers collected: " +
            collectedFlowers + "/" + totalFlowers
        );

        if (collectedFlowers >= totalFlowers)
        {
            TriggerFlowerDialogue();
        }
    }

    private void TriggerFlowerDialogue()
    {
        if (hasShownDialogue)
            return;

        if (endingScene != null)
        {
            endingScene.SetActive(true);
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning(
                "DialogueManager not found!"
            );
            return;
        }

        hasShownDialogue = true;

        string[] lines =
        {
            "Yay! I found all my flowers!",
            "Now it's time to get back home."
        };

        DialogueManager.Instance.ShowDialogue(
            "Hero",
            characterProfile,
            lines
        );
    }
}
