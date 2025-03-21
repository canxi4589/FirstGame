using UnityEngine;
using System.Collections;
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource[] effectSources;
    private int currentSourceIndex = 0;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip coinCollectSound;
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip enemyDeathSound;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip mainScreen;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip selectMenuSound;
    [SerializeField] private AudioClip fallSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip spikeTrapArmedSound; // New sound for spike trap arming
    [SerializeField] private AudioClip spikeTrapShootSound; // New sound for spike trap shooting

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float effectsVolume = 0.8f;
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.6f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySound(AudioClip clip, float volume = -1f)
    {
        if (clip == null) return;

        AudioSource source = effectSources[currentSourceIndex];
        currentSourceIndex = (currentSourceIndex + 1) % effectSources.Length;

        source.clip = clip;
        source.volume = (volume >= 0f) ? volume : effectsVolume;
        source.Play();

        if (!source.loop)
        {
            StartCoroutine(StopSourceAfterDelay(source, clip.length));
        }
    }

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
    public void PlayDeathSound() => PlaySound(deathSound);
    public void PlaySpikeTrapArmedSound() => PlaySound(spikeTrapArmedSound); // New method for armed sound
    public void PlaySpikeTrapShootSound() => PlaySound(spikeTrapShootSound); // New method for shoot sound

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

