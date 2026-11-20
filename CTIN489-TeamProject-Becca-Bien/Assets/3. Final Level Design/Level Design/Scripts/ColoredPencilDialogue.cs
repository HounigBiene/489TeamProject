
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ColoredPencilDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public Sprite characterProfile;

    private bool hasTriggered = false;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only activate when the player enters.
        if (!other.CompareTag("Player"))
            return;

        // Only play the dialogue once.
        if (hasTriggered)
            return;

        if (DialogueManager.Instance == null)
            return;

        hasTriggered = true;

        string[] lines =
        {
            "A colored pencil!",
            "I think I can help her complete some of these drawings!"
        };

        DialogueManager.Instance.ShowDialogue(
            "Hero",
            characterProfile,
            lines
        );
    }
}
