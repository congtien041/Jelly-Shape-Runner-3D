using UnityEngine;

/// <summary>
/// Quản lý âm thanh toàn bộ game. Singleton pattern.
/// Hai AudioSource: BGM (loop) và SFX (one-shot).
/// Lưu trạng thái Sound On/Off qua PlayerPrefs.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Jelly Runner/Audio Manager")]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("--- Audio Sources ---")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("--- Nhạc Nền ---")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("--- Hiệu Ứng Âm Thanh ---")]
    [SerializeField] private AudioClip shapeShiftSound;
    [SerializeField] private AudioClip passWallSound;
    [SerializeField] private AudioClip coinCollectSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip buttonClickSound;

    [Header("--- Cài Đặt ---")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1.0f;

    private const string SOUND_ON_KEY = "SoundOn";

    public bool IsSoundOn { get; private set; } = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Tự tạo AudioSources nếu chưa gán
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }

        // Load trạng thái Sound
        IsSoundOn = PlayerPrefs.GetInt(SOUND_ON_KEY, 1) == 1;
        ApplySoundState();
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    // === Phát nhạc nền ===

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null || bgmSource == null) return;

        bgmSource.clip = backgroundMusic;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;

        if (IsSoundOn)
            bgmSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    // === Phát hiệu ứng âm thanh cụ thể ===

    public void PlayShapeShiftSound()
    {
        PlaySFX(shapeShiftSound);
    }

    public void PlayPassWallSound()
    {
        PlaySFX(passWallSound);
    }

    public void PlayCoinCollectSound()
    {
        PlaySFX(coinCollectSound);
    }

    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound);
    }

    public void PlayObstacleHitSound()
    {
        PlaySFX(gameOverSound);
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(buttonClickSound);
    }

    /// <summary>
    /// Phát một clip SFX bất kỳ.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (!IsSoundOn || clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // === Bật/Tắt âm thanh ===

    /// <summary>
    /// Toggle bật/tắt toàn bộ âm thanh.
    /// </summary>
    public void ToggleSound()
    {
        SetSound(!IsSoundOn);
    }

    /// <summary>
    /// Đặt trạng thái âm thanh cụ thể.
    /// </summary>
    public void SetSound(bool on)
    {
        IsSoundOn = on;
        PlayerPrefs.SetInt(SOUND_ON_KEY, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplySoundState();
    }

    private void ApplySoundState()
    {
        if (bgmSource != null)
        {
            if (IsSoundOn)
            {
                if (!bgmSource.isPlaying && bgmSource.clip != null)
                    bgmSource.Play();
            }
            else
            {
                bgmSource.Pause();
            }
        }

        AudioListener.volume = IsSoundOn ? 1f : 0f;
    }

    // === Điều chỉnh âm lượng ===

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}
