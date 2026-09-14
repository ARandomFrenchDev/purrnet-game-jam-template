using Unity.Cinemachine;
using PurrNet;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Camera (Cinemachine Target Group)")]
    [SerializeField] private float targetWeight = 1f;
    [SerializeField] private float targetRadius = 1f;

    [Header("Bark (ralentit les autres)")]
    [SerializeField] private float barkRadius = 3f;
    [SerializeField] private float barkSlowFactor = 0.5f;
    [SerializeField] private float barkSlowDuration = 2f;
    [SerializeField] private float barkCooldown = 3f;

    [Header("Prout (boost soi-même)")]
    [SerializeField] private float boostMultiplier = 1.8f;
    [SerializeField] private float boostDuration = 1.5f;
    [SerializeField] private float boostCooldown = 4f;

    // État réseauté : tout le monde doit voir le même effet de vitesse/stun
    private readonly SyncVar<float> _speedMultiplier = new(1f);
    private readonly SyncVar<bool> _isStunned = new(false);

    private CinemachineTargetGroup _targetGroup;

    private float _nextBarkTime;
    private float _nextBoostTime;

    protected override void OnSpawned(bool asServer)
    {
        if (asServer)
            GameManager.Instance.RegisterPlayer();

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

        if(GameManager.Instance._roundActive)
        {
            HandleMovement();  
        }


        if (Input.GetKeyDown(KeyCode.E) && Time.time >= _nextBarkTime)
        {
            _nextBarkTime = Time.time + barkCooldown;
            RequestBark();
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= _nextBoostTime)
        {
            _nextBoostTime = Time.time + boostCooldown;
            RequestBoost();
        }
    }

    private void HandleMovement()
    {
        if (_isStunned.value)
            return; // bloqué par un chat

        Vector2 input = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float speed = moveSpeed * _speedMultiplier.value;
        transform.position += (Vector3)input.normalized * speed * Time.deltaTime;
    }

    [ServerRpc]
    private void RequestBark()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, barkRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerController>(out var other) && other != this)
            {
                other.ApplySlow(barkSlowFactor, barkSlowDuration);
            }
        }
    }

    private void ApplySlow(float factor, float duration)
    {
        if (!isServer)
            return;

        _speedMultiplier.value = factor;
        Invoke(nameof(ResetSpeed), duration);
    }

    private void ResetSpeed()
    {
        if (isServer)
            _speedMultiplier.value = 1f;
    }

    [ServerRpc]
    private void RequestBoost()
    {
        _speedMultiplier.value = boostMultiplier;
        Invoke(nameof(ResetSpeed), boostDuration);
    }

    // Appelé par CatController quand ce joueur touche un chat
    public void Stun(float duration)
    {
        if (!isServer)
            return;

        _isStunned.value = true;
        Invoke(nameof(EndStun), duration);
    }

    private void EndStun()
    {
        if (isServer)
            _isStunned.value = false;
    }
}