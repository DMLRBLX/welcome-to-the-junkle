using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scoring")]
    [SerializeField] private int totalPoints = 0;
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints(int amount)
    {
        totalPoints += amount;
        UpdateScoreUI();
    }

    public int GetTotalPoints() => totalPoints;

    public void ResetPoints()
    {
        totalPoints = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = totalPoints.ToString();
            // scoreText.text = $"Score: {totalPoints}";
        }
    }
}
