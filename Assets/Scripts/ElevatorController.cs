using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElevatorController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text statusText;
    public TMP_Text targetText;
    public TMP_Text inputText;

    [Header("Buttons")]
    public Button[] floorButtons; // size 10, in order 1..10
    public Button resetButton;
    public Button skipButton;

    [Header("Audio (responsive)")]
    public AudioSource sfxSource;
    public AudioClip pressSfx;
    public AudioClip errorSfx;
    public AudioClip successSfx;
    public AudioClip stingSfx; // optional: woman enters

    [Header("Music (ambient)")]
    public AudioSource musicSource;
    public AudioClip musicLoop;


    [Header("Prototype Settings")]
    public List<int> targetSequence = new() { 1, 4, 2, 6, 2, 10, 5, 1 };

    private int index = 0;
    private readonly List<int> playerInput = new();
    private bool finished = false;

    void Start()
    {
        if (titleText != null) titleText.text = "ELEVATOR PHASE (Bad Version)";
        UpdateAllUI();

        // Wire buttons
        if (floorButtons != null)
        {
            for (int i = 0; i < floorButtons.Length; i++)
            {
                int floor = i + 1; // 1..10
                if (floorButtons[i] != null)
                    floorButtons[i].onClick.AddListener(() => PressFloor(floor));
            }
        }

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetRun);
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipToEnd);

        // Start music
        if (musicSource != null && musicLoop != null)
        {
            musicSource.clip = musicLoop;
            musicSource.loop = true;
            musicSource.Play();
        }


    }

    void Update()
    {
        // QOL: R reloads scene
        var kb = Keyboard.current;
        if (kb != null && kb.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void PressFloor(int floor)
    {
        if (finished) return;

        PlayOneShot(pressSfx);

        // Correct input
        if (index < targetSequence.Count && floor == targetSequence[index])
        {
            playerInput.Add(floor);
            index++;

            // Special moment: when player successfully inputs 5 (the 7th step here)
            if (floor == 5)
            {
                statusText.text = "A woman enters on Floor 5...";
                PlayOneShot(stingSfx);
            }

            // Success condition
            if (index >= targetSequence.Count)
            {
                CompleteRun();
            }
            else
            {
                statusText.text = $"Correct. Next input #{index + 1}...";
                UpdateAllUI();
            }
            return;

        }

        // Wrong input
        statusText.text = $"ERROR: Expected {targetSequence[index]}. Sequence reset.";
        PlayOneShot(errorSfx);

        index = 0;
        playerInput.Clear();
        UpdateAllUI();
    }

    void ResetRun()
    {
        finished = false;
        index = 0;
        playerInput.Clear();
        statusText.text = "Reset. Awaiting input...";
        EnableFloorButtons();
        UpdateAllUI();
    }

    void UpdateAllUI()
    {
        if (targetText != null)
            targetText.text = "Target: " + string.Join(" ", targetSequence);

        if (inputText != null)
            inputText.text = "Input: " + (playerInput.Count == 0 ? "(none)" : string.Join(" ", playerInput));
    }

    void DisableFloorButtons()
    {
        if (floorButtons == null) return;
        foreach (var b in floorButtons)
            if (b != null) b.interactable = false;
    }

    void EnableFloorButtons()
    {
        if (floorButtons == null) return;
        foreach (var b in floorButtons)
            if (b != null) b.interactable = true;
    }

    void PlayOneShot(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
    public void SkipToEnd()
    {
        if (finished) return;

        // Optional: fill input to look "complete"
        playerInput.Clear();
        playerInput.AddRange(targetSequence);
        index = targetSequence.Count;

        CompleteRun();
    }

    void CompleteRun()
    {
        finished = true;
        statusText.text = "SUCCESS: Elevator misroutes to Floor 10. Dimension breach.";
        PlayOneShot(successSfx);
        DisableFloorButtons();
        UpdateAllUI();
    }


}
