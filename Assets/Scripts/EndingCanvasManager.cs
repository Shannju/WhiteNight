using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class EndingCanvasEntry
{
    public int index;
    public GameObject imageObject;
    public string title;

    [TextArea(3, 8)]
    public string content;
}

public class EndingCanvasManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform imageRoot;
    public TMP_Text titleTMPText;
    public TMP_Text contentTMPText;
    public TMP_Text scoreTMPText;

    [Header("Score Text")]
    [SerializeField] private string scoreFormat = "Score: {0:0}/100";

    [Header("Ending Data")]
    public List<EndingCanvasEntry> endings = new List<EndingCanvasEntry>();

    [Header("Startup")]
    public bool autoCollectImagesOnAwake = true;
    public bool showEndingOnStart = false;
    public int startEndingIndex = 0;

    private int currentEndingIndex = -1;

    public int CurrentEndingIndex => currentEndingIndex;

    private void Awake()
    {
        if (autoCollectImagesOnAwake)
        {
            CollectImagesFromImageRoot();
        }

        HideAllImages();
    }

    private void Start()
    {
        if (showEndingOnStart)
        {
            ShowEnding(startEndingIndex);
        }
    }

    [ContextMenu("Collect Images From Image Root")]
    public void CollectImagesFromImageRoot()
    {
        if (imageRoot == null)
        {
            Transform foundRoot = transform.Find("image");
            if (foundRoot != null)
            {
                imageRoot = foundRoot;
            }
        }

        if (imageRoot == null)
        {
            Debug.LogWarning("EndingCanvasManager: imageRoot is not assigned and child object named 'image' was not found.", this);
            return;
        }

        Dictionary<int, EndingCanvasEntry> existingEntries = new Dictionary<int, EndingCanvasEntry>();
        foreach (EndingCanvasEntry entry in endings)
        {
            if (entry == null)
                continue;

            existingEntries[entry.index] = entry;
        }

        List<EndingCanvasEntry> collectedEntries = new List<EndingCanvasEntry>();

        for (int i = 0; i < imageRoot.childCount; i++)
        {
            Transform child = imageRoot.GetChild(i);
            if (!int.TryParse(child.name, out int endingIndex))
                continue;

            EndingCanvasEntry entry;
            if (!existingEntries.TryGetValue(endingIndex, out entry) || entry == null)
            {
                entry = new EndingCanvasEntry
                {
                    index = endingIndex
                };
            }

            entry.index = endingIndex;
            entry.imageObject = child.gameObject;
            collectedEntries.Add(entry);
        }

        if (collectedEntries.Count == 0)
        {
            return;
        }

        endings.Clear();
        endings.AddRange(collectedEntries);
        endings.Sort((left, right) => left.index.CompareTo(right.index));
    }

    public void ShowEnding(int endingIndex)
    {
        EndingCanvasEntry entry = GetEnding(endingIndex);
        if (entry == null)
        {
            Debug.LogWarning($"EndingCanvasManager: ending index {endingIndex} was not found.", this);
            return;
        }

        HideAllImages();

        if (entry.imageObject != null)
        {
            entry.imageObject.SetActive(true);
        }

        SetTitle(entry.title);
        SetContent(entry.content);
        UpdateScoreText(ExamScoreGlobal.CurrentScore);
        currentEndingIndex = entry.index;
    }

    public void ShowEnding(int endingIndex, float examScore)
    {
        ShowEnding(endingIndex);
        UpdateScoreText(examScore);
    }

    public void ShowEndingByListPosition(int listPosition)
    {
        if (listPosition < 0 || listPosition >= endings.Count)
        {
            Debug.LogWarning($"EndingCanvasManager: list position {listPosition} is out of range.", this);
            return;
        }

        ShowEnding(endings[listPosition].index);
    }

    public void ShowNextEnding()
    {
        if (endings.Count == 0)
            return;

        int currentPosition = GetCurrentListPosition();
        int nextPosition = currentPosition < 0 ? 0 : (currentPosition + 1) % endings.Count;
        ShowEndingByListPosition(nextPosition);
    }

    public void ShowPreviousEnding()
    {
        if (endings.Count == 0)
            return;

        int currentPosition = GetCurrentListPosition();
        int previousPosition = currentPosition < 0 ? endings.Count - 1 : currentPosition - 1;
        if (previousPosition < 0)
        {
            previousPosition = endings.Count - 1;
        }

        ShowEndingByListPosition(previousPosition);
    }

    public void HideAllImages()
    {
        foreach (EndingCanvasEntry entry in endings)
        {
            if (entry != null && entry.imageObject != null)
            {
                entry.imageObject.SetActive(false);
            }
        }
    }

    public void HideEnding()
    {
        HideAllImages();
        SetTitle(string.Empty);
        SetContent(string.Empty);
        SetScoreText(string.Empty);
        currentEndingIndex = -1;
    }

    public void UpdateScoreText(float examScore)
    {
        SetScoreText(string.Format(scoreFormat, Mathf.Clamp(examScore, 0f, 100f)));
    }

    public EndingCanvasEntry GetEnding(int endingIndex)
    {
        foreach (EndingCanvasEntry entry in endings)
        {
            if (entry != null && entry.index == endingIndex)
            {
                return entry;
            }
        }

        return null;
    }

    private int GetCurrentListPosition()
    {
        for (int i = 0; i < endings.Count; i++)
        {
            if (endings[i] != null && endings[i].index == currentEndingIndex)
            {
                return i;
            }
        }

        return -1;
    }

    private void SetTitle(string value)
    {
        if (titleTMPText != null)
        {
            titleTMPText.text = value;
        }
    }

    private void SetContent(string value)
    {
        if (contentTMPText != null)
        {
            contentTMPText.text = value;
        }
    }

    private void SetScoreText(string value)
    {
        if (scoreTMPText != null)
        {
            scoreTMPText.text = value;
        }
    }
}
