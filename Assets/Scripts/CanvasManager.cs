using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ManagedCanvasEntry
{
    public string key;
    public GameObject canvasObject;
}

public class CanvasManager : MonoBehaviour
{
    [Header("Managed Canvases")]
    [SerializeField] private List<ManagedCanvasEntry> canvases = new List<ManagedCanvasEntry>();

    [Header("Auto Collect")]
    [SerializeField] private bool autoCollectDirectChildrenOnAwake = false;

    public IReadOnlyList<ManagedCanvasEntry> Canvases => canvases;

    private void Awake()
    {
        if (autoCollectDirectChildrenOnAwake)
        {
            CollectDirectChildCanvases();
        }
    }

    [ContextMenu("Collect Direct Child Canvases")]
    public void CollectDirectChildCanvases()
    {
        canvases.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child.GetComponent<Canvas>() == null)
            {
                continue;
            }

            canvases.Add(new ManagedCanvasEntry
            {
                key = child.name,
                canvasObject = child.gameObject
            });
        }
    }

    public void ShowAll()
    {
        SetAllActive(true);
    }

    public void HideAll()
    {
        SetAllActive(false);
    }

    public void SetAllActive(bool isActive)
    {
        foreach (ManagedCanvasEntry entry in canvases)
        {
            SetEntryActive(entry, isActive);
        }
    }

    public void ShowCanvas(int index)
    {
        SetCanvasActive(index, true);
    }

    public void HideCanvas(int index)
    {
        SetCanvasActive(index, false);
    }

    public void SetCanvasActive(int index, bool isActive)
    {
        ManagedCanvasEntry entry = GetEntry(index);
        if (entry == null)
        {
            Debug.LogWarning($"CanvasManager: canvas index {index} is out of range.", this);
            return;
        }

        SetEntryActive(entry, isActive);
    }

    public void ShowCanvas(string key)
    {
        SetCanvasActive(key, true);
    }

    public void HideCanvas(string key)
    {
        SetCanvasActive(key, false);
    }

    public void SetCanvasActive(string key, bool isActive)
    {
        ManagedCanvasEntry entry = GetEntry(key);
        if (entry == null)
        {
            Debug.LogWarning($"CanvasManager: canvas key '{key}' was not found.", this);
            return;
        }

        SetEntryActive(entry, isActive);
    }

    public void ShowOnlyCanvas(int index)
    {
        for (int i = 0; i < canvases.Count; i++)
        {
            SetEntryActive(canvases[i], i == index);
        }
    }

    public void ShowOnlyCanvas(string key)
    {
        foreach (ManagedCanvasEntry entry in canvases)
        {
            bool shouldShow = entry != null &&
                string.Equals(entry.key, key, StringComparison.OrdinalIgnoreCase);
            SetEntryActive(entry, shouldShow);
        }
    }

    public void ShowSubtitleCanvas()
    {
        ShowCanvas("Canvas_subtitle");
    }

    public void ShowStartCanvas()
    {
        ShowCanvas("Canvas_start");
    }

    public void ShowEndCanvas()
    {
        ShowCanvas("Canvas_end");
    }

    public void ShowDebugCanvas()
    {
        ShowCanvas("Canvas_debug");
    }

    public void HideSubtitleCanvas()
    {
        HideCanvas("Canvas_subtitle");
    }

    public void HideStartCanvas()
    {
        HideCanvas("Canvas_start");
    }

    public void HideEndCanvas()
    {
        HideCanvas("Canvas_end");
    }

    public void HideDebugCanvas()
    {
        HideCanvas("Canvas_debug");
    }

    public void ShowOnlySubtitleCanvas()
    {
        ShowOnlyCanvas("Canvas_subtitle");
    }

    public void ShowOnlyStartCanvas()
    {
        ShowOnlyCanvas("Canvas_start");
    }

    public void ShowOnlyEndCanvas()
    {
        ShowOnlyCanvas("Canvas_end");
    }

    public void ShowOnlyDebugCanvas()
    {
        ShowOnlyCanvas("Canvas_debug");
    }

    private ManagedCanvasEntry GetEntry(int index)
    {
        if (index < 0 || index >= canvases.Count)
        {
            return null;
        }

        return canvases[index];
    }

    private ManagedCanvasEntry GetEntry(string key)
    {
        foreach (ManagedCanvasEntry entry in canvases)
        {
            if (entry != null && string.Equals(entry.key, key, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        return null;
    }

    private void SetEntryActive(ManagedCanvasEntry entry, bool isActive)
    {
        if (entry?.canvasObject != null)
        {
            entry.canvasObject.SetActive(isActive);
        }
    }
}
