using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    private AudioSource audioSource;
    
    [Header("Audio Clips")]
    public AudioClip collectableSound;
    public AudioClip pencilPickupSound;
    public AudioClip coloringSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    public void PlayCollectableSound() => PlaySound(collectableSound);
    public void PlayPencilPickupSound() => PlaySound(pencilPickupSound);
    public void PlayColoringSound() => PlaySound(coloringSound);

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}