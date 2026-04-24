using UnityEngine;

public class ExamScoreGlobal : MonoBehaviour
{
    public static float CurrentScore { get; private set; }

    [Header("Exam Score")]
    [Range(0f, 100f)]
    [SerializeField] private float score;

    [SerializeField] private bool keepBetweenScenes = true;

    public float Score => score;

    private void Awake()
    {
        SetScore(score);

        if (keepBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnValidate()
    {
        score = Mathf.Clamp(score, 0f, 100f);
    }

    public void SetScore(float value)
    {
        score = Mathf.Clamp(value, 0f, 100f);
        CurrentScore = score;
    }

    public static void SetGlobalScore(float value)
    {
        CurrentScore = Mathf.Clamp(value, 0f, 100f);

        ExamScoreGlobal instance = FindObjectOfType<ExamScoreGlobal>();
        if (instance != null)
        {
            instance.score = CurrentScore;
        }
    }
}
