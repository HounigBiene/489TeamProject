using UnityEngine;

public class Collectible : MonoBehaviour
{
    public string collectibleName = "Smiley";
    public int scoreValue = 1;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Play collect sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCollectableSound();
            
            Debug.Log("Collected: " + collectibleName + " (+" + scoreValue + ")");
            
            // TODO: Add to score, play particle effect, etc.
            
            Destroy(gameObject);
        }
    }
}