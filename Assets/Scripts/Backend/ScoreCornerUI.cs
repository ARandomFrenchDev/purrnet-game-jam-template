using TMPro;
using UnityEngine;

public class ScoreCornerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text scoreText;

    public void SetData(string username, int score)
    {
        usernameText.text = username;
        scoreText.text = score.ToString();
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}