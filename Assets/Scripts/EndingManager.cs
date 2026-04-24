using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EndingValueCheck
{
    Ignore,
    AtLeast,
    LessThan
}

[Serializable]
public class EndingRule
{
    public int endingIndex = 1;

    [Header("Teacher Dialog")]
    public EndingValueCheck teacherDialogCheck = EndingValueCheck.Ignore;
    public int teacherDialogValue;

    [Header("Mate Dialog")]
    public EndingValueCheck mateDialogCheck = EndingValueCheck.Ignore;
    public int mateDialogValue;

    [Header("Exam Score")]
    public EndingValueCheck examScoreCheck = EndingValueCheck.Ignore;
    public float examScoreValue;
}

public class EndingManager : MonoBehaviour
{
    [Header("Data Sources")]
    [SerializeField] private ActionPointSystem actionPointSystem;

    [Header("Ending Canvas")]
    [SerializeField] private EndingCanvasManager endingCanvasManager;

    [Header("Default Thresholds")]
    [SerializeField] private int highDialogCount = 10;
    [SerializeField] private float passExamScore = 60f;

    [Header("Ending Rules")]
    [SerializeField] private List<EndingRule> endingRules = new List<EndingRule>();
    [SerializeField] private int fallbackEndingIndex = 0;

    [Header("Before Ending Events")]
    public List<UnityEvent> beforeEndingEvents = new List<UnityEvent>();

    private int currentEndingIndex = -1;

    public int CurrentEndingIndex => currentEndingIndex;
    public int TeacherDialogCount => actionPointSystem != null ? actionPointSystem.TeacherSpentActionPoints : 0;
    public int MateDialogCount => actionPointSystem != null ? actionPointSystem.MateSpentActionPoints : 0;
    public float ExamScore => MultiQuestionUIController.GlobalExamScore;

    private void Awake()
    {
        ResolveReferences();
        EnsureDefaultRules();
    }

    private void Reset()
    {
        ResolveReferences();
        RebuildDefaultRules();
    }

    [ContextMenu("Play Ending")]
    public void PlayEnding()
    {
        int endingIndex = PickEndingIndex();
        PlayEndingIndex(endingIndex);
    }

    [ContextMenu("Play Ending Index 0")]
    public void PlayEndingIndex0()
    {
        PlayEndingIndex(0);
    }

    public void PlayEndingIndex(int endingIndex)
    {
        ResolveReferences();
        currentEndingIndex = endingIndex;

        InvokeBeforeEndingEvents();

        if (endingCanvasManager == null)
        {
            Debug.LogWarning("EndingManager: endingCanvasManager is not assigned.", this);
            return;
        }

        endingCanvasManager.gameObject.SetActive(true);
        endingCanvasManager.ShowEnding(endingIndex);
    }

    public int PickEndingIndex()
    {
        int teacherDialogCount = TeacherDialogCount;
        int mateDialogCount = MateDialogCount;
        float examScore = ExamScore;

        EnsureDefaultRules();

        foreach (EndingRule rule in endingRules)
        {
            if (rule != null && IsRuleMatched(rule, teacherDialogCount, mateDialogCount, examScore))
            {
                return rule.endingIndex;
            }
        }

        return fallbackEndingIndex;
    }

    public void SetExamScore(float score)
    {
        MultiQuestionUIController.SetGlobalExamScore(score);
    }

    public void RebuildDefaultRules()
    {
        endingRules = new List<EndingRule>
        {
            CreateRule(0, EndingValueCheck.AtLeast, highDialogCount, EndingValueCheck.LessThan, highDialogCount, EndingValueCheck.AtLeast, passExamScore),
            CreateRule(0, EndingValueCheck.AtLeast, highDialogCount, EndingValueCheck.LessThan, highDialogCount, EndingValueCheck.LessThan, passExamScore),
            CreateRule(0, EndingValueCheck.LessThan, highDialogCount, EndingValueCheck.AtLeast, highDialogCount, EndingValueCheck.AtLeast, passExamScore),
            CreateRule(0, EndingValueCheck.LessThan, highDialogCount, EndingValueCheck.AtLeast, highDialogCount, EndingValueCheck.LessThan, passExamScore),
            CreateRule(0, EndingValueCheck.AtLeast, highDialogCount, EndingValueCheck.AtLeast, highDialogCount, EndingValueCheck.AtLeast, passExamScore),
            CreateRule(0, EndingValueCheck.Ignore, 0, EndingValueCheck.Ignore, 0, EndingValueCheck.Ignore, 0f)
        };
    }

    private void ResolveReferences()
    {
        if (actionPointSystem == null)
        {
            actionPointSystem = FindObjectOfType<ActionPointSystem>();
        }

        if (endingCanvasManager == null)
        {
            endingCanvasManager = FindObjectOfType<EndingCanvasManager>();
        }

        if (endingCanvasManager == null)
        {
            EndingCanvasManager[] canvasManagers = FindObjectsOfType<EndingCanvasManager>(true);
            if (canvasManagers.Length > 0)
            {
                endingCanvasManager = canvasManagers[0];
            }
        }
    }

    private void EnsureDefaultRules()
    {
        if (endingRules == null)
        {
            endingRules = new List<EndingRule>();
        }

        if (endingRules.Count == 0)
        {
            RebuildDefaultRules();
        }
    }

    private void InvokeBeforeEndingEvents()
    {
        if (beforeEndingEvents == null)
        {
            return;
        }

        foreach (UnityEvent endingEvent in beforeEndingEvents)
        {
            endingEvent?.Invoke();
        }
    }

    private bool IsRuleMatched(EndingRule rule, int teacherDialogCount, int mateDialogCount, float examScore)
    {
        return IsIntMatched(rule.teacherDialogCheck, teacherDialogCount, rule.teacherDialogValue)
            && IsIntMatched(rule.mateDialogCheck, mateDialogCount, rule.mateDialogValue)
            && IsFloatMatched(rule.examScoreCheck, examScore, rule.examScoreValue);
    }

    private bool IsIntMatched(EndingValueCheck check, int actualValue, int ruleValue)
    {
        switch (check)
        {
            case EndingValueCheck.AtLeast:
                return actualValue >= ruleValue;
            case EndingValueCheck.LessThan:
                return actualValue < ruleValue;
            default:
                return true;
        }
    }

    private bool IsFloatMatched(EndingValueCheck check, float actualValue, float ruleValue)
    {
        switch (check)
        {
            case EndingValueCheck.AtLeast:
                return actualValue >= ruleValue;
            case EndingValueCheck.LessThan:
                return actualValue < ruleValue;
            default:
                return true;
        }
    }

    private EndingRule CreateRule(
        int endingIndex,
        EndingValueCheck teacherCheck,
        int teacherValue,
        EndingValueCheck mateCheck,
        int mateValue,
        EndingValueCheck examCheck,
        float examValue)
    {
        return new EndingRule
        {
            endingIndex = endingIndex,
            teacherDialogCheck = teacherCheck,
            teacherDialogValue = teacherValue,
            mateDialogCheck = mateCheck,
            mateDialogValue = mateValue,
            examScoreCheck = examCheck,
            examScoreValue = examValue
        };
    }
}
