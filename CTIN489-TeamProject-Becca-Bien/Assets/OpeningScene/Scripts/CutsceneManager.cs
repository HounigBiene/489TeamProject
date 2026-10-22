using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject skipButton;

    [Header("Cutscene Data")]
    public GameObject[] comicPanels; 
    public string nextSceneName = "TutorialScene"; 

    [Header("Page Flip Settings")]
    public float pageFlipPauseTime = 1.0f;

    private int currentPanelIndex = 0;
    private bool isPaused = false;

    void Start()
    {
        // 1. Turn OFF all panels at the start
        foreach (GameObject panel in comicPanels)
        {
            panel.SetActive(false);
        }

        // 2. Start at the first panel
        currentPanelIndex = 0;
        ShowPanel();
    }

    void Update()
    {
        if (isPaused) return; 

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            NextPanel();
        }
    }

    public void NextPanel()
    {
        currentPanelIndex++;

        if (currentPanelIndex >= comicPanels.Length)
        {
            LoadTutorial();
            return;
        }

        // Check if the NEXT panel is a Clear Scene
        string nextPanelName = comicPanels[currentPanelIndex].name;

        if (nextPanelName.Contains("Clear"))
        {
            // 1. Turn on the Clear Scene
            comicPanels[currentPanelIndex].SetActive(true);

            // 2. Turn on the panel right after the Clear Scene
            if (currentPanelIndex + 1 < comicPanels.Length)
            {
                comicPanels[currentPanelIndex + 1].SetActive(true);
                
                // 3. Skip ahead so the next click goes to the panel AFTER the one we just showed
                currentPanelIndex++; 
            }

            // 4. Pause briefly for the "page flip" effect
            StartCoroutine(PageFlipPause());
        }
        else
        {
            // Normal behavior: just show the next panel
            ShowPanel();
        }
    }

    void ShowPanel()
    {
        if (comicPanels.Length > 0 && currentPanelIndex < comicPanels.Length)
        {
            comicPanels[currentPanelIndex].SetActive(true);
        }
    }

    IEnumerator PageFlipPause()
    {
        isPaused = true;
        yield return new WaitForSeconds(pageFlipPauseTime);
        isPaused = false;
    }

    public void SkipCutscene()
    {
        LoadTutorial();
    }

    void LoadTutorial()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}