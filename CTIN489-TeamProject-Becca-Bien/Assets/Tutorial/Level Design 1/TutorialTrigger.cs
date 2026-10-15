using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public enum TriggerType { Gap, Pencil, Bridge }

    [SerializeField] private TriggerType triggerType = TriggerType.Gap;

    private bool hasFired = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasFired) return;
        if (!other.CompareTag("Player")) return;

        hasFired = true;

        if (TutorialManager.Instance == null) return;

        switch (triggerType)
        {
            case TriggerType.Gap:
                TutorialManager.Instance.OnPlayerReachedGap();
                break;
            case TriggerType.Pencil:
                // Future: pencil trigger
                break;
            case TriggerType.Bridge:
                // Future: bridge trigger
                break;
        }
    }
}