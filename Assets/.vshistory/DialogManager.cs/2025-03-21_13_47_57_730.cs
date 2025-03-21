using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image characterImage;
    [SerializeField] private float defaultDisplayDuration = 3f;
    [SerializeField] private float typingSpeed = 0.05f; // Time between each character in typing animation
    [SerializeField] private AudioClip typingSound; // Sound effect for typing

    private Coroutine currentDialogCoroutine;
    private Coroutine currentBounceCoroutine;
    private Vector3 startImagePos;
    private float previousTimeScale; // To store the Time.timeScale before pausing

    [System.Serializable]
    public struct DialogLine
    {
        public string message;
        public Sprite speakerSprite;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("DialogManager: DialogPanel is not assigned!");
        }

        if (dialogText == null)
        {
            Debug.LogError("DialogManager: DialogText is not assigned!");
        }

        if (characterImage == null)
        {
            Debug.LogError("DialogManager: CharacterImage is not assigned!");
        }
        else
        {
            startImagePos = characterImage.transform.localPosition;
        }
    }

    // Show a single dialog with a message for a specified duration
    public void ShowDialog(string message, Sprite characterSprite = null, float duration = -1f)
    {
        if (currentDialogCoroutine != null)
        {
            StopCoroutine(currentDialogCoroutine);
        }
        currentDialogCoroutine = StartCoroutine(DisplayDialogWithTyping(message, characterSprite, duration >= 0f ? duration : defaultDisplayDuration));
    }

    // Show a single dialog that waits for player input to dismiss
    public void ShowDialogWithInput(string message, Sprite characterSprite = null)
    {
        if (currentDialogCoroutine != null)
        {
            StopCoroutine(currentDialogCoroutine);
        }
        currentDialogCoroutine = StartCoroutine(DisplayDialogWithInput(message, characterSprite));
    }

    // Show a sequence of dialog messages
    public void ShowDialogSequence(List<DialogLine> dialogLines, bool waitForInput = false)
    {
        if (currentDialogCoroutine != null)
        {
            StopCoroutine(currentDialogCoroutine);
        }
        currentDialogCoroutine = StartCoroutine(DisplayDialogSequence(dialogLines, waitForInput));
    }

    private IEnumerator DisplayDialogWithTyping(string message, Sprite characterSprite, float duration)
    {
        // Pause the game
        //previousTimeScale = Time.timeScale;
        //Time.timeScale = 0f;

        dialogText.text = "";
        if (characterSprite != null)
        {
            characterImage.sprite = characterSprite;
            characterImage.enabled = true;
        }
        else
        {
            characterImage.enabled = false;
        }

        dialogPanel.SetActive(true);

        if (characterImage.enabled && currentBounceCoroutine == null)
        {
            currentBounceCoroutine = StartCoroutine(BounceUpDown());
        }
        int i = 0;

        // Type out the message
        foreach (char c in message)
        {
            dialogText.text += c;
            // Play typing sound for each character
            if (SoundManager.Instance != null && typingSound != null && i == 3)
            {
                i = 0;
                SoundManager.Instance.PlaySound1(typingSound, 0.5f); // Adjust volume as needed
            }
            i++;
            yield return new WaitForSecondsRealtime(typingSpeed); // Use unscaled time for typing speed
        }

        // Wait for the specified duration after typing
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // End the dialog
        dialogPanel.SetActive(false);
        if (currentBounceCoroutine != null)
        {
            StopCoroutine(currentBounceCoroutine);
            currentBounceCoroutine = null;
            characterImage.transform.localPosition = startImagePos;
        }

        // Resume the game
        //Time.timeScale = previousTimeScale;
        currentDialogCoroutine = null;
    }

    private IEnumerator DisplayDialogWithInput(string message, Sprite characterSprite)
    {
        // Pause the game
        //previousTimeScale = Time.timeScale;
        //Time.timeScale = 0f;

        dialogText.text = "";
        if (characterSprite != null)
        {
            characterImage.sprite = characterSprite;
            characterImage.enabled = true;
        }
        else
        {
            characterImage.enabled = false;
        }

        dialogPanel.SetActive(true);

        if (characterImage.enabled && currentBounceCoroutine == null)
        {
            currentBounceCoroutine = StartCoroutine(BounceUpDown());
        }

        // Type out the message
        foreach (char c in message)
        {
            dialogText.text += c;
            if (SoundManager.Instance != null && typingSound != null)
            {
                SoundManager.Instance.PlaySound(typingSound, 0.5f);
            }
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        // Wait for the player to press the E key to dismiss
        while (!Input.GetKeyDown(KeyCode.E))
        {
            yield return null;
        }

        dialogPanel.SetActive(false);
        if (currentBounceCoroutine != null)
        {
            StopCoroutine(currentBounceCoroutine);
            currentBounceCoroutine = null;
            characterImage.transform.localPosition = startImagePos;
        }

        // Resume the game
        //Time.timeScale = previousTimeScale;
        currentDialogCoroutine = null;
    }

    private IEnumerator DisplayDialogSequence(List<DialogLine> dialogLines, bool waitForInput)
    {
        // Pause the game
        //previousTimeScale = Time.timeScale;
        //Time.timeScale = 0f;

        foreach (DialogLine line in dialogLines)
        {
            dialogText.text = "";
            if (line.speakerSprite != null)
            {
                characterImage.sprite = line.speakerSprite;
                characterImage.enabled = true;
            }
            else
            {
                characterImage.enabled = false;
            }

            dialogPanel.SetActive(true);

            if (characterImage.enabled && currentBounceCoroutine == null)
            {
                currentBounceCoroutine = StartCoroutine(BounceUpDown());
            }

            // Type out the message
            foreach (char c in line.message)
            {
                dialogText.text += c;
                if (SoundManager.Instance != null && typingSound != null)
                {
                    SoundManager.Instance.PlaySound(typingSound, 0.5f);
                }
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            if (waitForInput)
            {
                while (!Input.GetKeyDown(KeyCode.E))
                {
                    yield return null;
                }
            }
            else
            {
                float elapsed = 0f;
                while (elapsed < defaultDisplayDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            dialogPanel.SetActive(false);
            if (currentBounceCoroutine != null)
            {
                StopCoroutine(currentBounceCoroutine);
                currentBounceCoroutine = null;
                characterImage.transform.localPosition = startImagePos;
            }

            // Small delay between messages
            yield return new WaitForSecondsRealtime(0.2f);
        }

        // Resume the game
        //Time.timeScale = previousTimeScale;
        currentDialogCoroutine = null;
    }

    private IEnumerator BounceUpDown()
    {
        while (true)
        {
            Vector3 startPos = characterImage.transform.localPosition;
            float bounceHeight = 20f;
            float duration = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float newY = startPos.y + Mathf.Sin(elapsedTime / duration * Mathf.PI) * bounceHeight;
                characterImage.transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
                elapsedTime += Time.unscaledDeltaTime; // Use unscaled time for animation
                yield return null;
            }

            characterImage.transform.localPosition = startPos;
        }
    }

    public bool IsDialogActive()
    {
        return dialogPanel != null && dialogPanel.activeInHierarchy;
    }
}