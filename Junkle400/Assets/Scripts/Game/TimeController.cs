using System.Collections;
using UnityEngine;
using TMPro;
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

    [Header("Countdown")]
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] string countdownMessage;
    [SerializeField] string countdownNumberReplacement;
    [SerializeField] string endMatchMessage;
    [SerializeField] int countdown;

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
    }
}