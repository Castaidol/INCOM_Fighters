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

    [Header("Velocità (m/s)")]
    [SerializeField] float forwardSpeed = 2.2f;
    [SerializeField] float backSpeed = 1.7f; // SFIDA S1: all'indietro si cammina più piano

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
        UpdateFacing();

        // Direzione relativa all'avversario: +1 avanti, -1 indietro, 0 fermo.
        float relative = input.Horizontal * Facing;
        float speed = relative >= 0f ? forwardSpeed : backSpeed;

        Vector3 pos = transform.position;
        pos.x += input.Horizontal * speed * Time.deltaTime;
        pos.x = ClampToArena(pos.x);
        pos.x = ResolvePushbox(pos.x);
        pos.z = 0f; // l'asse Z è bloccato: siamo in 2.5D
        transform.position = pos;

        // Il blend tree Locomotion usa MoveX: -1 WalkBack, 0 Idle, +1 WalkFwd.
        animator.SetFloat(MoveXHash, relative, animDamping, Time.deltaTime);
    }

    void UpdateFacing()
    {
        if (opponent == null) return;

        float dx = opponent.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f) Facing = Mathf.Sign(dx);

        // Il modello guarda lungo +Z: lo ruotiamo verso +X o -X.
        transform.rotation = Quaternion.LookRotation(Vector3.right * Facing, Vector3.up);
    }

    float ClampToArena(float x)
    {
        if (arena == null) return x;
        return Mathf.Clamp(x, arena.MinX + pushbox.HalfWidth, arena.MaxX - pushbox.HalfWidth);
    }

    float ResolvePushbox(float x)
    {
        if (opponentPushbox == null) return x;

        // I due centri devono restare ad almeno questa distanza.
        float minDistance = pushbox.HalfWidth + opponentPushbox.HalfWidth;
        float ox = opponent.position.x;

        // Chi guarda a destra sta a sinistra dell'avversario, e viceversa.
        return Facing > 0f ? Mathf.Min(x, ox - minDistance) : Mathf.Max(x, ox + minDistance);
    }
}
