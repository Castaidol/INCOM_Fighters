using UnityEngine;

/// <summary>
/// Limiti orizzontali dell'arena, in coordinate mondo.
/// Li leggono il player (sessione 1), il manichino per il knockback (sessione 3) e chiunque ne abbia bisogno.
/// </summary>
public class ArenaBounds : MonoBehaviour
{
    [SerializeField] float minX = -6f;
    [SerializeField] float maxX = 6f;

    public float MinX => minX;
    public float MaxX => maxX;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(minX, 0f, 0f), new Vector3(minX, 3f, 0f));
        Gizmos.DrawLine(new Vector3(maxX, 0f, 0f), new Vector3(maxX, 3f, 0f));
        Gizmos.DrawLine(new Vector3(minX, 0f, 0f), new Vector3(maxX, 0f, 0f));
    }
}
