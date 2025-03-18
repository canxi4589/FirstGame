using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } // Singleton instance

    [SerializeField] private AudioSource musicSource;         // Dedicated source for background music
    [SerializeField] private AudioSource[] effectSources;     // Pool of AudioSources for sound effects
    private int currentSourceIndex = 0;                       // Index to cycle through effect sources

    [Header("Audio Clips")]
    [SerializeField] private AudioClip shootSound;            // Sound for player shooting
    [SerializeField] private AudioClip coinCollectSound;      // Sound for collecting coins
    [SerializeField] private AudioClip enemyHitSound;         // Sound for enemy taking damage
    [SerializeField] private AudioClip enemyDeathSound;       // Sound for enemy death
    [SerializeField] private AudioClip backgroundMusic;       // Background music clip
    [SerializeField] private AudioClip footstepSound;         // Sound for footsteps
    [SerializeField] private AudioClip mainScreen;            // Sound for main screen
    [SerializeField] private AudioClip dashSound;             // Sound for dashing
    [SerializeField] private AudioClip selectMenuSound;       // Sound for menu selection
    [SerializeField] private AudioClip fallSound;             // Sound for falling into a hole
    [SerializeField] private AudioClip hurtSound;             // Sound for taking damage
    [SerializeField] private AudioClip deathSound;            // Sound for player death (new)

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;        // Music volume (0 to 1)
    [Range(0f, 1f)]
    [SerializeField] private float effectsVolume = 0.8f;      // Effects volume (0 to 1)
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.6f;     // Footstep volume (0 to 1)

    void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes

        // Initialize music source
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // Play a sound effect
    public void PlaySound(AudioClip clip, float volume = -1f)
    {
        if (clip == null) return;

        // Cycle through available effect sources
        AudioSource source = effectSources[currentSourceIndex];
        currentSourceIndex = (currentSourceIndex + 1) % effectSources.Length;

        source.clip = clip;
        source.volume = (volume >= 0f) ? volume : effectsVolume; // Use provided volume or default effects volume
        source.Play();

        // Optional: Stop the source after the clip finishes
        if (!source.loop)
        {
            StartCoroutine(StopSourceAfterDelay(source, clip.length));
        }
    }

    // Play specific predefined sounds
    public void PlayShootSound() => PlaySound(shootSound);
    public void PlayCoinCollectSound() => PlaySound(coinCollectSound);
    public void PlayEnemyHitSound() => PlaySound(enemyHitSound);
    public void PlayEnemyDeathSound() => PlaySound(enemyDeathSound);
    public void PlayFootstepSound() => PlaySound(footstepSound, footstepVolume);
    public void PlayMainScreenSound() => PlaySound(mainScreen);
    public void PlayDashSound() => PlaySound(dashSound);
    public void PlaySelectMenuSound() => PlaySound(selectMenuSound);
    public void PlayFallSound() => PlaySound(fallSound);
    public void PlayHurtSound() => PlaySound(hurtSound);
    public void PlayDeathSound() => PlaySound(deathSound); // New method for death sound

    // Adjust volumes
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
        foreach (AudioSource source in effectSources)
        {
            if (source.isPlaying) source.volume = effectsVolume;
        }
    }

    public void SetFootstepVolume(float volume)
    {
        footstepVolume = Mathf.Clamp01(volume);
    }

    private System.Collections.IEnumerator StopSourceAfterDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        source.clip = null;
    }
}