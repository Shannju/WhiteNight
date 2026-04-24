using UnityEngine;

public enum ActionPointSpendTarget
{
    None,
    Teacher,
    Mate
}

public class ActionPointSystem : MonoBehaviour
{
    [Header("Action Point Settings")]
    [SerializeField] private int maxActionPoints = 20;
    public int currentActionPoints = 20;
    [SerializeField] private int actionCostPerCommand = 1;

    [Header("Global Action Spend Stats")]
    public int teacherSpentActionPoints;
    public int mateSpentActionPoints;

    [Header("Action State")]
    public bool startActionCommand;

    [Header("External Systems")]
    [SerializeField] private DaySystem daySystem;

    public int MaxActionPoints => maxActionPoints;
    public int CurrentActionPoints => currentActionPoints;
    public int ActionCostPerCommand => actionCostPerCommand;
    public int SpentActionPoints => Mathf.Max(0, maxActionPoints - currentActionPoints);
    public int TeacherSpentActionPoints => teacherSpentActionPoints;
    public int MateSpentActionPoints => mateSpentActionPoints;

    private void Awake()
    {
        NormalizeActionPointState();

        if (daySystem == null)
        {
            daySystem = FindObjectOfType<DaySystem>();
        }
    }

    private void OnValidate()
    {
        NormalizeActionPointState();
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

    public bool TryStartAction(ActionPointSpendTarget spendTarget = ActionPointSpendTarget.None)
    {
        if (!CanStartAction())
        {
            Debug.LogWarning($"{name} does not have enough action points to start an action.", this);
            return false;
        }

        int spentAmount = actionCostPerCommand;
        currentActionPoints -= spentAmount;
        RecordActionPointSpend(spendTarget, spentAmount);

        if (currentActionPoints <= 0 && daySystem != null)
        {
            daySystem.nextDayCommand = true;
        }

        return true;
    }

    public void ReceiveStartActionCommand()
    {
        TryStartAction();
    }

    public void ResetActionPointSpendStats()
    {
        teacherSpentActionPoints = 0;
        mateSpentActionPoints = 0;
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

    public void SetDaySystem(DaySystem system)
    {
        daySystem = system;
    }

    private void NormalizeActionPointState()
    {
        maxActionPoints = Mathf.Max(0, maxActionPoints);
        actionCostPerCommand = Mathf.Max(1, actionCostPerCommand);
        currentActionPoints = Mathf.Clamp(currentActionPoints, 0, maxActionPoints);
        teacherSpentActionPoints = Mathf.Max(0, teacherSpentActionPoints);
        mateSpentActionPoints = Mathf.Max(0, mateSpentActionPoints);
    }

    private void RecordActionPointSpend(ActionPointSpendTarget spendTarget, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        switch (spendTarget)
        {
            case ActionPointSpendTarget.Teacher:
                teacherSpentActionPoints += amount;
                break;
            case ActionPointSpendTarget.Mate:
                mateSpentActionPoints += amount;
                break;
        }
    }
}
