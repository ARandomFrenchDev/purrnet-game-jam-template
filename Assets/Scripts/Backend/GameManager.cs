using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Lobby;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Round settings")]
    [SerializeField] public float roundDuration = 60f;
    [SerializeField] private float countdownDuration = 3f;

    [Header("Player count source")]
    [SerializeField] private GameOrchestrator _orchestrator; // même asset "Orchestrator.Jam" que dans LobbyManager
    [SerializeField] private int fallbackPlayerCount = 4; // filet de sécurité si tu testes sans passer par le lobby

    private readonly SyncTimer _roundTimer = new();
    private readonly SyncTimer _countdownTimer = new();
    private readonly SyncDictionary<PlayerID, int> _scores = new();
    private readonly SyncVar<int> _connectedPlayerCount = new(0);

    private int _expectedPlayerCount;
    private readonly SyncVar<bool> _roundActive = new(false);
    private bool _countdownStarted;

    public static event Action OnRoundStarted;
    public static event Action OnRoundEnded;
    public static event Action<int> OnCountdownTick; // 3, 2, 1
    public static event Action<float> OnTimerTick;

    private void Awake()
    {
        Instance = this;

        _roundTimer.onTimerEnd += HandleRoundEnd;
        _roundTimer.onTimerSecondTick += () => OnTimerTick?.Invoke(_roundTimer.remaining);

        _countdownTimer.onTimerEnd += HandleCountdownEnd;
        _countdownTimer.onTimerStart += () => OnCountdownTick?.Invoke(Mathf.CeilToInt(_countdownTimer.remaining));
    }

    protected override void OnSpawned(bool asServer)
    {
        if (!isServer)
            return;

        _expectedPlayerCount = _orchestrator != null && _orchestrator.activeLobby != null
            ? _orchestrator.activeLobby.players.Count
            : fallbackPlayerCount;
    }

    // Appelée par chaque PlayerController depuis OnSpawned(asServer: true)
    public void RegisterPlayer()
    {
        if (!isServer || _countdownStarted)
            return;

        _connectedPlayerCount.value++;

        if (_connectedPlayerCount.value >= _expectedPlayerCount)
            StartCountdown();
    }

    private void StartCountdown()
    {
        _countdownStarted = true;
        _countdownTimer.StartTimer(countdownDuration);
    }

    private void HandleCountdownEnd()
    {
        if (!isServer)
            return;

        StartRound();
    }

    private void StartRound()
    {
        _scores.Clear();
        _roundActive.value = true;
        _roundTimer.StartTimer(roundDuration);
        BroadcastRoundStarted();
    }

    private void HandleRoundEnd()
    {
        if (!isServer || !_roundActive)
            return;

        _roundActive.value = false;
        BroadcastRoundEnded();
    }

    public void AddScoreServer(PlayerController player, int amount)
    {
        if (!isServer || !_roundActive)
            return;

        var id = player.owner.Value;
        if (_scores.ContainsKey(id))
            _scores[id] += amount;
        else
            _scores.Add(id, amount);
    }

    public SyncDictionary<PlayerID, int> Scores => _scores;
    public bool IsRoundActive => _roundActive;

    [ObserversRpc]
    private void BroadcastRoundStarted() => OnRoundStarted?.Invoke();

    [ObserversRpc]
    private void BroadcastRoundEnded()  {
        OnRoundEnded?.Invoke();
    }
}