using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image profilePic;
    public TextMeshProUGUI speakerName;
    public TextMeshProUGUI dialogueText;
    public GameObject clickIndicator;

    [Header("Settings")]
    public float typeSpeed = 0.03f;
    public float inputCooldown { get; private set; } = 0f;

    private string[] currentLines;
    private int currentLineIndex;
    private bool isTyping;
    private bool isActive;
    private Action onComplete;

    void Awake()
    {
        Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.textWrappingMode = TextWrappingModes.Normal;
        }
    }

    public bool IsActive()
    {
        return isActive || inputCooldown > 0f;
    }

    void Update()
    {
        if (inputCooldown > 0f)
            inputCooldown -= Time.deltaTime;

        if (!isActive) return;
        
        // Block input if just finished
        if (inputCooldown > 0f) return;

        // Click or space to advance
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Skip typing - show full line
                StopAllCoroutines();
                dialogueText.text = currentLines[currentLineIndex];
                isTyping = false;
                clickIndicator.SetActive(true);
            }
            else
            {
                // Advance to next line
                currentLineIndex++;
                if (currentLineIndex >= currentLines.Length)
                {
                    EndDialogue();
                }
                else
                {
                    StartCoroutine(TypeLine(currentLines[currentLineIndex]));
                }
            }
        }
    }

    public void ShowDialogue(string speaker, Sprite profile, string[] lines, Action onFinish = null)
    {
        isActive = true;
        currentLines = lines;
        currentLineIndex = 0;
        onComplete = onFinish;
        isTyping = false;

        dialoguePanel.SetActive(true);

        if (speakerName != null) speakerName.text = speaker;
        if (profilePic != null && profile != null) profilePic.sprite = profile;

        StartCoroutine(TypeLine(currentLines[0]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        clickIndicator.SetActive(false);
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        clickIndicator.SetActive(true);
    }

    void EndDialogue()
    {
        isActive = false;
        dialoguePanel.SetActive(false);
        inputCooldown = 0.2f;

        Action callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }
}