
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject skipButton;

    [Header("Cutscene Data")]
    public GameObject[] comicPanels;
    public string nextSceneName = "TutorialScene";

    [Header("Page Flip Settings")]
    public float pageFlipPauseTime = 1.0f;

    [Header("SFX")]
    public AudioClip pageFlipSound;
    [Range(0f, 1f)] public float pageFlipVolume = 1f;

    private AudioSource audioSource;

    private int currentPanelIndex = 0;
    private bool isPaused = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
    }

    private void Start()
    {
        // Hide all panels at the beginning.
        foreach (GameObject panel in comicPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        if (comicPanels == null || comicPanels.Length == 0)
        {
            Debug.LogWarning("No comic panels assigned!");
            return;
        }

        // Show the first two panels.
        currentPanelIndex = 0;
        ShowPanel();

        if (comicPanels.Length > 1)
        {
            currentPanelIndex = 1;
            ShowPanel();
        }

        // Play the opening sound once.
        PlayPageFlipSound();
    }

    private void Update()
    {
        if (isPaused)
            return;

        if (Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            NextPanel();
        }
    }

    public void NextPanel()
    {
        if (isPaused)
            return;

        currentPanelIndex++;

        if (currentPanelIndex >= comicPanels.Length)
        {
            LoadTutorial();
            return;
        }

        string nextPanelName = comicPanels[currentPanelIndex].name;

        if (nextPanelName.Contains("Clear"))
        {
            // Reveal the clear page.
            comicPanels[currentPanelIndex].SetActive(true);

            // Reveal the panel immediately after it.
            if (currentPanelIndex + 1 < comicPanels.Length)
            {
                comicPanels[currentPanelIndex + 1].SetActive(true);
                currentPanelIndex++;
            }
        }
        else
        {
            ShowPanel();
        }

        // Play once for every new panel/page advancement.
        PlayPageFlipSound();

        // Optional page flip pause.
        // StartCoroutine(PageFlipPause());
    }

    private void ShowPanel()
    {
        if (comicPanels == null ||
            currentPanelIndex < 0 ||
            currentPanelIndex >= comicPanels.Length)
        {
            return;
        }

        if (comicPanels[currentPanelIndex] != null)
            comicPanels[currentPanelIndex].SetActive(true);
    }

    private void PlayPageFlipSound()
    {
        if (audioSource == null)
        {
            Debug.LogError("Cutscene AudioSource is missing!");
            return;
        }

        if (pageFlipSound == null)
        {
            Debug.LogError("Page Flip Sound is not assigned!");
            return;
        }

        Debug.Log("Playing page flip sound!");

        audioSource.PlayOneShot(pageFlipSound, pageFlipVolume);
    }

    private IEnumerator PageFlipPause()
    {
        isPaused = true;

        yield return new WaitForSeconds(pageFlipPauseTime);

        isPaused = false;
    }

    public void SkipCutscene()
    {
        LoadTutorial();
    }

    private void LoadTutorial()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
