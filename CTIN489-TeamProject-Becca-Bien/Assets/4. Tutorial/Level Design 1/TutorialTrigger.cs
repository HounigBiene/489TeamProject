using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public enum TriggerType { Gap, DrawMore, Pencil, ToVerticalSlice }

    [SerializeField] private TriggerType triggerType = TriggerType.Gap;

    private bool hasFired = false;
    private bool playerInZone = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = true;
        TryFire();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!hasFired) TryFire();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInZone = false;
    }

    void TryFire()
    {
        // If it already fired successfully, do nothing
        if (hasFired) return; 
        if (TutorialManager.Instance == null) return;

        bool successfullyTriggered = false;

        switch (triggerType)
        {
            case TriggerType.Gap:
                // Check if tutorial is at the right step
                if (TutorialManager.Instance.currentStep == TutorialStep.Explore || 
                    TutorialManager.Instance.currentStep == TutorialStep.WalkAround)
                {
                    TutorialManager.Instance.OnPlayerReachedGap();
                    successfullyTriggered = true;
                }
                break;

            case TriggerType.DrawMore:
                // Check if tutorial is at the right step
                if (TutorialManager.Instance.currentStep == TutorialStep.WaitingForDrawMore || 
                    TutorialManager.Instance.currentStep == TutorialStep.CrossBridge)
                {
                    TutorialManager.Instance.OnPlayerReachedDrawMoreArea();
                    successfullyTriggered = true;
                }
                break;

            case TriggerType.ToVerticalSlice:
                if (TutorialManager.Instance.currentStep == TutorialStep.Done)
                {
                    TutorialManager.Instance.OnPlayerReachedEnd();
                    successfullyTriggered = true;
                }
                break;
        }

        // ONLY lock the trigger if it actually did something!
        if (successfullyTriggered)
        {
            hasFired = true;
            Debug.Log("Trigger locked: " + triggerType);
        }
    }
}