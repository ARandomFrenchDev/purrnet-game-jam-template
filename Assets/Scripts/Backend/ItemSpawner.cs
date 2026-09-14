using PurrNet;
using UnityEngine;
using System.Collections;
using System.Linq;

public class ItemSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] _itemPrefabs; // Pool d'items à spawner
    [SerializeField] private int _maxItemsInZone = 2;   // Limite d'items dans la zone
    [SerializeField] private float _minSpawnTime = 2f;  // Délai min entre spawns
    [SerializeField] private float _maxSpawnTime = 4f;  // Délai max entre spawns

    [Header("References")]
    [SerializeField] private LayerMask _itemLayer;     // LayerMask pour filtrer les items (ex: Layer "Item")

    private BoxCollider _spawnZoneCollider;
    private Coroutine _spawnCoroutine;
    private bool _isSpawning = false;

    private void Awake()
    {
        _spawnZoneCollider = GetComponent<BoxCollider>();
        if (_spawnZoneCollider == null)
        {
            Debug.LogError("ItemSpawner: Aucun BoxCollider trouvé sur la SpawnZone !", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        // Écoute l'événement du GameManager
        GameManager.OnRoundStarted += StartSpawning;
    }

    private void OnDisable()
    {
        // Désabonne et arrête la coroutine
        GameManager.OnRoundStarted -= StartSpawning;
        StopSpawning();
    }

    private void StartSpawning()
    {
        if (!isServer) return; // Seulement côté serveur
        if (_isSpawning) return; // Évite les doubles lancements

        _isSpawning = true;
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
        _isSpawning = false;
    }

    private IEnumerator SpawnRoutine()
    {
        while (GameManager.Instance != null &&  GameManager.Instance.IsRoundActive)
        {
            // Attend un délai aléatoire
            float waitTime = Random.Range(_minSpawnTime, _maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // Vérifie si la round est toujours active
            if (GameManager.Instance == null || !GameManager.Instance.IsRoundActive)
                break;

            // Compte les items déjà présents dans la zone
            int currentItemCount = CountItemsInZone();
            if (currentItemCount >= _maxItemsInZone)
                continue; // Ne spawn pas si la zone est pleine

            // Spawn un item aléatoire
            SpawnItem();
        }
        _isSpawning = false;
    }

    private int CountItemsInZone()
    {
        // Centre et taille du collider
        Vector3 center = _spawnZoneCollider.center + transform.position;
        Vector3 halfExtents = _spawnZoneCollider.size * 0.5f;

        // Rotation du collider (pour OverlapBox)
        Quaternion rotation = _spawnZoneCollider.transform.rotation;

        // Détecte tous les colliders sur le layer "Item"
        Collider[] itemColliders = Physics.OverlapBox(
            center,
            halfExtents,
            rotation,
            _itemLayer
        );

        return itemColliders.Length;
    }

    private void SpawnItem()
    {
        if (_itemPrefabs == null || _itemPrefabs.Length == 0)
        {
            Debug.LogError("ItemSpawner: Aucun prefab d'item configuré !", this);
            return;
        }

        // Choisit un prefab aléatoire
        GameObject prefab = _itemPrefabs[Random.Range(0, _itemPrefabs.Length)];

        // Position aléatoire dans le BoxCollider
        Vector3 spawnPosition = GetRandomPositionInCollider();
        Quaternion spawnRotation = Quaternion.identity; // Rotation neutre (ou aléatoire si besoin)

        // Instancie l'item (côté serveur)
        GameObject newItem = Instantiate(prefab, spawnPosition, spawnRotation);

        // Si l'item a un NetworkIdentity, il sera synchronisé automatiquement
        // Debug.Log($"Item spawned: {prefab.name} at {spawnPosition}");
    }

    private Vector3 GetRandomPositionInCollider()
    {
        Vector3 center = _spawnZoneCollider.center + transform.position;
        Vector3 halfExtents = _spawnZoneCollider.size * 0.5f;

        // Génère une position aléatoire dans le volume du collider
        Vector3 randomOffset = new Vector3(
            Random.Range(-halfExtents.x, halfExtents.x),
            Random.Range(-halfExtents.y, halfExtents.y),
            Random.Range(-halfExtents.z, halfExtents.z)
        );

        return center + randomOffset;
    }
}