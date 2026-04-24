using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DayNightFadeTransition : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject fadeCanvasRoot;
    [SerializeField] private Image fadeImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool startBlack = false;
    [SerializeField] private bool disableCanvasAfterFadeOut = true;

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

        if (!startBlack && disableCanvasAfterFadeOut)
        {
            SetCanvasActive(false);
        }
    }

    private void OnDisable()
    {
        StopFade();
    }

    public void FadeIn()
    {
        SetCanvasActive(true);
        StartFade(1f, fadeInDuration, true, onFadeInComplete);
    }

    public void FadeOut()
    {
        StartFade(0f, fadeOutDuration, false, onFadeOutComplete, disableCanvasAfterFadeOut);
    }

    public void FadeInThenOut()
    {
        StopFade();
        SetCanvasActive(true);

        if (!isActiveAndEnabled)
        {
            SetAlpha(0f);
            SetCanvasBlocking(false);
            if (disableCanvasAfterFadeOut)
            {
                SetCanvasActive(false);
            }
            onFadeInThenOutComplete?.Invoke();
            return;
        }

        fadeCoroutine = StartCoroutine(FadeInThenOutRoutine());
    }

    public void SetBlackInstant()
    {
        StopFade();
        SetCanvasActive(true);
        SetAlpha(1f);
        SetCanvasBlocking(true);
    }

    public void SetClearInstant()
    {
        StopFade();
        SetAlpha(0f);
        SetCanvasBlocking(false);
        if (disableCanvasAfterFadeOut)
        {
            SetCanvasActive(false);
        }
    }

    private void ResolveReferences()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponentInChildren<Image>(true);
        }

        if (fadeCanvasRoot == null && fadeImage != null)
        {
            Canvas canvas = fadeImage.GetComponentInParent<Canvas>(true);
            fadeCanvasRoot = canvas != null ? canvas.gameObject : fadeImage.gameObject;
        }
    }

    private void StartFade(
        float targetAlpha,
        float duration,
        bool blockAfterFade,
        UnityEvent completedEvent,
        bool disableCanvasWhenComplete = false)
    {
        StopFade();
        SetCanvasActive(true);

        if (!isActiveAndEnabled)
        {
            SetAlpha(targetAlpha);
            SetCanvasBlocking(blockAfterFade);
            if (disableCanvasWhenComplete)
            {
                SetCanvasActive(false);
            }
            completedEvent?.Invoke();
            return;
        }

        fadeCoroutine = StartCoroutine(FadeAndClearRoutine(
            targetAlpha,
            duration,
            blockAfterFade,
            completedEvent,
            disableCanvasWhenComplete));
    }

    private IEnumerator FadeAndClearRoutine(
        float targetAlpha,
        float duration,
        bool blockAfterFade,
        UnityEvent completedEvent,
        bool disableCanvasWhenComplete)
    {
        yield return FadeRoutine(targetAlpha, duration, blockAfterFade, completedEvent);
        fadeCoroutine = null;

        if (disableCanvasWhenComplete)
        {
            SetCanvasActive(false);
        }
    }

    private IEnumerator FadeInThenOutRoutine()
    {
        yield return FadeRoutine(1f, fadeInDuration, true, onFadeInComplete);
        yield return FadeRoutine(0f, fadeOutDuration, false, onFadeOutComplete);

        fadeCoroutine = null;
        onFadeInThenOutComplete?.Invoke();

        if (disableCanvasAfterFadeOut)
        {
            SetCanvasActive(false);
        }
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

    private void SetCanvasActive(bool isActive)
    {
        ResolveReferences();

        if (fadeCanvasRoot != null)
        {
            fadeCanvasRoot.SetActive(isActive);
        }
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
