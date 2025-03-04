using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // Change "GameScene" to your actual game scene name
    }

    public void OpenOptions()
    {
        Debug.Log("Options Menu Opened"); // Replace with actual options menu logic
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
