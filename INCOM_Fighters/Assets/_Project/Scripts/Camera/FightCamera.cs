using UnityEngine;

/// <summary>
/// Camera laterale del 2.5D: guarda lungo +Z, segue il punto medio tra i due combattenti
/// e si allontana quando loro si allontanano.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FightCamera : MonoBehaviour
{
    [SerializeField] Transform fighterA;
    [SerializeField] Transform fighterB;

    [Header("Inquadratura")]
    [SerializeField] float height = 1.2f;
    [SerializeField] float smoothTime = 0.15f;

    [Header("Zoom in base alla distanza")] // SFIDA S1
    [SerializeField] float minDistance = 6f;
    [SerializeField] float maxDistance = 12f;
    [SerializeField] float nearSpan = 2f;
    [SerializeField] float farSpan = 8f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (fighterA == null || fighterB == null) return;

        float midX = (fighterA.position.x + fighterB.position.x) * 0.5f;
        float span = Mathf.Abs(fighterA.position.x - fighterB.position.x);

        // 0 quando sono vicini, 1 quando sono lontani: la distanza della camera segue.
        float t = Mathf.InverseLerp(nearSpan, farSpan, span);
        float distance = Mathf.Lerp(minDistance, maxDistance, t);

        Vector3 target = new Vector3(midX, height, -distance);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
    }
}
