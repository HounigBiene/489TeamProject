using UnityEngine;
using UnityEngine.UI;

public class PencilHUD : MonoBehaviour
{
    [Header("UI References")]
    public GameObject hudContainer; // The outside border (always visible)
    public Image leadFillImage;     // The inside lead (turns on/off)

    [Header("Durability Settings")]
    public float maxPencilUseTime = 2f; 

    void Update()
    {
        PlayerPencil playerPencil = FindObjectOfType<PlayerPencil>();
        if (playerPencil == null) return;

        // When holding a pencil
        if (playerPencil.isHoldingPencil && playerPencil.currentPencil != null)
        {
            // Turn the LEAD on
            if (leadFillImage != null && !leadFillImage.gameObject.activeSelf)
                leadFillImage.gameObject.SetActive(true);

            // Calculate how much is left
            if (leadFillImage != null)
            {
                float fillPercentage = 1f - (playerPencil.currentPencil.currentUseTime / maxPencilUseTime);
                leadFillImage.fillAmount = Mathf.Clamp01(fillPercentage);
            }
        }
        else
        {
            // Turn the LEAD off (but leave the container on)
            if (leadFillImage != null && leadFillImage.gameObject.activeSelf)
                leadFillImage.gameObject.SetActive(false);
        }
    }
}