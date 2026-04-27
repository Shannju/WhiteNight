using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class DailyBookImages
{
    [SerializeField] private int day = 1;
    [SerializeField] private Sprite firstImage;
    [SerializeField] private Sprite secondImage;

    public int Day => day;
    public Sprite FirstImage => firstImage;
    public Sprite SecondImage => secondImage;

    public void Normalize()
    {
        day = Mathf.Max(1, day);
    }
}

[System.Serializable]
public class BookImagesChangedEvent : UnityEvent<int, Sprite, Sprite>
{
}

public class BookManager : MonoBehaviour
{
    [Header("Day Source")]
    [SerializeField] private DaySystem daySystem;
    [SerializeField] private bool autoFindDaySystem = true;

    [Header("Target Images")]
    [SerializeField] private Image firstTargetImage;
    [SerializeField] private Image secondTargetImage;

    [Header("Daily Images")]
    [SerializeField] private List<DailyBookImages> dailyImages = new List<DailyBookImages>(8);
    [SerializeField] private bool keepLastImagesWhenDayMissing = true;

    [Header("Events")]
    [SerializeField] private BookImagesChangedEvent onBookImagesChanged = new BookImagesChangedEvent();

    public IReadOnlyList<DailyBookImages> DailyImages => dailyImages;
    public BookImagesChangedEvent OnBookImagesChanged => onBookImagesChanged;

    private DailyBookImages currentImages;

    private void Awake()
    {
        if (daySystem == null && autoFindDaySystem)
        {
            daySystem = FindObjectOfType<DaySystem>(true);
        }
    }

    private void OnEnable()
    {
        if (daySystem != null)
        {
            daySystem.DayStarted += ApplyImagesForDay;
        }
    }

    private void Start()
    {
        if (daySystem != null)
        {
            ApplyImagesForDay(daySystem.CurrentDay);
        }
    }

    private void OnDisable()
    {
        if (daySystem != null)
        {
            daySystem.DayStarted -= ApplyImagesForDay;
        }
    }

    private void OnValidate()
    {
        NormalizeDailyImages();
    }

    public void SetDaySystem(DaySystem system)
    {
        if (daySystem == system)
        {
            return;
        }

        if (isActiveAndEnabled && daySystem != null)
        {
            daySystem.DayStarted -= ApplyImagesForDay;
        }

        daySystem = system;

        if (isActiveAndEnabled && daySystem != null)
        {
            daySystem.DayStarted += ApplyImagesForDay;
            ApplyImagesForDay(daySystem.CurrentDay);
        }
    }

    public void ApplyImagesForDay(int day)
    {
        DailyBookImages images = FindImagesForDay(day);

        if (images == null)
        {
            if (!keepLastImagesWhenDayMissing)
            {
                SetSprites(day, null, null);
            }

            return;
        }

        currentImages = images;
        SetSprites(day, images.FirstImage, images.SecondImage);
    }

    public void ApplyImagesByListIndex(int index)
    {
        if (dailyImages == null || dailyImages.Count == 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, dailyImages.Count - 1);
        DailyBookImages images = dailyImages[index];
        if (images == null)
        {
            return;
        }

        currentImages = images;
        SetSprites(images.Day, images.FirstImage, images.SecondImage);
    }

    private DailyBookImages FindImagesForDay(int day)
    {
        if (dailyImages == null)
        {
            return null;
        }

        int normalizedDay = Mathf.Max(1, day);
        foreach (DailyBookImages images in dailyImages)
        {
            if (images != null && images.Day == normalizedDay)
            {
                return images;
            }
        }

        return null;
    }

    private void SetSprites(int day, Sprite firstSprite, Sprite secondSprite)
    {
        if (firstTargetImage != null)
        {
            firstTargetImage.sprite = firstSprite;
            firstTargetImage.enabled = firstSprite != null;
        }

        if (secondTargetImage != null)
        {
            secondTargetImage.sprite = secondSprite;
            secondTargetImage.enabled = secondSprite != null;
        }

        onBookImagesChanged.Invoke(day, firstSprite, secondSprite);
    }

    private void NormalizeDailyImages()
    {
        if (dailyImages == null)
        {
            dailyImages = new List<DailyBookImages>(8);
            return;
        }

        foreach (DailyBookImages images in dailyImages)
        {
            images?.Normalize();
        }
    }
}
