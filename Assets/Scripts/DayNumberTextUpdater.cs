using TMPro;
using UnityEngine;

public class DayNumberTextUpdater : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private DaySystem daySystem;

    [Header("UI References")]
    [SerializeField] private TMP_Text targetText;

    private int lastDay = int.MinValue;

    private void Awake()
    {
        ResolveReferences();

        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        ResolveReferences();
        RefreshTextIfChanged();
    }

    public void Refresh()
    {
        lastDay = int.MinValue;
        RefreshTextIfChanged();
    }

    private void ResolveReferences()
    {
        if (daySystem == null)
        {
            daySystem = FindObjectOfType<DaySystem>();
        }
    }

    private void RefreshTextIfChanged()
    {
        if (targetText == null)
        {
            return;
        }

        int currentDay = daySystem != null ? daySystem.CurrentDay : 0;
        if (currentDay == lastDay)
        {
            return;
        }

        lastDay = currentDay;
        targetText.text = currentDay.ToString();
    }
}
