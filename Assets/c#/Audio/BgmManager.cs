using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BgmManager : MonoBehaviour
{
    public static BgmManager Instance { get; private set; }

    [Header("Playback")]
    [SerializeField] private AudioClip defaultBgm;
    [SerializeField] private AudioClip dayBgm;
    [SerializeField] private AudioClip nightBgm;
    [SerializeField, Range(0f, 1f)] private float volume = 0.45f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool persistAcrossScenes = true;

    [Header("Day Phase")]
    [SerializeField] private DaySystem daySystem;
    [SerializeField] private bool followDayPhase = true;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.75f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;
    private bool subscribedToDaySystem;

    public AudioClip CurrentClip => audioSource != null ? audioSource.clip : null;
    public float Volume => volume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        ResolveAudioSource();
    }

    private void OnEnable()
    {
        SubscribeToDaySystem();
    }

    private void Start()
    {
        if (!playOnStart)
        {
            return;
        }

        if (followDayPhase && TryPlayForCurrentDayPhase())
        {
            return;
        }

        if (defaultBgm != null)
        {
            Play(defaultBgm, fadeInDuration);
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromDaySystem();
    }

    private void OnValidate()
    {
        volume = Mathf.Clamp01(volume);
        fadeInDuration = Mathf.Max(0f, fadeInDuration);
        fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
    }

    public void PlayDefault()
    {
        Play(defaultBgm, fadeInDuration);
    }

    public void PlayForPhase(DayPhase phase)
    {
        AudioClip clip = GetClipForPhase(phase);

        if (clip != null)
        {
            Play(clip, fadeInDuration);
        }
    }

    public void Play(AudioClip clip)
    {
        Play(clip, fadeInDuration);
    }

    public void Play(AudioClip clip, float fadeDuration)
    {
        if (clip == null)
        {
            return;
        }

        ResolveAudioSource();

        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            StartFade(volume, fadeDuration);
            return;
        }

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.volume = fadeDuration > 0f ? 0f : volume;
        audioSource.Play();
        StartFade(volume, fadeDuration);
    }

    public void Stop()
    {
        Stop(fadeOutDuration);
    }

    public void Stop(float fadeDuration)
    {
        ResolveAudioSource();

        if (!audioSource.isPlaying)
        {
            return;
        }

        if (fadeDuration <= 0f)
        {
            audioSource.Stop();
            audioSource.clip = null;
            return;
        }

        StartFade(0f, fadeDuration, stopWhenDone: true);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    private void ResolveAudioSource()
    {
        if (audioSource != null)
        {
            return;
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
    }

    private void SubscribeToDaySystem()
    {
        if (!followDayPhase || subscribedToDaySystem)
        {
            return;
        }

        ResolveDaySystem();

        if (daySystem == null)
        {
            return;
        }

        daySystem.DayPhaseChanged += HandleDayPhaseChanged;
        subscribedToDaySystem = true;
    }

    private void UnsubscribeFromDaySystem()
    {
        if (!subscribedToDaySystem || daySystem == null)
        {
            subscribedToDaySystem = false;
            return;
        }

        daySystem.DayPhaseChanged -= HandleDayPhaseChanged;
        subscribedToDaySystem = false;
    }

    private void ResolveDaySystem()
    {
        if (daySystem == null)
        {
            daySystem = FindObjectOfType<DaySystem>();
        }
    }

    private bool TryPlayForCurrentDayPhase()
    {
        ResolveDaySystem();

        if (daySystem == null)
        {
            return false;
        }

        AudioClip clip = GetClipForPhase(daySystem.CurrentPhase);

        if (clip == null)
        {
            return false;
        }

        Play(clip, fadeInDuration);
        return true;
    }

    private AudioClip GetClipForPhase(DayPhase phase)
    {
        switch (phase)
        {
            case DayPhase.Day:
                return dayBgm != null ? dayBgm : defaultBgm;
            case DayPhase.Night:
                return nightBgm != null ? nightBgm : defaultBgm;
            default:
                return defaultBgm;
        }
    }

    private void HandleDayPhaseChanged(DayPhase phase)
    {
        PlayForPhase(phase);
    }

    private void StartFade(float targetVolume, float duration, bool stopWhenDone = false)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeTo(targetVolume, duration, stopWhenDone));
    }

    private IEnumerator FadeTo(float targetVolume, float duration, bool stopWhenDone)
    {
        if (duration <= 0f)
        {
            audioSource.volume = targetVolume;
        }
        else
        {
            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            audioSource.volume = targetVolume;
        }

        if (stopWhenDone)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        fadeCoroutine = null;
    }
}
