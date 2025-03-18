using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float time = 0f;  // Time in seconds
    private int money = 0;    // Coins collected
    private int kills = 0;    // Enemies killed

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes (for hub)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Time.timeScale > 0) // Only count time when the game isn't paused
        {
            time += Time.deltaTime;
        }
    }

    // Methods to update stats
    public void AddMoney(int amount) => money += amount;
    public void AddKill() => kills++;

    // Getters for stats
    public string GetTimeFormatted()
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }
    public int GetMoney() => money;
    public int GetKills() => kills;

    // Reset stats for a new run
    public void ResetStats()
    {
        time = 0f;
        money = 0;
        kills = 0;
    }
}