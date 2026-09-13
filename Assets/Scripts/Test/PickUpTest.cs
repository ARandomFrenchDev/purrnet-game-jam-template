using PurrNet;
using UnityEngine;
// à tester dans une scène vide sans Purrnet
public class PickUpTest : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger detected with {other.name}");
    }

    private void OnDrawGizmos()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
}
}