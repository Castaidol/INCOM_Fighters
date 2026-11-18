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
    Vector3 basePosition; // posizione senza tremolio: è questa che segue i combattenti
    float shakeStrength;
    float shakeTimeLeft;

    void Awake()
    {
        basePosition = transform.position;
    }

    /// <summary>Fa tremare la camera di "strength" metri; il tremolio si smorza in shakeDuration secondi.</summary>
    public void Shake(float strength)
    {
        float current = shakeTimeLeft > 0f ? shakeStrength * shakeTimeLeft / shakeDuration : 0f;
        shakeStrength = Mathf.Max(strength, current);
        shakeTimeLeft = shakeDuration;
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
        basePosition = Vector3.SmoothDamp(basePosition, target, ref velocity, smoothTime);

        // Il tremolio gira in tempo reale: continua anche durante l'hitstop, che ferma solo i combattenti.
        // TODO 8 (sessione 7, dopo il playtest): in pausa Time.deltaTime è 0 e shakeTimeLeft non scende più, ma questo
        //   blocco calcola lo stesso uno spostamento casuale a ogni frame: la camera vibra dietro al menu.
        //   Fai partire il tremolio solo se anche Time.deltaTime è maggiore di 0.
        Vector3 offset = Vector3.zero;
        if (shakeTimeLeft > 0f)
        {
            shakeTimeLeft -= Time.deltaTime;
            float fade = Mathf.Clamp01(shakeTimeLeft / shakeDuration);
            offset = (Vector3)(Random.insideUnitCircle * (shakeStrength * fade));
        }
        transform.position = basePosition + offset;
    }
}
