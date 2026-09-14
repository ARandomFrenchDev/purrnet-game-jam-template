using System.Collections;
using PurrNet;
using UnityEngine;

public class CatController : NetworkBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float stunDuration = 1.5f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private SpriteRenderer childSprite; // sprite sur le GameObject enfant

    private Transform _targetPoint;
    private bool _isDespawning;

    public void Initialize(Transform[] patrolPoints)
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            _targetPoint = patrolPoints[Random.Range(0, patrolPoints.Length)];
    }

    private void Update()
    {
        if (!isServer || _isDespawning || _targetPoint == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, _targetPoint.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetPoint.position) < 0.1f)
            BeginDespawnSequence(); // arrivé à destination -> despawn normal
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || _isDespawning)
            return;

        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.Stun(stunDuration);
            BeginDespawnSequence(); // baston -> despawn anticipé
        }
    }

    private void BeginDespawnSequence()
    {
        if (_isDespawning)
            return;

        _isDespawning = true;
        PlayFadeOutSequence();
        Invoke(nameof(DespawnNow), fadeDuration);
    }

    [ObserversRpc]
    private void PlayFadeOutSequence()
    {
        if (boxCollider != null)
            boxCollider.enabled = false;

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (childSprite == null)
            yield break;

        float t = 0f;
        Color startColor = childSprite.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            childSprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    private void DespawnNow()
    {
        if (isServer)
            Destroy(gameObject);
    }
}