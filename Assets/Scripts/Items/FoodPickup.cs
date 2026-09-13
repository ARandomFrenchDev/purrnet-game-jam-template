using PurrNet;
using UnityEngine;
// à tester dans une scène vide sans Purrnet
public class FoodPickup : NetworkBehaviour
{
    [SerializeField] private int scoreValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (other.TryGetComponent<FoodPickup>(out var f))
        {
            Debug.Log(f);
        }

        if (other.TryGetComponent<PlayerController>(out var player))
        {
            GameManager.Instance.AddScoreServer(player, scoreValue);
            Despawn();
        }
    }
}