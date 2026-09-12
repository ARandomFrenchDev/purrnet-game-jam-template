using PurrNet;
using UnityEngine;

// PurrNet : on hérite de NetworkBehaviour au lieu de MonoBehaviour
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Vector2 _input;

    private void OnSpawned()
    {
        Debug.Log(isOwner);
    }
    private void Update()
    {
        // isOwner = true uniquement sur le client qui possède ce joueur
        // Sans ce check, tout le monde bougerait tous les joueurs à la fois
        if (!isOwner)
            return;

        _input.x = Input.GetAxisRaw("Horizontal");
        _input.y = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(_input.x, _input.y, 0f).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}