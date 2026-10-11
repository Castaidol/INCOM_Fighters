using UnityEngine;

/// <summary>Cosa sta facendo il manichino. Nella sessione 5 arrivano parata e KO.</summary>
public enum DummyState { Idle, Hit }

/// <summary>
/// Il manichino d'allenamento: reagisce ai colpi con hitstun e knockback e si rigenera se lo si lascia stare.
/// Come per il player, i tempi li decide il codice contando i frame: la clip GetHit si limita a mostrarli.
/// </summary>
// Il manichino conta i suoi frame prima che il player attacchi:
// così il frame in cui arriva il colpo non viene già scalato dall'hitstun.
[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Health), typeof(Pushbox))]
public class TrainingDummy : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] Animator animator;
    [SerializeField] ArenaBounds arena;

    [Header("Animazione")]
    [SerializeField] string idleState = "Idle";
    [SerializeField] string hitState = "Hit";
    [Tooltip("Secondi di dissolvenza per tornare in Idle a fine hitstun.")]
    [SerializeField] float crossFade = 0.1f;

    [Header("Reazione")]
    [Tooltip("In quanti frame il manichino percorre il knockback.")]
    [SerializeField, Min(1)] int knockbackFrames = 6;
    [Tooltip("Frame senza colpi prima che la vita torni piena (120 = 2 s).")]
    [SerializeField, Min(1)] int regenDelay = 120;

    Health health;
    Pushbox pushbox;
    Hurtbox hurtbox;

    int hitstunLeft;
    int knockbackLeft;
    float knockbackStep;
    FighterMovement attackerMovement;
    int framesSinceHit;

    public DummyState State { get; private set; } = DummyState.Idle;

    /// <summary>Frame di hitstun ancora da subire.</summary>
    public int HitstunLeft => hitstunLeft;

    void Awake()
    {
        health = GetComponent<Health>();
        pushbox = GetComponent<Pushbox>();
        hurtbox = GetComponentInChildren<Hurtbox>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        animator.applyRootMotion = false;
        framesSinceHit = regenDelay;
    }

    void OnEnable()
    {
        hurtbox.OnHit += HandleHit;
    }

    void OnDisable()
    {
        hurtbox.OnHit -= HandleHit;
    }

    void HandleHit(AttackData attack, Transform attacker)
    {
        // Un nuovo colpo fa ripartire tutto: hitstun, knockback e animazione.
        State = DummyState.Hit;
        hitstunLeft = attack.hitstun;
        framesSinceHit = 0;

        // Il knockback allontana dall'attaccante ed è distribuito sui primi frame dell'hitstun.
        float direction = Mathf.Sign(transform.position.x - attacker.position.x);
        knockbackStep = direction * attack.knockback / knockbackFrames;
        knockbackLeft = knockbackFrames;
        attacker.TryGetComponent(out attackerMovement);

        // Play dal tempo 0: due colpi ravvicinati mostrano due reazioni, non una sola.
        animator.Play(hitState, 0, 0f);
    }

    void FixedUpdate()
    {
        if (knockbackLeft > 0) ApplyKnockback();

        if (State == DummyState.Hit)
        {
            // Finché restano frame di hitstun se ne consuma uno; quando sono finiti si torna in guardia,
            // anche se la clip GetHit non è arrivata in fondo.
            if (hitstunLeft > 0) hitstunLeft--;
            else
            {
                State = DummyState.Idle;
                animator.CrossFadeInFixedTime(idleState, crossFade);
            }
        }

        // Rigenerazione: se lo si lascia stare per regenDelay frame, la vita torna piena.
        if (framesSinceHit < regenDelay)
        {
            framesSinceHit++;
            if (framesSinceHit == regenDelay) health.RestoreFull();
        }
    }

    void ApplyKnockback()
    {
        knockbackLeft--;

        Vector3 pos = transform.position;
        float wanted = pos.x + knockbackStep;
        pos.x = arena != null
            ? Mathf.Clamp(wanted, arena.MinX + pushbox.HalfWidth, arena.MaxX - pushbox.HalfWidth)
            : wanted;
        transform.position = pos;

        // SFIDA S3: se il muro ferma il manichino, la spinta che avanza respinge l'attaccante.
        float blocked = wanted - pos.x;
        if (blocked != 0f && attackerMovement != null) attackerMovement.Push(-blocked);
    }

    /// <summary>Torna subito in guardia, senza hitstun né knockback. Lo usa il reset del TrainingManager.</summary>
    public void ResetState()
    {
        State = DummyState.Idle;
        hitstunLeft = 0;
        knockbackLeft = 0;
        attackerMovement = null;
        framesSinceHit = regenDelay;
        animator.Play(idleState, 0, 0f);
    }
}
