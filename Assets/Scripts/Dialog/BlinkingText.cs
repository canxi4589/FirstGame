using System.Collections;
using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public float blinkSpeed = 1f; // Speed of blinking

    private void Start()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();

        StartCoroutine(BlinkText());
    }

    private IEnumerator BlinkText()
    {
        while (true)
        {
            textMeshPro.alpha = 1; // Visible
            yield return new WaitForSeconds(1f / blinkSpeed);

            textMeshPro.alpha = 0; // Invisible
            yield return new WaitForSeconds(1f / blinkSpeed);
        }
    }
}
