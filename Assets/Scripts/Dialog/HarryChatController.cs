using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HarryChatController : MonoBehaviour
{
    [SerializeField] private Button talkButton; // Optional: Assign this in the Inspector if you want a button
    [SerializeField] private Sprite harrySprite; // Harry Pooper’s sprite, assign in Inspector

    private bool isChatting = false; // Prevent overlapping chats

    void Start()
    {
        if (talkButton != null)
        {
            talkButton.onClick.AddListener(StartVoiceChat);
        }
        else
        {
            Debug.LogWarning("Talk button not assigned; using Space key only.");
        }

        // Load sprite dynamically if not assigned
        if (harrySprite == null)
        {
            harrySprite = Resources.Load<Sprite>("HarryPooperSprite");
            if (harrySprite == null) Debug.LogError("HarryPooperSprite not found in Resources!");
        }
    }

    void Update()
    {
        // Trigger chat with Space key
        if (Input.GetKeyDown(KeyCode.Space) && !isChatting)
        {
            StartVoiceChat();
        }
    }

    void StartVoiceChat()
    {
        if (DialogManager.Instance != null && !isChatting)
        {
            isChatting = true;
            if (talkButton != null) talkButton.interactable = false; // Disable button if present
            DialogManager.Instance.StartVoiceChatWithHarryPooper(harrySprite);
            StartCoroutine(ReEnableChat());
        }
        else if (DialogManager.Instance == null)
        {
            Debug.LogError("DialogManager instance not found!");
        }
    }

    private IEnumerator ReEnableChat()
    {
        yield return new WaitForSecondsRealtime(6f); // Wait for recording (5s) + buffer
        isChatting = false;
        if (talkButton != null) talkButton.interactable = true; // Re-enable button if present
    }

    void OnDestroy()
    {
        if (talkButton != null)
        {
            talkButton.onClick.RemoveListener(StartVoiceChat);
        }
    }
}