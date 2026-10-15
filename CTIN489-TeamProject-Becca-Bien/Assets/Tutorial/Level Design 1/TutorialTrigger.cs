using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialTrigger : MonoBehaviour
{
    public enum TriggerType { Gap, DrawMore, Pencil, ToVerticalSlice }

    [SerializeField] private TriggerType triggerType = TriggerType.Gap;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private string sceneName = "VerticalSlice"; // for ToVerticalSlice

    private bool hasFired = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && hasFired) return;
        if (!other.CompareTag("Player")) return;

        hasFired = true;

        if (TutorialManager.Instance == null)
        {
            Debug.LogWarning("TutorialTrigger: No TutorialManager in scene!");
            return;
        }

        switch (triggerType)
        {
            case TriggerType.Gap:
                TutorialManager.Instance.OnPlayerReachedGap();
                break;
            case TriggerType.DrawMore:
                TutorialManager.Instance.OnPlayerReachedDrawMoreArea();
                break;
            case TriggerType.Pencil:
                Debug.Log("Pencil trigger fired");
                break;
            case TriggerType.ToVerticalSlice:
                TutorialManager.Instance.OnPlayerReachedEnd();
                break;
        }
    }
}