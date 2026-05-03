using UnityEngine;

public enum ActionPointSpendTarget
{
    None,
    Teacher,
    Mate,
    Windows
}

public class ActionPointSystem : MonoBehaviour
{
    public const int DailyActionPoints = 12;

    [Header("Action Point Settings")]
    public int currentActionPoints = DailyActionPoints;
    [SerializeField] private int actionCostPerCommand = 1;

    [Header("Global Action Spend Stats")]
    public int teacherSpentActionPoints;
    public int mateSpentActionPoints;
    public int windowsSpentActionPoints;

    [Header("Action State")]
    public bool startActionCommand;

    [Header("External Systems")]
    [SerializeField] private DaySystem daySystem;

    public int MaxActionPoints => DailyActionPoints;
    public int CurrentActionPoints => currentActionPoints;
    public int ActionCostPerCommand => actionCostPerCommand;
    public int SpentActionPoints => Mathf.Max(0, DailyActionPoints - currentActionPoints);
    public int TeacherSpentActionPoints => teacherSpentActionPoints;
    public int MateSpentActionPoints => mateSpentActionPoints;
    public int WindowsSpentActionPoints => windowsSpentActionPoints;

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
        windowsSpentActionPoints = 0;
    }

    public void ResetActionPoints()
    {
        currentActionPoints = DailyActionPoints;
    }

    public void SetCurrentActionPoints(int amount)
    {
        currentActionPoints = Mathf.Clamp(amount, 0, DailyActionPoints);
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
        actionCostPerCommand = Mathf.Max(1, actionCostPerCommand);
        currentActionPoints = Mathf.Clamp(currentActionPoints, 0, DailyActionPoints);
        teacherSpentActionPoints = Mathf.Max(0, teacherSpentActionPoints);
        mateSpentActionPoints = Mathf.Max(0, mateSpentActionPoints);
        windowsSpentActionPoints = Mathf.Max(0, windowsSpentActionPoints);
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
            case ActionPointSpendTarget.Windows:
                windowsSpentActionPoints += amount;
                break;
        }
    }
}
