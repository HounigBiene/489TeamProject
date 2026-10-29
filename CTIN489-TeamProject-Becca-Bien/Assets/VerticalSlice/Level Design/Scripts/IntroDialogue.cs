using UnityEngine;

public class IntroDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public Sprite characterProfile;

    void Start()
    {
        // Depay
        Invoke("PlayIntroDialogue", 0.2f);
    }

    void PlayIntroDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            string[] lines = {
                "These must be my creator's childhood drawings...",
                "They look so different.",
                "When I fell, I lost my basket of flowers too.",
                "I think the best idea is to get all my belongings back",
                "and return to my own page."
            };

            DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
                Debug.Log("Intro dialogue finished. The player can now move!");
            });
        }
        else
        {
            Debug.LogError("DialogueManager is missing from the scene!");
        }
    }
}