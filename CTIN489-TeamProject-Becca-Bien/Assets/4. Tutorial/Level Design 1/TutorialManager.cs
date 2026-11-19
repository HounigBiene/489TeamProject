using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using USCG.Core.Telemetry;

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
    public GameObject rwKeysUI;        // NEW: R / W UI
    public GameObject clickPromptUI;
    public GameObject drawPromptUI;
    public GameObject sharpenPromptUI; // NEW: Sharpen UI

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
        if (rwKeysUI != null) rwKeysUI.SetActive(false);
        if (clickPromptUI != null) clickPromptUI.SetActive(false);
        if (drawPromptUI != null) drawPromptUI.SetActive(false);
        if (sharpenPromptUI != null) sharpenPromptUI.SetActive(false);
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
        if (rwKeysUI != null) rwKeysUI.SetActive(true); // Turn on R/W here
        StartCoroutine(WaitForMovement());
    }

    IEnumerator WaitForMovement()
    {
        float timer = 0f;
        float maxTime = 8f; 
        bool startedMoving = false;

        // Wait for them to press a button OR wait 6 seconds
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

        // NEW: If they pressed a button immediately, this forces the UI to stay 
        // on screen for at least 4 seconds total before disappearing.
        if (timer < 5f)
        {
            yield return new WaitForSeconds(4f - timer);
        }
        else
        {
            // If they took a long time to move, just wait 1.5 seconds to show them the UI
            yield return new WaitForSeconds(1.5f);
        }

        // Only advance if we are still in the walk tutorial (prevents sequence breaking)
        if (currentStep == TutorialStep.WalkAround)
        {
            SetStep(TutorialStep.Explore);
        }
    }

    // IEnumerator WaitForMovement()
    // {
    //     float timer = 0f;
    //     float maxTime = 5f; 
    //     bool startedMoving = false;

    //     while (timer < maxTime && !startedMoving)
    //     {
    //         timer += Time.deltaTime;

    //         if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
    //             Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ||
    //             Input.GetKey(KeyCode.Space))
    //         {
    //             startedMoving = true;
    //         }

    //         yield return null;
    //     }

    //     yield return new WaitForSeconds(1.5f);
        
    //     // Only advance if we are still in the walk tutorial
    //     if (currentStep == TutorialStep.WalkAround)
    //     {
    //         SetStep(TutorialStep.Explore);
    //     }
    // }

    void ShowGapDialogue()
    {
        string[] lines = {
            "I can't jump over this gap...",
            "Wait — is that a pencil over there?",
            "Maybe I could use it for something."
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            if (currentStep == TutorialStep.SeeGap) // Ensure we didn't skip ahead
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

        yield return new WaitForSeconds(0.1f);
        if (clickPromptUI != null) clickPromptUI.SetActive(false);
        
        if (currentStep == TutorialStep.PickUpPencil)
            SetStep(TutorialStep.DrawBridge);
    }

    void ShowDrawTutorial()
    {
        if (drawPromptUI != null) drawPromptUI.SetActive(true);

        string[] lines = {
            "Maybe I can draw a bridge over to the other side?",
            "If it doesn't work I guess I can just erase it."
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
        float maxWaitTime = 20f; 

        while (drawing.StrokesCreated <= startingStrokes)
        {
            timer += Time.deltaTime;
            if (timer >= maxWaitTime) break;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        if (drawPromptUI != null) drawPromptUI.SetActive(false);

        if (currentStep == TutorialStep.DrawBridge)
            SetStep(TutorialStep.CrossBridge);
    }

    void ShowCrossTutorial()
    {
        string[] lines = {
            "It worked!",
            "I can use this to get around."
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            if (currentStep == TutorialStep.CrossBridge)
                SetStep(TutorialStep.WaitingForDrawMore); 
        });
    }

    // --- FORGIVING TRIGGERS ---

    public void OnPlayerReachedGap()
    {
        // If they are anywhere BEFORE the gap dialogue, fast-forward to it
        if (currentStep == TutorialStep.Intro || 
            currentStep == TutorialStep.WalkAround || 
            currentStep == TutorialStep.Explore)
        {
            StopAllCoroutines(); 
            SetStep(TutorialStep.SeeGap);
        }
    }

    public void OnPlayerReachedDrawMoreArea()
    {
        // If they ran past the bridge and reached the end before finishing
        if (currentStep == TutorialStep.WaitingForDrawMore || 
            currentStep == TutorialStep.CrossBridge ||
            currentStep == TutorialStep.DrawBridge ||
            currentStep == TutorialStep.PickUpPencil)
        {
            StopAllCoroutines();
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
        float maxWaitTime = 30f; 

        PlayerPencil playerPencil = PlayerPencil.Instance;

        while (true)
        {
            timer += Time.deltaTime;

            bool pencilBroken = playerPencil != null && 
                                playerPencil.currentPencil != null && 
                                playerPencil.currentPencil.isBroken;

            if (pencilBroken) break;
            if (timer >= maxWaitTime) break;

            yield return null;
        }

        yield return new WaitForSeconds(1f);
        if (drawPromptUI != null) drawPromptUI.SetActive(false);

        if (currentStep == TutorialStep.DrawMoreForFun)
            SetStep(TutorialStep.PencilBreakDialogue);
    }

    void ShowPencilBreakDialogue()
    {
        string[] lines = {
            "Oh no... my pencil snapped!",
            "I guess there's a limit to how much I can draw.",
            "Luckily there's a sharpener right here!"
        };

        DialogueManager.Instance.ShowDialogue("Hero", characterProfile, lines, () => {
            // 1. Set step to Done (this hides all other prompts)
            SetStep(TutorialStep.Done);
            
            // 2. NOW turn on the sharpener icon so it stays on screen
            if (sharpenPromptUI != null) 
                sharpenPromptUI.SetActive(true);

            // 3. Start the timer to hide it
            StartCoroutine(HideSharpenerAfterDelay(2f)); // Hides after 4 seconds
        });
    }

    IEnumerator HideSharpenerAfterDelay(float delay)
    {
        // Wait for the specified number of seconds
        yield return new WaitForSeconds(delay);

        // Turn the sharpener icon off
        if (sharpenPromptUI != null)
            sharpenPromptUI.SetActive(false);
    }

    public void OnPlayerReachedEnd()
    {
        // Accept the End Trigger if they are on Done OR PencilBreakDialogue
        // if (currentStep == TutorialStep.Done || currentStep == TutorialStep.PencilBreakDialogue)
        // {
        SaveDrawingsTelemetry.SaveDrawingData("Tutorial");

        //save csv
        if (TelemetryManager.instance != null)
        {
            TelemetryManager.instance.PrintMetricsImmediate();
        }

        Debug.Log("Loading Act 1...");
        SceneManager.LoadScene("Act 1");
        // }
    }
}