using Unity.Cinemachine;
using PurrNet;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float targetWeight = 1f;
    [SerializeField] private float targetRadius = 1f;

    private CinemachineTargetGroup _targetGroup;

    protected override void OnSpawned(bool asServer)
    {
        if (asServer)
            return;

        _targetGroup = FindObjectOfType<CinemachineTargetGroup>();
        if (_targetGroup != null)
            _targetGroup.AddMember(transform, targetWeight, targetRadius);
    }

    protected override void OnDespawned(bool asServer)
    {
        if (asServer)
            return;

        if (_targetGroup != null)
            _targetGroup.RemoveMember(transform);
    }

    private void Update()
    {
        if (!isOwner)
            return;

        Vector2 input = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        transform.position += (Vector3)input.normalized * moveSpeed * Time.deltaTime;
    }
}