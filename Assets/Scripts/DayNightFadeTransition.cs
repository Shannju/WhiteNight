using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class DayNightFadeTransition : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool startBlack = false;

    [Header("Fade Events")]
    [SerializeField] private UnityEvent onFadeInComplete;
    [SerializeField] private UnityEvent onFadeOutComplete;
    [SerializeField] private UnityEvent onFadeOutThenInComplete;

    private Coroutine fadeCoroutine;

    public bool IsFading => fadeCoroutine != null;
    public UnityEvent OnFadeInComplete => onFadeInComplete;
    public UnityEvent OnFadeOutComplete => onFadeOutComplete;
    public UnityEvent OnFadeOutThenInComplete => onFadeOutThenInComplete;

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

    public void FadeOutThenIn()
    {
        StopFade();

        if (!gameObject.activeInHierarchy)
        {
            SetAlpha(1f);
            SetCanvasBlocking(true);
            onFadeOutThenInComplete?.Invoke();
            return;
        }

        fadeCoroutine = StartCoroutine(FadeOutThenInRoutine());
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
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
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

    private IEnumerator FadeOutThenInRoutine()
    {
        yield return FadeRoutine(0f, fadeOutDuration, false, onFadeOutComplete);
        yield return FadeRoutine(1f, fadeInDuration, true, onFadeInComplete);

        fadeCoroutine = null;
        onFadeOutThenInComplete?.Invoke();
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool blockAfterFade, UnityEvent completedEvent)
    {
        SetCanvasBlocking(true);

        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 0f;

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
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }
    }

    private void SetCanvasBlocking(bool isBlocking)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.blocksRaycasts = isBlocking;
        canvasGroup.interactable = isBlocking;
    }
}
