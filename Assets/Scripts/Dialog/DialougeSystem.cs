using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public DialougeObject dialogueData;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerText;
    public GameObject dialogueBox;
    public Image characterImage;
    public float typingSpeed = 0.05f;

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private Dictionary<string, Color> speakerColors = new Dictionary<string, Color>()
    {
        { "Người dẫn truyện", Color.gray },
        { "Lilia", new Color(1.0f, 0.4f, 0.7f) },
        { "Hoàng tử", new Color(1.0f, 0.6f, 0.0f) },
        { "Tay Trái", Color.green },
        { "Tay Phải", Color.blue },
        { "Chân Trái", Color.yellow },
        { "Chân Phải", Color.magenta },
        { "Zeros", Color.red },
        { "Kẻ Buôn Bán", Color.black }
    };

    void Start()
    {
        if (dialogueData != null && dialogueData.dialogues.Length > 0)
        {
            dialogueBox.SetActive(true);
            StartCoroutine(TypeDialogue());
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = dialogueData.dialogues[currentIndex].text;
                isTyping = false;
            }
            else
            {
                NextDialogue();
            }
        }
    }

    IEnumerator TypeDialogue()
    {
        isTyping = true;
        string speaker = dialogueData.dialogues[currentIndex].speaker;
        speakerText.text = speaker;
        dialogueText.text = "";

        if (speakerColors.ContainsKey(speaker))
        {
            speakerText.color = speakerColors[speaker];
        }
        else
        {
            speakerText.color = Color.white;
        }

        UpdateCharacterImage(speaker);

        typingCoroutine = StartCoroutine(TypingEffect());
        yield return typingCoroutine;
        isTyping = false;
    }

    void UpdateCharacterImage(string speaker)
    {
        if (speaker == "Người dẫn truyện" || speaker == "Narrator")
        {
            characterImage.gameObject.SetActive(false);
        }
        else
        {
            Sprite characterSprite = Resources.Load<Sprite>("CharacterSprites/" + speaker);
            if (characterSprite != null)
            {
                characterImage.sprite = characterSprite;
                characterImage.gameObject.SetActive(true);
                StartCoroutine(BounceUpDown());
            }
            else
            {
                characterImage.gameObject.SetActive(false);
            }
        }
    }
    IEnumerator BounceUpDown()
    {
        Vector3 startPos = characterImage.transform.localPosition;
        float bounceHeight = 20f; 
        float duration = 0.3f;  
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float newY = startPos.y + Mathf.Sin(elapsedTime / duration * Mathf.PI) * bounceHeight;
            characterImage.transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        characterImage.transform.localPosition = startPos; 
    }

    IEnumerator TypingEffect()
    {
        foreach (char letter in dialogueData.dialogues[currentIndex].text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void NextDialogue()
    {
        currentIndex++;
        if (currentIndex < dialogueData.dialogues.Length)
        {
            StartCoroutine(TypeDialogue());
        }
        else
        {
            StartCoroutine(EndDialogueAndLoadScene());
        }
    }

    IEnumerator EndDialogueAndLoadScene()
    {
        SceneFader sceneFader = FindObjectOfType<SceneFader>(); // Find the SceneFader in the scene
        if (sceneFader != null)
        {
            sceneFader.FadeToScene("SampleScene"); // Change "NextScene" to your actual scene name
        }
        else
        {
            Debug.LogError("SceneFader not found in the scene!");
        }

        yield return new WaitForSeconds(1f);
        dialogueBox.SetActive(false);
    }


    void SetDialogueBoxTransparency(float alpha)
    {
        CanvasGroup canvasGroup = dialogueBox.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = dialogueBox.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = alpha;
    }
}
