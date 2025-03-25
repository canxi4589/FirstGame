using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image characterImage;
    [SerializeField] private float defaultDisplayDuration = 3f;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioSource audioSource; // For TTS playback

    [SerializeField] private string openAIKey = ""; // Set in Inspector
    private const string whisperEndpoint = "https://api.openai.com/v1/audio/transcriptions";
    private const string chatEndpoint = "https://api.openai.com/v1/chat/completions";
    private const string ttsEndpoint = "https://api.openai.com/v1/audio/speech";
    private AudioClip recordedClip;
    private bool isRecording = false;
    private string transcriptionResult;
    private string chatResponseResult;

    private const float MAX_DAILY_COST = 2.00f;
    private const float COST_PER_CHAT = 0.015f; // Adjusted for TTS (~$0.015/1k chars)
    private const int MAX_CHATS_PER_DAY = (int)(MAX_DAILY_COST / COST_PER_CHAT);
    private int chatsToday = 0;
    private DateTime lastResetDate = DateTime.MinValue;

    private Coroutine currentDialogCoroutine;
    private Coroutine currentBounceCoroutine;
    private Vector3 startImagePos;

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

        if (dialogPanel != null) dialogPanel.SetActive(false);
        else Debug.LogError("DialogManager: DialogPanel is not assigned!");
        if (dialogText == null) Debug.LogError("DialogManager: DialogText is not assigned!");
        if (characterImage == null) Debug.LogError("DialogManager: CharacterImage is not assigned!");
        else startImagePos = characterImage.transform.localPosition;

        lastResetDate = DateTime.Today;
    }

    void Update()
    {
        if (DateTime.Today != lastResetDate)
        {
            chatsToday = 0;
            lastResetDate = DateTime.Today;
            Debug.Log("Daily chat counter reset.");
        }
    }

    // New method for voice chat with Harry Pooper
    public void StartVoiceChatWithHarryPooper(Sprite harrySprite = null)
    {
        if (string.IsNullOrEmpty(openAIKey))
        {
            Debug.LogError("OpenAI API Key is empty! Please set it in the Inspector.");
            return;
        }

        if (chatsToday >= MAX_CHATS_PER_DAY)
        {
            if (currentDialogCoroutine != null) StopCoroutine(currentDialogCoroutine);
            currentDialogCoroutine = StartCoroutine(DisplayLimitReachedMessage(harrySprite));
            return;
        }

        if (currentDialogCoroutine != null) StopCoroutine(currentDialogCoroutine);
        currentDialogCoroutine = StartCoroutine(VoiceChatCoroutine(harrySprite));
    }


    private IEnumerator DisplayLimitReachedMessage(Sprite characterSprite)
    {
        dialogText.text = "Oi, that’s enough chatter for today! Ministry’s cuttin’ me budget.";
        dialogPanel.SetActive(true);
        if (characterSprite != null)
        {
            characterImage.sprite = characterSprite;
            characterImage.enabled = true;
            if (currentBounceCoroutine == null) currentBounceCoroutine = StartCoroutine(BounceUpDown());
        }

        yield return new WaitForSecondsRealtime(defaultDisplayDuration);
        EndDialog();
    }
    private IEnumerator VoiceChatCoroutine(Sprite characterSprite)
    {
        isRecording = true;
        recordedClip = Microphone.Start(null, false, 10, 44100);
        dialogText.text = "Oi, speak up, mate! Prison’s noisy.";
        dialogPanel.SetActive(true);
        if (characterSprite != null)
        {
            characterImage.sprite = characterSprite;
            characterImage.enabled = true;
            if (currentBounceCoroutine == null) currentBounceCoroutine = StartCoroutine(BounceUpDown());
        }

        yield return new WaitForSecondsRealtime(5f);

        Microphone.End(null);
        isRecording = false;

        string filePath = Path.Combine(Application.persistentDataPath, "recording.wav");
        if (SavWav.Save(filePath, recordedClip))
        {
            yield return StartCoroutine(TranscribeAudio(filePath));
            string transcribedText = transcriptionResult;

            if (string.IsNullOrEmpty(transcribedText))
            {
                dialogText.text = "What? Speak proper, you muggle!";
                yield return new WaitForSecondsRealtime(defaultDisplayDuration);
                EndDialog();
                yield break;
            }

            LogAPIUsage("Whisper", "Transcription", transcribedText);

            yield return StartCoroutine(GetChatResponse(transcribedText));
            string response = chatResponseResult;

            if (!string.IsNullOrEmpty(response))
            {
                LogAPIUsage("OpenAI Chat", "Response", response);

                // Generate and play speech
                AudioClip speechClip = null;
                yield return StartCoroutine(GenerateSpeech(response, clip => speechClip = clip));
                if (speechClip != null)
                {
                    audioSource.PlayOneShot(speechClip);
                    yield return StartCoroutine(DisplayDialogWithTyping(response, characterSprite, speechClip.length));
                }
                else
                {
                    yield return StartCoroutine(DisplayDialogWithTyping(response, characterSprite, defaultDisplayDuration));
                }
                chatsToday++;
            }
            else
            {
                dialogText.text = "Blimey, the magic’s gone wonky again.";
                yield return new WaitForSecondsRealtime(defaultDisplayDuration);
            }
        }
        else
        {
            dialogText.text = "Can’t even record a spell right in here.";
            yield return new WaitForSecondsRealtime(defaultDisplayDuration);
        }

        EndDialog();
    }
    private void EndDialog()
    {
        dialogPanel.SetActive(false);
        if (currentBounceCoroutine != null)
        {
            StopCoroutine(currentBounceCoroutine);
            currentBounceCoroutine = null;
            characterImage.transform.localPosition = startImagePos;
        }
        currentDialogCoroutine = null;
    }

    private IEnumerator TranscribeAudio(string filePath)
    {
        Debug.Log($"TranscribeAudio - API Key: '{openAIKey}'");
        byte[] audioData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "recording.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        using (UnityWebRequest www = UnityWebRequest.Post(whisperEndpoint, form))
        {
            www.SetRequestHeader("Authorization", $"Bearer {openAIKey}");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                var json = JsonUtility.FromJson<WhisperResponse>(jsonResponse);
                transcriptionResult = json.text;
            }
            else
            {
                Debug.LogError($"Whisper API Error: {www.error} - Response Code: {www.responseCode}");
                transcriptionResult = null;
            }
        }
    }


    private IEnumerator GetChatResponse(string inputText)
    {
        Debug.Log($"GetChatResponse - API Key: '{openAIKey}'");
        string systemPrompt = "You are Harry Pooper, a wizard imprisoned for illegal broom racing. " +
                              "You’re a bit cheeky, sarcastic, and bitter about being locked up. " +
                              "You miss flying and often reference brooms, magic, or prison life in your responses. " +
                              "Keep your tone casual and a bit grumpy, with a hint of wizardly flair. " +
                              "Respond in English.";

        string jsonBody = JsonUtility.ToJson(new ChatRequest
        {
            model = "gpt-3.5-turbo",
            messages = new List<ChatMessage>
            {
                new ChatMessage { role = "system", content = systemPrompt },
                new ChatMessage { role = "user", content = inputText }
            }
        });

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(chatEndpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {openAIKey}");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                var response = JsonUtility.FromJson<ChatResponse>(jsonResponse);
                chatResponseResult = response.choices[0].message.content;
            }
            else
            {
                Debug.LogError($"Chat API Error: {www.error} - Response Code: {www.responseCode}");
                chatResponseResult = null;
            }
        }
    }

    private IEnumerator GenerateSpeech(string text, Action<AudioClip> onComplete)
    {
        Debug.Log($"GenerateSpeech - API Key: '{openAIKey}'");
        string jsonBody = JsonUtility.ToJson(new TTSRequest
        {
            model = "tts-1",
            input = text,
            voice = "onyx" // Grumpy voice
        });

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(ttsEndpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerAudioClip(ttsEndpoint, AudioType.MPEG);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {openAIKey}");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = ((DownloadHandlerAudioClip)www.downloadHandler).audioClip;
                onComplete(clip);
            }
            else
            {
                Debug.LogError($"TTS API Error: {www.error} - Response Code: {www.responseCode}");
                onComplete(null);
            }
        }
    }

    private void LogAPIUsage(string apiName, string action, string details)
    {
        string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {apiName} | {action} | {details} | Chats Today: {chatsToday}/{MAX_CHATS_PER_DAY}";
        Debug.Log(log);
        File.AppendAllText(Path.Combine(Application.persistentDataPath, "api_usage.log"), log + "\n");
    }

    // Original methods retained below
    public void ShowDialog(string message, Sprite characterSprite = null, float duration = -1f)
    {
        if (currentDialogCoroutine != null) StopCoroutine(currentDialogCoroutine);
        currentDialogCoroutine = StartCoroutine(DisplayDialogWithTyping(message, characterSprite, duration >= 0f ? duration : defaultDisplayDuration));
    }

    public void ShowDialogWithInput(string message, Sprite characterSprite = null)
    {
        if (currentDialogCoroutine != null) StopCoroutine(currentDialogCoroutine);
        currentDialogCoroutine = StartCoroutine(DisplayDialogWithInput(message, characterSprite));
    }

    public void ShowDialogSequence(List<DialogLine> dialogLines, bool waitForInput = false)
    {
        if (currentDialogCoroutine != null) StopCoroutine(currentDialogCoroutine);
        currentDialogCoroutine = StartCoroutine(DisplayDialogSequence(dialogLines, waitForInput));
    }

    private IEnumerator DisplayDialogWithTyping(string message, Sprite characterSprite, float duration)
    {
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

        foreach (char c in message)
        {
            dialogText.text += c;
            if (SoundManager.Instance != null && typingSound != null)
            {
            }
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        yield return new WaitForSecondsRealtime(duration);
        dialogPanel.SetActive(false);
        if (currentBounceCoroutine != null)
        {
            StopCoroutine(currentBounceCoroutine);
            currentBounceCoroutine = null;
            characterImage.transform.localPosition = startImagePos;
        }
        currentDialogCoroutine = null;
    }

    private IEnumerator DisplayDialogWithInput(string message, Sprite characterSprite)
    {
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

        foreach (char c in message)
        {
            dialogText.text += c;
            if (SoundManager.Instance != null && typingSound != null)
            {
                SoundManager.Instance.PlaySound(typingSound, 0.5f);
            }
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

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
        currentDialogCoroutine = null;
    }

    private IEnumerator DisplayDialogSequence(List<DialogLine> dialogLines, bool waitForInput)
    {
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

            yield return new WaitForSecondsRealtime(0.2f);
        }
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
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            characterImage.transform.localPosition = startPos;
        }
    }

    public bool IsDialogActive()
    {
        return dialogPanel != null && dialogPanel.activeInHierarchy;
    }

    [System.Serializable] private class WhisperResponse { public string text; }
    [System.Serializable] private class ChatRequest { public string model; public List<ChatMessage> messages; }
    [System.Serializable] private class ChatMessage { public string role; public string content; }
    [System.Serializable] private class ChatResponse { public List<Choice> choices; }
    [System.Serializable] private class Choice { public ChatMessage message; }
    [System.Serializable] private class TTSRequest { public string model; public string input; public string voice; }


}