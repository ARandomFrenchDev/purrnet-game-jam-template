using System.Collections.Generic;
using PurrNet.Lobby;
using UnityEngine;
using UnityEngine.UI;

public class EndRoundLeaderboard : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform rowsParent;
    [SerializeField] private LeaderboardRow rowPrefab;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameOrchestrator _orchestrator; // même asset "Orchestrator.Jam" que dans LobbyManager
    private void OnEnable()
    {
        GameManager.OnRoundEnded += Show;
        quitButton.onClick.AddListener(QuitToMainMenu);
        panel.SetActive(false);

    }

    private void OnDisable()
    {
        GameManager.OnRoundEnded -= Show;
        quitButton.onClick.RemoveListener(QuitToMainMenu);
    }

    private void Show()
    {
        Debug.Log("showing leaderboard ending");
        BuildRows();
        panel.SetActive(true);
    }

    private void BuildRows()
    {
        foreach (Transform child in rowsParent)
            Destroy(child.gameObject);

        var players = FindObjectsOfType<PlayerController>();
        var infos = new List<(string username, int score)>();

        foreach (var p in players)
        {
            int score = GameManager.Instance.Scores.TryGetValue(p.owner.Value, out var s) ? s : 0;
            infos.Add((p.Username, score));
        }

        infos.Sort((a, b) => b.score.CompareTo(a.score)); // décroissant, meilleur score en haut

        foreach (var info in infos)
        {
            var row = Instantiate(rowPrefab, rowsParent);
            row.SetData(info.username, info.score);
        }
    }

    private void QuitToMainMenu()
    {
        if (PurrNet.NetworkManager.main.isServer)
            PurrNet.NetworkManager.main.StopServer();
        else
            PurrNet.NetworkManager.main.StopClient();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}