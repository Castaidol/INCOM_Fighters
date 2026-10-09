using UnityEngine;

/// <summary>
/// Ingombro orizzontale del combattente: due pushbox non possono sovrapporsi.
/// Non usa la fisica: è solo una misura che FighterMovement legge.
/// </summary>
public class Pushbox : MonoBehaviour
{
    [Tooltip("Metà della larghezza, in metri.")]
    [SerializeField, Min(0.05f)] float halfWidth = 0.35f;

    [Tooltip("Altezza, usata solo per disegnare il Gizmo.")]
    [SerializeField, Min(0.1f)] float height = 1.8f;

    public float HalfWidth => halfWidth;

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.9f);
        Vector3 center = transform.position + Vector3.up * (height * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(halfWidth * 2f, height, 0.3f));
    }
}
