using UnityEngine;
using UnityEngine.UI;

public class PencilHUD : MonoBehaviour
{
    [Header("UI References")]
    public GameObject hudContainer; 
    public Image leadFillImage;     

    [Header("Durability Settings")]
    public float maxPencilUseTime = 2f; 

    void Start()
    {
        // Start with it hidden
        if (hudContainer != null)
            hudContainer.SetActive(false);
    }

    void Update()
    {
        // 1. Find the player pencil component. 
        // We use FindObjectOfType to guarantee we find it, even if Instance is being weird.
        PlayerPencil playerPencil = FindObjectOfType<PlayerPencil>();
        
        if (playerPencil == null) return;

        // 2. Check if we are holding a pencil
        if (playerPencil.isHoldingPencil && playerPencil.currentPencil != null)
        {
            // Show the UI
            if (hudContainer != null && !hudContainer.activeSelf)
                hudContainer.SetActive(true);

            // Update the bar
            if (leadFillImage != null)
            {
                leadFillImage.color = playerPencil.currentPencil.colorValue;
                
                float fillPercentage = 1f - (playerPencil.currentPencil.currentUseTime / maxPencilUseTime);
                leadFillImage.fillAmount = Mathf.Clamp01(fillPercentage);
            }
        }
        else
        {
            // Hide the UI (Only hide if we are absolutely not holding a pencil)
            if (hudContainer != null && hudContainer.activeSelf)
                hudContainer.SetActive(false);
        }
    }
}