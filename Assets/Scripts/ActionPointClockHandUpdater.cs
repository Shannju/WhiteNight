using UnityEngine;

public class ActionPointClockHandUpdater : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private ActionPointSystem actionPointSystem;

    [Header("Clock Settings")]
    [SerializeField] private int totalActionPoints = 12;
    [SerializeField] private float fullActionPointsZ = 180f;
    [SerializeField] private float emptyActionPointsZ = -180f;

    private int lastActionPoints = int.MinValue;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        ResolveReferences();
        RefreshHandIfChanged();
    }

    public void Refresh()
    {
        lastActionPoints = int.MinValue;
        RefreshHandIfChanged();
    }

    private void ResolveReferences()
    {
        if (actionPointSystem == null)
        {
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
        }
    }

    private void RefreshHandIfChanged()
    {
        if (actionPointSystem == null)
        {
            return;
        }

        int actionPointCount = Mathf.Max(1, totalActionPoints);
        int currentActionPoints = Mathf.Clamp(actionPointSystem.CurrentActionPoints, 0, actionPointCount);

        if (currentActionPoints == lastActionPoints)
        {
            return;
        }

        lastActionPoints = currentActionPoints;

        float spentRatio = (actionPointCount - currentActionPoints) / (float)actionPointCount;
        float zRotation = Mathf.Lerp(fullActionPointsZ, emptyActionPointsZ, spentRatio);
        Vector3 eulerAngles = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
    }
}
