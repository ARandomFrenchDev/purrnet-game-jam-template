using System;
using PurrNet;
using UnityEngine;

// Place ce script sur un GameObject présent DANS LA SCÈNE (pas un prefab instancié
// dynamiquement) : PurrNet réseaute automatiquement les objets de scène qui ont
// un composant réseauté dessus, donc pas besoin de le spawn manuellement.
public class GameManager : NetworkBehaviour
{
    [SerializeField] private readonly float _roundDuration = 60f;

    // Timer réseauté avec réconciliation automatique entre clients
    private readonly SyncTimer _timer = new();

    // Score par joueur, synchronisé à tous les clients
    private readonly SyncDictionary<PlayerID, int> _scores = new();

    private bool _roundActive;

    // Événements locaux, à écouter depuis ton UI / spawner de nourriture / etc.
    public static event Action onRoundStarted;
    public static event Action onRoundEnded;
    public static event Action<PlayerID, int> onScoreChanged;

    private void Awake()
    {
        _timer.onTimerEnd += HandleTimerEnd;
        _scores.onChanged += OnOnScoreChanged;


    }

    protected override void OnSpawned(bool asServer)
    {
        if (!isServer)
            return;

        StartRound();
    }

    private void StartRound()
    {
        _scores.Clear();
        _roundActive = true;
        _timer.StartTimer(_roundDuration);
        BroadcastRoundStarted();
    }

    private void HandleTimerEnd()
    {
        if (!isServer || !_roundActive)
            return;

        _roundActive = false;
        BroadcastRoundEnded();
    }

    // Appelé par un PlayerController quand un joueur ramasse de la nourriture.
    // requireOwnership: false car ce script n'appartient à aucun joueur en particulier.
    [ServerRpc(requireOwnership: false)]
    public void RequestAddScore(int amount, RPCInfo info = default)
    {
        if (!_roundActive)
            return;

        var scorer = info.sender; // PlayerID de qui a appelé le RPC, fourni par le serveur
        if (_scores.ContainsKey(scorer))
            _scores[scorer] += amount;
        else
            _scores.Add(scorer, amount);
    }

    [ObserversRpc]
    private void BroadcastRoundStarted() => onRoundStarted?.Invoke();

    [ObserversRpc]
    private void BroadcastRoundEnded() => onRoundEnded?.Invoke();

    private static void OnOnScoreChanged(SyncDictionaryChange<PlayerID, int> change)
    {
        onScoreChanged?.Invoke(change.key, change.value);
    }
}