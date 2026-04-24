using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 多题UI控制器 - 用于管理多个UIImageFillControllerManager并进行答案对比评分
/// </summary>
public class MultiQuestionUIController : MonoBehaviour
{
    public static float GlobalExamScore { get; private set; }

    [SerializeField] private List<UIImageFillControllerManager> managers = new List<UIImageFillControllerManager>();
    [SerializeField] private string correctAnswers = ""; // 正确答案，使用标签组合如"ABC"或"A-C"（-表示空）

    private QuizResult lastResult;

    /// <summary>
    /// 检查答案并返回评分结果
    /// </summary>
    public QuizResult CheckAnswers()
    {
        if (managers == null || managers.Count == 0)
        {
            Debug.LogError("MultiQuestionUIController: 没有配置任何管理器!");
            SetGlobalExamScore(0f);
            return new QuizResult { totalQuestions = 0, correctCount = 0, score = 0f };
        }

        if (string.IsNullOrEmpty(correctAnswers))
        {
            Debug.LogError("MultiQuestionUIController: 没有设置正确答案!");
            SetGlobalExamScore(0f);
            return new QuizResult { totalQuestions = 0, correctCount = 0, score = 0f };
        }

        List<FillOptionLabel?> currentAnswers = GetCurrentAnswers();
        List<FillOptionLabel?> correctAnswersList = ParseCorrectAnswers();

        // 验证答案数量是否匹配
        if (currentAnswers.Count != correctAnswersList.Count)
        {
            Debug.LogWarning($"MultiQuestionUIController: 当前答案数 {currentAnswers.Count} 与正确答案数 {correctAnswersList.Count} 不匹配!");
        }

        int correct = 0;
        int total = Mathf.Max(currentAnswers.Count, correctAnswersList.Count);

        for (int i = 0; i < total; i++)
        {
            FillOptionLabel? currentAnswer = i < currentAnswers.Count ? currentAnswers[i] : null;
            FillOptionLabel? correctAnswer = i < correctAnswersList.Count ? correctAnswersList[i] : null;

            if (currentAnswer == correctAnswer)
            {
                correct++;
            }
            else
            {
                Debug.Log($"第 {i + 1} 题: 你的答案是 {(currentAnswer.HasValue ? currentAnswer.Value.ToString() : "空")}, 正确答案是 {(correctAnswer.HasValue ? correctAnswer.Value.ToString() : "空")}");
            }
        }

        float score = total > 0 ? (float)correct / total * 100f : 0f;

        lastResult = new QuizResult
        {
            totalQuestions = total,
            correctCount = correct,
            score = score,
            currentAnswers = currentAnswers,
            correctAnswers = correctAnswersList
        };

        SetGlobalExamScore(score);
        Debug.Log($"MultiQuestionUIController: 测试完成 - 正确 {correct}/{total}, 分数 {score:F1}%");

        return lastResult;
    }

    public static void SetGlobalExamScore(float score)
    {
        GlobalExamScore = Mathf.Clamp(score, 0f, 100f);
        ExamScoreGlobal.SetGlobalScore(GlobalExamScore);
    }

    /// <summary>
    /// 获取当前所有答案
    /// </summary>
    private List<FillOptionLabel?> GetCurrentAnswers()
    {
        List<FillOptionLabel?> answers = new List<FillOptionLabel?>();

        foreach (UIImageFillControllerManager manager in managers)
        {
            if (manager == null)
            {
                answers.Add(null);
                continue;
            }

            FillOptionLabel? selectedLabel = manager.GetSelectedOptionLabel();
            answers.Add(selectedLabel);
        }

        return answers;
    }

    /// <summary>
    /// 解析正确答案字符串
    /// 格式: "abc" 或 "ABC" 表示依次选A、B、C
    /// 注意：没有空选项的概念，但答案可能为空（用户未选中）
    /// </summary>
    private List<FillOptionLabel?> ParseCorrectAnswers()
    {
        List<FillOptionLabel?> answers = new List<FillOptionLabel?>();

        if (string.IsNullOrEmpty(correctAnswers))
        {
            return answers;
        }

        // 直接逐个字符处理，支持"abc"或"ABC"格式
        foreach (char answerChar in correctAnswers)
        {
            if (char.IsLetter(answerChar))
            {
                if (System.Enum.TryParse<FillOptionLabel>(answerChar.ToString().ToUpper(), out FillOptionLabel label))
                {
                    answers.Add(label);
                }
                else
                {
                    Debug.LogWarning($"MultiQuestionUIController: 无法解析答案字符 '{answerChar}'");
                }
            }
        }

        return answers;
    }

    /// <summary>
    /// 设置正确答案
    /// </summary>
    public void SetCorrectAnswers(string answers)
    {
        correctAnswers = answers;
        Debug.Log($"MultiQuestionUIController: 已设置正确答案为 '{answers}'");
    }

    /// <summary>
    /// 获取最后一次的评分结果
    /// </summary>
    public QuizResult GetLastResult()
    {
        return lastResult;
    }

    /// <summary>
    /// 检查答案并打印分数（可在面板上调用）
    /// </summary>
    public void CheckAnswersAndLogScore()
    {
        QuizResult result = CheckAnswers();
        Debug.Log($"MultiQuestionUIController: 最终分数 {result.score:F1}% ({result.correctCount}/{result.totalQuestions})");
    }

    /// <summary>
    /// 重置所有管理器（清空所有答案）
    /// </summary>
    public void ResetAll()
    {
        foreach (UIImageFillControllerManager manager in managers)
        {
            if (manager != null)
            {
                manager.ClearAll();
            }
        }

        Debug.Log("MultiQuestionUIController: 已重置所有选项");
    }

    /// <summary>
    /// 获取当前答案字符串（用于显示或调试）
    /// </summary>
    public string GetCurrentAnswersString()
    {
        List<FillOptionLabel?> answers = GetCurrentAnswers();
        string result = "";

        foreach (FillOptionLabel? answer in answers)
        {
            if (result.Length > 0)
                result += "-";

            result += answer.HasValue ? answer.Value.ToString() : "";
        }

        return result;
    }

    /// <summary>
    /// 添加管理器
    /// </summary>
    public void AddManager(UIImageFillControllerManager manager)
    {
        if (manager != null && !managers.Contains(manager))
        {
            managers.Add(manager);
        }
    }

    /// <summary>
    /// 移除管理器
    /// </summary>
    public void RemoveManager(UIImageFillControllerManager manager)
    {
        if (manager != null)
        {
            managers.Remove(manager);
        }
    }
}

/// <summary>
/// 测试结果结构体
/// </summary>
public struct QuizResult
{
    public int totalQuestions;      // 总题数
    public int correctCount;        // 正确题数
    public float score;             // 分数 (0-100)
    public List<FillOptionLabel?> currentAnswers;  // 当前答案列表
    public List<FillOptionLabel?> correctAnswers;  // 正确答案列表
}
