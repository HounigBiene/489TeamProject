using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum TutorialStep
{
    Intro,
    WalkAround,
    Explore,
    SeeGap,
    PickUpPencil,
    DrawBridge,
    CrossBridge,
    WaitingForDrawMore,
    DrawMoreForFun,
    PencilBreakDialogue,
    Done
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Dialogue")]
    public Sprite characterProfile;

    [Header("UI Prompts")]
    public GameObject moveKeysUI;
    public GameObject clickPromptUI;
    public GameObject drawPromptUI;

    [Header("Scene References")]
    public GameObject pencilObject;
    public Transform gapLocation;

    public TutorialStep currentStep { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetStep(TutorialStep.Intro);
    }

    public void SetStep(TutorialStep step)
    {
        currentStep = step;
        HideAllPrompts();

        switch (step)
        {
            case TutorialStep.Intro: ShowIntroDialogue(); break;
            case TutorialStep.WalkAround: ShowWalkTutorial(); break;
            case TutorialStep.Explore: break;
            case TutorialStep.SeeGap: ShowGapDialogue(); break;
            case TutorialStep.PickUpPencil: ShowPencilTutorial(); break;
            case TutorialStep.DrawBridge: ShowDrawTutorial(); break;
            case TutorialStep.CrossBridge: ShowCrossTutorial(); break;
            case TutorialStep.WaitingForDrawMore: break;  
            case TutorialStep.DrawMoreForFun: ShowDrawMoreForFun(); break;
            case TutorialStep.PencilBreakDialogue: ShowPencilBreakDialogue(); break;
            case TutorialStep.Done: Debug.Log("Tutorial complete!"); break;
        }
    }

    void HideAllPrompts()
    {
        if (moveKeysUI != null) moveKeysUI.SetActive(false);
        if (clickPromptUI != null) clickPromptUI.SetActive(false);
        if (drawPromptUI != null) drawPromptUI.SetActive(false);
    }

    void ShowIntroDialogue()
    {
        string[] lines = {
            "Where... am I?",
            "This doesn't look like home.",
            "Maybe I should walk around and see what's here."
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            SetStep(TutorialStep.WalkAround);
        });
    }

    void ShowWalkTutorial()
    {
        if (moveKeysUI != null) moveKeysUI.SetActive(true);
        StartCoroutine(WaitForMovement());
    }

    IEnumerator WaitForMovement()
    {
        float timer = 0f;
        float maxTime = 5f; // advance after 5 seconds regardless
        bool startedMoving = false;

        while (timer < maxTime && !startedMoving)
        {
            timer += Time.deltaTime;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ||
                Input.GetKey(KeyCode.Space))
            {
                startedMoving = true;
            }

            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        if (moveKeysUI != null) moveKeysUI.SetActive(false);
        SetStep(TutorialStep.Explore);
    }

    void ShowGapDialogue()
    {
        string[] lines = {
            "I can't jump over this gap...",
            "Wait — is that a pencil over there?",
            "Maybe I could use it for something."
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            SetStep(TutorialStep.PickUpPencil);
        });
    }

    void ShowPencilTutorial()
    {
        if (clickPromptUI != null) clickPromptUI.SetActive(true);
        StartCoroutine(WaitForPencilPickup());
    }

    IEnumerator WaitForPencilPickup()
    {
        PlayerPencil playerPencil = PlayerPencil.Instance;

        while (playerPencil == null || !playerPencil.isHoldingPencil)
            yield return null;

        yield return new WaitForSeconds(0.5f);
        if (clickPromptUI != null) clickPromptUI.SetActive(false);
        SetStep(TutorialStep.DrawBridge);
    }

    void ShowDrawTutorial()
    {
        if (drawPromptUI != null) drawPromptUI.SetActive(true);

        string[] lines = {
            "Maybe I can draw a bridge over to the other side?",
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            StartCoroutine(WaitForDrawing());
        });
    }

    IEnumerator WaitForDrawing()
    {
        Drawing drawing = FindObjectOfType<Drawing>();
        if (drawing == null) yield break;

        yield return new WaitForSeconds(0.2f);

        int startingStrokes = drawing.StrokesCreated;
        float timer = 0f;
        float maxWaitTime = 20f; // Give them 20 seconds to figure it out

        while (drawing.StrokesCreated <= startingStrokes)
        {
            timer += Time.deltaTime;
            if (timer >= maxWaitTime) 
            {
                Debug.Log("Timeout: Player didn't draw a bridge immediately.");
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        if (drawPromptUI != null) drawPromptUI.SetActive(false);
        SetStep(TutorialStep.CrossBridge);
    }

    void ShowCrossTutorial()
    {
        string[] lines = {
            "It worked!",
            "I can use this to get around."
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            SetStep(TutorialStep.WaitingForDrawMore); // Wait, don't loop
        });
    }

    public void OnPlayerReachedGap()
    {
        // If they somehow reach the gap but the step is still WalkAround/Explore
        if (currentStep == TutorialStep.Explore || currentStep == TutorialStep.WalkAround)
            SetStep(TutorialStep.SeeGap);
    }

    public void OnPlayerReachedDrawMoreArea()
    {
        // If they are in WaitingForDrawMore OR if they skipped ahead and are still in CrossBridge
        if (currentStep == TutorialStep.WaitingForDrawMore || currentStep == TutorialStep.CrossBridge)
        {
            SetStep(TutorialStep.DrawMoreForFun);
        }
    }

    void ShowDrawMoreForFun()
    {
        if (drawPromptUI != null) drawPromptUI.SetActive(true);

        string[] lines = {
            "Look at all this blank space!"
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            StartCoroutine(WaitForMoreDrawing());
        });
    }

    IEnumerator WaitForMoreDrawing()
    {
        yield return new WaitForSeconds(0.2f);
        
        float timer = 0f;
        float maxWaitTime = 30f; // Safety timeout

        PlayerPencil playerPencil = PlayerPencil.Instance;

        while (true)
        {
            timer += Time.deltaTime;

            // Just check if the pencil is broken. We don't care about strokes anymore!
            bool pencilBroken = playerPencil != null && 
                                playerPencil.currentPencil != null && 
                                playerPencil.currentPencil.isBroken;

            if (pencilBroken)
            {
                Debug.Log("Pencil broke! Advancing.");
                break;
            }

            if (timer >= maxWaitTime)
            {
                Debug.LogWarning("Safety timeout reached in DrawMore.");
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(1f);
        if (drawPromptUI != null) drawPromptUI.SetActive(false);

        SetStep(TutorialStep.PencilBreakDialogue);
    }

    void ShowPencilBreakDialogue()
    {
        string[] lines = {
            "Oh no... my pencil snapped!",
            "I guess there's a limit to how much I can draw.",
            "Luckily there's a sharpener and a new pencil right here!"
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            SetStep(TutorialStep.Done);
        });
    }

    // void ShowFindNewPencil()
    // {
    //     if (clickPromptUI != null) clickPromptUI.SetActive(true);

    //     string[] lines = {
    //         "Another pencil! I'll use it more wisely this time."
    //     };

    //     DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
    //         StartCoroutine(WaitForNewPencilPickup());
    //     });
    // }

    // IEnumerator WaitForNewPencilPickup()
    // {
    //     PlayerPencil playerPencil = PlayerPencil.Instance;

    //     // Wait until they drop the broken pencil
    //     while (playerPencil != null && playerPencil.isHoldingPencil)
    //         yield return null;

    //     // Wait until they pick up the new one
    //     while (playerPencil != null && !playerPencil.isHoldingPencil)
    //         yield return null;

    //     // Hide the prompt immediately
    //     if (clickPromptUI != null) clickPromptUI.SetActive(false);

    //     // Wait a moment for the pickup to feel complete
    //     yield return new WaitForSeconds(1.5f);

    //     string[] lines = {
    //         "Perfect! Now I know to be careful.",
    //         "Let's keep going."
    //     };

    //     DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
    //         SetStep(TutorialStep.Done);
    //     });
    // }

    //CHANGE SCENE
    public void OnPlayerReachedEnd()
    {
        if (currentStep == TutorialStep.Done)
        {
            Debug.Log("Loading Vertical Slice...");
            SceneManager.LoadScene("VerticalSlice");
        }
    }
}