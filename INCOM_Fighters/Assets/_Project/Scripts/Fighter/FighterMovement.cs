using UnityEngine;

/// <summary>
/// Movimento 2.5D: il combattente si sposta solo lungo X, resta dentro l'arena,
/// guarda sempre l'avversario e non lo attraversa.
/// Niente fisica e niente root motion: la posizione la decide solo questo script.
/// </summary>
[RequireComponent(typeof(FighterInput), typeof(Pushbox))]
public class FighterMovement : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] Transform opponent;
    [SerializeField] ArenaBounds arena;
    [SerializeField] Animator animator;

    [Header("Movimento")]
    [Tooltip("Velocità di camminata in m/s.")]
    [SerializeField] float walkSpeed = 2f;

    [Header("Animazione")]
    [Tooltip("Tempo di smorzamento del parametro MoveX: evita scatti tra Idle e camminata.")]
    [SerializeField] float animDamping = 0.08f;

    static readonly int MoveXHash = Animator.StringToHash("MoveX");

    FighterInput input;
    Pushbox pushbox;
    Pushbox opponentPushbox;

    /// <summary>+1 se l'avversario è a destra, -1 se è a sinistra.</summary>
    public float Facing { get; private set; } = 1f;

    void Awake()
    {
        input = GetComponent<FighterInput>();
        pushbox = GetComponent<Pushbox>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (opponent != null) opponent.TryGetComponent(out opponentPushbox);

        // La posizione la gestisce il codice: il root motion delle clip va ignorato.
        animator.applyRootMotion = false;
    }

    void Update()
    {
        // TODO 6a: chiama UpdateFacing() per girarti verso l'avversario.

        // TODO 4a: sposta il combattente lungo X.
        //   - parti da una copia di transform.position
        //   - aggiungi a x: input.Horizontal * walkSpeed * Time.deltaTime
        //   - limita x con ClampToArena(...)
        //   - (passo 7) limita x anche con ResolvePushbox(...)
        //   - forza z a 0: siamo in 2.5D
        //   - riassegna transform.position

        // TODO 6c: aggiorna il parametro MoveX dell'Animator con lo smorzamento:
        //   animator.SetFloat(MoveXHash, valore, animDamping, Time.deltaTime)
        //   Il valore deve essere +1 quando cammini VERSO l'avversario e -1 quando ti allontani,
        //   da qualunque lato dello schermo tu sia. Ti servono input.Horizontal e Facing.
    }

    void UpdateFacing()
    {
        // TODO 6b: se opponent esiste, calcola dx = x dell'avversario - x del combattente.
        //   Se |dx| > 0.01, Facing = Mathf.Sign(dx).
        //   Poi ruota il personaggio (il modello guarda lungo +Z) verso +X o -X:
        //   transform.rotation = Quaternion.LookRotation(Vector3.right * Facing, Vector3.up);
    }

    float ClampToArena(float x)
    {
        // TODO 4b: se arena esiste, restituisci x limitato (Mathf.Clamp) tra
        //   arena.MinX + pushbox.HalfWidth  e  arena.MaxX - pushbox.HalfWidth.
        return x;
    }

    float ResolvePushbox(float x)
    {
        // TODO 7: se opponentPushbox esiste, i centri dei due combattenti devono restare
        //   ad almeno pushbox.HalfWidth + opponentPushbox.HalfWidth di distanza.
        //   Se guardi a destra (Facing > 0) sei a sinistra dell'avversario: x non può superare
        //   opponent.position.x meno quella distanza. Se guardi a sinistra vale il contrario.
        return x;
    }
}
