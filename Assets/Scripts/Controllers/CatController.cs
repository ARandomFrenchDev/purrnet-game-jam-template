using PurrNet;
using UnityEngine;

// PNJ chat : patrouille simple, aucune IA complexe nécessaire pour une jam
public class CatController : NetworkBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float stunDuration = 1.5f;

    private int _targetIndex;

    private void Update()
    {
        if (!isServer || patrolPoints.Length == 0)
            return; // seul le serveur déplace le chat, la position est répliquée via NetworkTransform

        Transform target = patrolPoints[_targetIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            _targetIndex = (_targetIndex + 1) % patrolPoints.Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.Stun(stunDuration);
        }
    }
}