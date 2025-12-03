using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class TimeController : MonoBehaviour
{
    [Header("Match")]
    [SerializeField] TextMeshProUGUI matchTimeText;
    [SerializeField] string matchTimeMessage;
    [SerializeField] string matchTimeMinutesReplacement;
    [SerializeField] string matchTimeSecondsReplacement;
    [SerializeField] int matchTimeMinutes = 5;
    [SerializeField] int matchTimeSeconds = 0;

    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] string countdownMessage;
    [SerializeField] string countdownNumberReplacement;
    [SerializeField] string endMatchMessage;
    [SerializeField] int countdown;

    [Tooltip("Name of the scene to load when the team reaches or exceeds the target points")]
    [SerializeField] private string winSceneName = "winScreen";
    [Tooltip("Name of the scene to load when the team fails to reach the target points")]
    [SerializeField] private string loseSceneName = "loseScreen";
    [SerializeField] private float endMessageDisplayDelay = 3f;
    [SerializeField] private int winPointsThreshold = 200;
    [SerializeField] private string nextLevelName = "LevelTwo";
    [SerializeField] private bool isFinalLevel = false;

    void Start()
    {
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        for (int i = countdown; i > 0; i--)
        {
            countdownText.text = countdownMessage.Replace(countdownNumberReplacement, i.ToString("00"));
            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(HideCountdownAfterDelay(0f));

        StartCoroutine(MatchTime());
    }

    private IEnumerator HideCountdownAfterDelay(float delay)
    {
        if (countdownText == null)
            yield break;

        yield return new WaitForSeconds(delay);
        countdownText.gameObject.SetActive(false);
    }

    IEnumerator MatchTime()
    {
        int totalSeconds = matchTimeMinutes * 60 + matchTimeSeconds;
        for (int i = totalSeconds; i > 0; i--)
        {
            int currentMinutes = i / 60;
            int currentSeconds = (int)(((i / 60f) - currentMinutes) * 60f);

            matchTimeText.text = matchTimeMessage.Replace(matchTimeMinutesReplacement, currentMinutes.ToString("00")).Replace(matchTimeSecondsReplacement, currentSeconds.ToString("00"));
            yield return new WaitForSeconds(1f);
        }

        matchTimeText.text = "";
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = endMatchMessage;
        }

        StartCoroutine(LoadEndSceneAfterDelay(endMessageDisplayDelay));
    }

    private IEnumerator LoadEndSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        int points = 0;
        if (GameManager.Instance != null)
        {
            points = GameManager.Instance.GetTotalPoints();
        }

        if (points >= winPointsThreshold)
        {
            if (isFinalLevel)
            {
                if (!string.IsNullOrEmpty(winSceneName))
                    SceneManager.LoadScene(winSceneName);
            }
            else
            {
                if (!string.IsNullOrEmpty(nextLevelName))
                    SceneManager.LoadScene(nextLevelName);
                else if (!string.IsNullOrEmpty(winSceneName))
                    SceneManager.LoadScene(winSceneName);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(loseSceneName))
                SceneManager.LoadScene(loseSceneName);
        }
    }
}