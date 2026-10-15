using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum TutorialStep
{
    Intro,
    WalkAround,
    Explore,
    SeeGap,
    PickUpPencil,
    DrawBridge,
    CrossBridge,
    Done
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Dialogue")]
    public Sprite characterProfile;

    [Header("UI Prompts")]
    public GameObject moveKeysUI;       // A/D + Space combined
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
            case TutorialStep.Intro:
                ShowIntroDialogue();
                break;
            case TutorialStep.WalkAround:
                ShowWalkTutorial();
                break;
            case TutorialStep.Explore:
                break;
            case TutorialStep.SeeGap:
                ShowGapDialogue();
                break;
            case TutorialStep.PickUpPencil:
                ShowPencilTutorial();
                break;
            case TutorialStep.DrawBridge:
                ShowDrawTutorial();
                break;
            case TutorialStep.CrossBridge:
                ShowCrossTutorial();
                break;
            case TutorialStep.Done:
                Debug.Log("Tutorial complete!");
                break;
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
        bool movedLeft = false, movedRight = false, jumped = false;

        while (!movedLeft || !movedRight || !jumped)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movedLeft = true;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movedRight = true;
            if (Input.GetKeyDown(KeyCode.Space)) jumped = true;
            yield return null;
        }

        // Wait a moment so player can see the completion
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
        {
            yield return null;
        }

        if (clickPromptUI != null) clickPromptUI.SetActive(false);
        SetStep(TutorialStep.DrawBridge);
    }

    void ShowDrawTutorial()
    {
        if (drawPromptUI != null) drawPromptUI.SetActive(true);

        string[] lines = {
            "A pencil! It feels familiar.",
            "If I hold and drag, maybe I can draw a bridge!",
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            StartCoroutine(WaitForDrawing());
        });
    }

    IEnumerator WaitForDrawing()
    {
        while (GameObject.FindGameObjectsWithTag("Platform").Length < 2)
        {
            yield return null;
        }

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
            SetStep(TutorialStep.Done);
        });
    }

    public void OnPlayerReachedGap()
    {
        if (currentStep == TutorialStep.Explore)
        {
            SetStep(TutorialStep.SeeGap);
        }
    }
}