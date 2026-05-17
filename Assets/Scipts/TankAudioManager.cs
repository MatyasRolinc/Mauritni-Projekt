using UnityEngine;
using UnityEngine.SceneManagement;

public class TankAudioManager : MonoBehaviour
{
    public static TankAudioManager Instance { get; private set; }

    [Header("Zvuky")]
    public AudioClip cannonFireClip;
    public AudioClip tracksRollingClip;
    public AudioClip armorHitClip;

    [Header("Hlasitost")]
    [Range(0f, 1f)] public float cannonVolume = 0.6f;
    [Range(0f, 1f)] public float tracksVolume = 1f;
    [Range(0f, 1f)] public float armorHitVolume = 1f;

    private const float TRACKS_START = 3f;
    private const float TRACKS_END   = 8f;

    private AudioSource cannonSource;
    private AudioSource tracksSource;
    private AudioSource armorHitSource;

    // Scény kde mají zvuky tanku hrát
    private static readonly string[] GAME_SCENES = { "Level1", "Level2", "Level3", "Level4", "Level5" };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        cannonSource   = gameObject.AddComponent<AudioSource>();
        tracksSource   = gameObject.AddComponent<AudioSource>();
        armorHitSource = gameObject.AddComponent<AudioSource>();

        tracksSource.clip = tracksRollingClip;
        tracksSource.loop = false;
        tracksSource.volume = tracksVolume;
        tracksSource.playOnAwake = false;

        cannonSource.playOnAwake = false;
        cannonSource.volume = cannonVolume;

        armorHitSource.playOnAwake = false;
        armorHitSource.volume = armorHitVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameScene = System.Array.Exists(GAME_SCENES, s => s == scene.name);

        if (!isGameScene)
        {
            StopAllSounds();
        }
    }

    void Update()
    {
        if (tracksSource != null && tracksSource.isPlaying)
        {
            if (tracksSource.time >= TRACKS_END)
                tracksSource.time = TRACKS_START;
        }
    }

    public void StopAllSounds()
    {
        if (tracksSource != null && tracksSource.isPlaying)
            tracksSource.Stop();
        if (cannonSource != null)
            cannonSource.Stop();
        if (armorHitSource != null)
            armorHitSource.Stop();
    }

    public void PlayCannonFire()
    {
        if (cannonFireClip == null) return;
        cannonSource.PlayOneShot(cannonFireClip, cannonVolume);
    }

    public void StartTracksSound()
    {
        if (tracksRollingClip == null || tracksSource.isPlaying) return;
        tracksSource.clip = tracksRollingClip;
        tracksSource.time = TRACKS_START;
        tracksSource.Play();
    }

    public void StopTracksSound()
    {
        if (tracksSource.isPlaying)
            tracksSource.Stop();
    }

    public void PlayArmorHit()
    {
        if (armorHitClip == null) return;
        armorHitSource.PlayOneShot(armorHitClip, armorHitVolume);
    }
}
