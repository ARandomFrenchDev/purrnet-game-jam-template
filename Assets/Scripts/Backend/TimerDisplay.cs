using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private void OnEnable() => GameManager.OnTimerTick += UpdateDisplay;
    private void OnDisable() => GameManager.OnTimerTick -= UpdateDisplay;

    private void UpdateDisplay(float remaining)
    {
        int seconds = Mathf.CeilToInt(remaining);
        timerText.text = seconds.ToString();
    }
}