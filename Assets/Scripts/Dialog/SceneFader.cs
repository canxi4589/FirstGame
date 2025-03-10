using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage; // Assign FadePanel's Image in the Inspector
    public float fadeDuration = 1f; // How long fade takes

    private void Start()
    {
        StartCoroutine(FadeIn()); // Fade in when scene starts
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration); // Fade from black to clear
            fadeImage.color = color;
            yield return null;
        }
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration); // Fade from clear to black
            fadeImage.color = color;
            yield return null;
        }

        SceneManager.LoadScene(sceneName); // Load next scene
    }
}
