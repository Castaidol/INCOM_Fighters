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

    [Header("Tremolio")] // sessione 6
    [Tooltip("Secondi in cui il tremolio si smorza fino a zero.")]
    [SerializeField] float shakeDuration = 0.2f;

    Vector3 velocity;

    // TODO 3a (sessione 6): tre campi privati: Vector3 basePosition, la posizione senza tremolio (è questa che segue
    //   i combattenti), float shakeStrength e float shakeTimeLeft.
    //   Aggiungi Awake: basePosition parte dalla posizione attuale della camera.

    /// <summary>Fa tremare la camera di "strength" metri; il tremolio si smorza in shakeDuration secondi.</summary>
    public void Shake(float strength)
    {
        // TODO 3b: shakeStrength = strength e shakeTimeLeft = shakeDuration.
        //   (Facoltativo: se è ancora in corso un tremolio più forte, non indebolirlo.)
    }

    void LateUpdate()
    {
        if (fighterA == null || fighterB == null) return;

        float midX = (fighterA.position.x + fighterB.position.x) * 0.5f;
        float span = Mathf.Abs(fighterA.position.x - fighterB.position.x);

        // 0 quando sono vicini, 1 quando sono lontani: la distanza della camera segue.
        float t = Mathf.InverseLerp(nearSpan, farSpan, span);
        float distance = Mathf.Lerp(minDistance, maxDistance, t);

        Vector3 target = new Vector3(midX, height, -distance);
        // TODO 3c: lo SmoothDamp deve muovere basePosition, non la camera.
        //   Poi, se shakeTimeLeft è maggiore di 0, scalalo di Time.deltaTime (il tremolio va in tempo reale e continua
        //   durante l'hitstop) e calcola fade = Mathf.Clamp01(shakeTimeLeft / shakeDuration), che scende da 1 a 0.
        //   Lo spostamento è (Vector3)(Random.insideUnitCircle * (shakeStrength * fade)); senza tremolio è zero.
        //   La camera va in basePosition + spostamento.
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
    }
}
