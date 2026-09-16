using UnityEngine;

public class StickerCollectible : MonoBehaviour
{
    [SerializeField] private AudioClip collectSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    collectSound,
                    transform.position
                );
            }

            Destroy(gameObject);
        }
    }
}