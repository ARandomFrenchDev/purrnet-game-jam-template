using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

// Un seul CatSpawner dans la scène de jeu
public class CatSpawner : NetworkBehaviour
{
    [Header("Prefab & points")]
    [SerializeField] private CatController catPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Nombre de chats (progression sur la durée du round)")]
    [SerializeField] private int maxCatsAtStart = 1;
    [SerializeField] private int maxCatsAtEnd = 4;

    [Header("Rythme de spawn")]
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;

    private readonly List<CatController> _activeCats = new();
    private Coroutine _spawnRoutine;
    private float _elapsed;

    private void OnEnable()
    {
        GameManager.OnRoundStarted += HandleRoundStarted;
        GameManager.OnRoundEnded += HandleRoundEnded;
        GameManager.OnTimerTick += HandleTimerTick;
    }

    private void OnDisable()
    {
        GameManager.OnRoundStarted -= HandleRoundStarted;
        GameManager.OnRoundEnded -= HandleRoundEnded;
        GameManager.OnTimerTick -= HandleTimerTick;
    }

    private void HandleRoundStarted()
    {
        if (!isServer)
            return;

        foreach (var cat in _activeCats)
            if (cat != null)
                Destroy(cat.gameObject);

        _activeCats.Clear();
        _elapsed = 0f;
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void HandleRoundEnded()
    {
        if (!isServer)
            return;

        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
    }

    private void HandleTimerTick(float remaining)
    {
        _elapsed = GameManager.Instance.roundDuration - remaining;
    }

    private int CurrentMaxCats()
    {
        float fraction = Mathf.Clamp01(_elapsed / GameManager.Instance.roundDuration);
        return Mathf.RoundToInt(Mathf.Lerp(maxCatsAtStart, maxCatsAtEnd, fraction));
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(wait);

            _activeCats.RemoveAll(c => c == null); // nettoie les chats déjà despawn

            if (_activeCats.Count < CurrentMaxCats() && spawnPoints.Length > 0)
                SpawnCat();
        }
    }

    private void SpawnCat()
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        CatController cat = Instantiate(catPrefab, point.position, point.rotation);
        cat.Initialize(patrolPoints);
        _activeCats.Add(cat);
    }
}