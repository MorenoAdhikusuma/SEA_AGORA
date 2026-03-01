using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    public static Audio_Manager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;

    [Header("Volume")]
[Range(0f, 1f)]
[SerializeField] private float musicVolume = 0.5f;

    [Header("Audio Clips")]
    public AudioClip music;
    public AudioClip jump;
    public AudioClip walk;
    public AudioClip sword;
    public AudioClip collect;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
{
    // Set volume (0 to 1)
     musicSource.volume = musicVolume;

    // Play music
     PlayMusic();
}

    // =========================
    // PLAY SFX
    // =========================
    public void PlayMusic()
    {
        PlayMusic(music);
    }

    void OnValidate()
{
    if (musicSource != null)
        musicSource.volume = musicVolume;
}
    // =========================
    // PLAY ONE SHOT SFX
    // =========================

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }


    // =========================
    // PLAY LOOP (walking, etc)
    // =========================

    public void PlayLoop(AudioClip clip)
    {
        if (clip == null) return;

        if (loopSource.clip == clip && loopSource.isPlaying)
            return;

        loopSource.clip = clip;
        loopSource.loop = true;
        loopSource.Play();
    }


    public void StopLoop()
    {
        loopSource.Stop();
    }


    // =========================
    // MUSIC
    // =========================

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }


    public void StopMusic()
    {
        musicSource.Stop();
    }
}