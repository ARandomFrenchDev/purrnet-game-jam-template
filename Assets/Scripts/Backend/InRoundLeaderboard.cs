using System.Collections.Generic;
using UnityEngine;

public class InRoundLeaderboard : MonoBehaviour
{
    // Ordre dans l'inspector : 0 = haut-gauche, 1 = haut-droite, 2 = bas-gauche, 3 = bas-droite
    [SerializeField] private ScoreCornerUI[] corners;

    private void OnEnable()
    {
        GameManager.OnRoundStarted += Refresh;
        InvokeRepeating(nameof(Refresh), 0f, 0.3f); // rafraîchit toutes les 0.3 secondes
    }

    private void OnDisable()
    {
        GameManager.OnRoundStarted -= Refresh;
        CancelInvoke(nameof(Refresh));
    }

    private void Refresh()
    {
        var players = FindObjectsOfType<PlayerController>();
        var infos = new List<(int slot, string username, int score)>();

        foreach (var p in players)
        {
            int score = GameManager.Instance.Scores.TryGetValue(p.owner.Value, out var s) ? s : 0;
            infos.Add((p.SlotIndex, p.Username, score));
        }

        infos.Sort((a, b) => a.slot.CompareTo(b.slot));

        for (int i = 0; i < corners.Length; i++)
        {
            if (i < infos.Count)
                corners[i].SetData(infos[i].username, infos[i].score);
            else
                corners[i].Hide();
        }
    }
}