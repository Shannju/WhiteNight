using UnityEngine;

public class ActionPointSystem : MonoBehaviour
{
    [Header("Action Point Settings")]
    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private int currentActionPoints = 3;
    [SerializeField] private int actionCostPerCommand = 1;

    [Header("Action State")]
    public bool startActionCommand;

    public int MaxActionPoints => maxActionPoints;
    public int CurrentActionPoints => currentActionPoints;
    public int ActionCostPerCommand => actionCostPerCommand;

    private void Awake()
    {
        maxActionPoints = Mathf.Max(0, maxActionPoints);
        actionCostPerCommand = Mathf.Max(1, actionCostPerCommand);
        currentActionPoints = Mathf.Clamp(currentActionPoints, 0, maxActionPoints);
    }

    private void Update()
    {
        if (!startActionCommand)
        {
            return;
        }

        TryStartAction();
        startActionCommand = false;
    }

    public bool CanStartAction()
    {
        return currentActionPoints >= actionCostPerCommand;
    }

    public bool TryStartAction()
    {
        if (!CanStartAction())
        {
            Debug.LogWarning($"{name} does not have enough action points to start an action.", this);
            return false;
        }

        currentActionPoints -= actionCostPerCommand;
        return true;
    }

    public void ReceiveStartActionCommand()
    {
        TryStartAction();
    }

    public void ResetActionPoints()
    {
        currentActionPoints = maxActionPoints;
    }

    public void AddActionPoints(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentActionPoints = Mathf.Clamp(currentActionPoints + amount, 0, maxActionPoints);
    }

    public void SetMaxActionPoints(int amount)
    {
        maxActionPoints = Mathf.Max(0, amount);
        currentActionPoints = Mathf.Clamp(currentActionPoints, 0, maxActionPoints);
    }

    public void SetCurrentActionPoints(int amount)
    {
        currentActionPoints = Mathf.Clamp(amount, 0, maxActionPoints);
    }

    public void SetActionCostPerCommand(int amount)
    {
        actionCostPerCommand = Mathf.Max(1, amount);
    }
}
