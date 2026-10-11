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
        // TODO 5a: iscriviti all'evento OnHit della hurtbox, con HandleHit come metodo da chiamare.
    }

    void OnDisable()
    {
        // TODO 5a: togli l'iscrizione (-=), come fa DamageLogger con Health.
    }

    /// <summary>Chiamato dalla Hurtbox a ogni colpo ricevuto.</summary>
    void HandleHit(AttackData attack, Transform attacker)
    {
        // TODO 5b: un nuovo colpo fa ripartire la reazione:
        //   - State = DummyState.Hit
        //   - hitstunLeft = l'hitstun dell'attacco
        //   - framesSinceHit = 0 (serve alla rigenerazione, passo 7)
        //   - animator.Play(hitState, 0, 0f): Play dal tempo 0, non CrossFade,
        //     così due colpi ravvicinati mostrano due reazioni.

        // TODO 6a: il knockback allontana dall'attaccante ed è distribuito su knockbackFrames frame:
        //   - direzione = Mathf.Sign(transform.position.x - attacker.position.x)
        //   - knockbackStep = direzione * knockback dell'attacco / knockbackFrames
        //   - knockbackLeft = knockbackFrames
    }

    void FixedUpdate()
    {
        if (knockbackLeft > 0) ApplyKnockback();

        // TODO 5c: se lo stato è Hit: finché hitstunLeft è maggiore di 0, togline uno;
        //   quando è già 0, State = DummyState.Idle e animator.CrossFadeInFixedTime(idleState, crossFade).
        //   Si torna in guardia anche se la clip GetHit non è finita: decide il contatore.

        // TODO 7: rigenerazione. Se framesSinceHit è minore di regenDelay, aumentalo di 1;
        //   quando diventa uguale a regenDelay chiama health.RestoreFull().
    }

    void ApplyKnockback()
    {
        knockbackLeft--;

        // TODO 6b: sposta il manichino di knockbackStep lungo X, senza uscire dall'arena:
        //   Mathf.Clamp tra arena.MinX + pushbox.HalfWidth e arena.MaxX - pushbox.HalfWidth
        //   (se arena è null, niente Clamp).
    }

    /// <summary>Torna subito in guardia, senza hitstun né knockback. Lo usa il reset del TrainingManager.</summary>
    public void ResetState()
    {
        State = DummyState.Idle;
        hitstunLeft = 0;
        knockbackLeft = 0;
        framesSinceHit = regenDelay;
        animator.Play(idleState, 0, 0f);
    }
}
