using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DayNightFadeTransition : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Image fadeImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool startBlack = false;

    [Header("Fade Events")]
    [SerializeField] private UnityEvent onFadeInComplete;
    [SerializeField] private UnityEvent onFadeOutComplete;
    [SerializeField] private UnityEvent onFadeInThenOutComplete;

    private Coroutine fadeCoroutine;

    public bool IsFading => fadeCoroutine != null;
    public UnityEvent OnFadeInComplete => onFadeInComplete;
    public UnityEvent OnFadeOutComplete => onFadeOutComplete;
    public UnityEvent OnFadeInThenOutComplete => onFadeInThenOutComplete;

    private void Awake()
    {
        ResolveReferences();
        SetAlpha(startBlack ? 1f : 0f);
        SetCanvasBlocking(startBlack);
    }

    private void OnDisable()
    {
        StopFade();
    }

    public void FadeIn()
    {
        StartFade(1f, fadeInDuration, true, onFadeInComplete);
    }

    public void FadeOut()
    {
        StartFade(0f, fadeOutDuration, false, onFadeOutComplete);
    }

    public void FadeInThenOut()
    {
        StopFade();

        if (!gameObject.activeInHierarchy)
        {
            SetAlpha(0f);
            SetCanvasBlocking(false);
            onFadeInThenOutComplete?.Invoke();
            return;
        }

        fadeCoroutine = StartCoroutine(FadeInThenOutRoutine());
    }

    public void SetBlackInstant()
    {
        StopFade();
        SetAlpha(1f);
        SetCanvasBlocking(true);
    }

    public void SetClearInstant()
    {
        StopFade();
        SetAlpha(0f);
        SetCanvasBlocking(false);
    }

    private void ResolveReferences()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }
    }

    private void StartFade(float targetAlpha, float duration, bool blockAfterFade, UnityEvent completedEvent)
    {
        StopFade();

        if (!gameObject.activeInHierarchy)
        {
            SetAlpha(targetAlpha);
            SetCanvasBlocking(blockAfterFade);
            completedEvent?.Invoke();
            return;
        }

        fadeCoroutine = StartCoroutine(FadeAndClearRoutine(targetAlpha, duration, blockAfterFade, completedEvent));
    }

    private IEnumerator FadeAndClearRoutine(float targetAlpha, float duration, bool blockAfterFade, UnityEvent completedEvent)
    {
        yield return FadeRoutine(targetAlpha, duration, blockAfterFade, completedEvent);
        fadeCoroutine = null;
    }

    private IEnumerator FadeInThenOutRoutine()
    {
        yield return FadeRoutine(1f, fadeInDuration, true, onFadeInComplete);
        yield return FadeRoutine(0f, fadeOutDuration, false, onFadeOutComplete);

        fadeCoroutine = null;
        onFadeInThenOutComplete?.Invoke();
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool blockAfterFade, UnityEvent completedEvent)
    {
        SetCanvasBlocking(true);

        float startAlpha = GetCurrentAlpha();

        if (duration <= 0f)
        {
            SetAlpha(targetAlpha);
        }
        else
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, progress));
                yield return null;
            }

            SetAlpha(targetAlpha);
        }

        SetCanvasBlocking(blockAfterFade);
        completedEvent?.Invoke();
    }

    private void StopFade()
    {
        if (fadeCoroutine == null)
        {
            return;
        }

        StopCoroutine(fadeCoroutine);
        fadeCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
        }
    }

    private void SetCanvasBlocking(bool isBlocking)
    {
        if (fadeImage == null)
        {
            return;
        }

        fadeImage.raycastTarget = isBlocking;
    }

    private float GetCurrentAlpha()
    {
        if (fadeImage == null)
        {
            return 0f;
        }

        return fadeImage.color.a;
    }
}
