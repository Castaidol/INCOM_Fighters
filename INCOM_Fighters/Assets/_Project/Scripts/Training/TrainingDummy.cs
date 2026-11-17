using UnityEngine;

/// <summary>Cosa sta facendo il manichino.</summary>
public enum DummyState { Idle, Hit, Block, KO, GetUp }

/// <summary>
/// Il manichino d'allenamento. È lui a decidere cosa succede a ogni colpo (IHitReceiver): lo subisce,
/// lo para o lo ignora, secondo le opzioni di TrainingSettings. Reagisce con hitstun o blockstun e knockback,
/// va KO e si rialza, e si rigenera se lo si lascia stare.
/// Come per il player, i tempi li decide il codice contando i frame: le clip si limitano a mostrarli.
/// </summary>
// Il manichino conta i suoi frame prima che il player attacchi:
// così il frame in cui arriva il colpo non viene già scalato dall'hitstun.
[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Health), typeof(Pushbox))]
public class TrainingDummy : MonoBehaviour, IHitReceiver
{
    [Header("Riferimenti")]
    [SerializeField] Animator animator;
    [SerializeField] ArenaBounds arena;
    [Tooltip("Opzioni dell'allenamento. Se manca, il manichino non para e non va mai KO.")]
    [SerializeField] TrainingSettings settings;
    [Tooltip("Durante l'hitstop il manichino non conta i suoi frame.")]
    [SerializeField] HitStop hitStop;

    [Header("Animazione")]
    [SerializeField] string idleState = "Idle";
    [SerializeField] string guardState = "Guard";
    [SerializeField] string hitState = "Hit";
    [SerializeField] string blockState = "Block";
    [SerializeField] string koState = "KO";
    [SerializeField] string getUpState = "GetUp";
    [Tooltip("Secondi di dissolvenza per tornare in guardia.")]
    [SerializeField] float crossFade = 0.1f;

    [Header("Reazione")]
    [Tooltip("In quanti frame il manichino percorre il knockback.")]
    [SerializeField, Min(1)] int knockbackFrames = 6;
    [Tooltip("Parte del knockback che resta quando il colpo è parato.")]
    [SerializeField, Range(0f, 1f)] float blockKnockback = 0.5f;
    [Tooltip("Frame senza colpi prima che la vita torni piena (120 = 2 s).")]
    [SerializeField, Min(1)] int regenDelay = 120;

    [Header("Parata e KO")]
    [Tooltip("\"Para dopo il primo colpo\": frame senza contatti dopo cui il manichino abbassa la guardia.")]
    [SerializeField, Min(1)] int guardMemory = 60;
    [Tooltip("Frame a terra dopo il KO, prima di rialzarsi.")]
    [SerializeField, Min(1)] int koFrames = 90;
    [Tooltip("Frame del rialzo: la durata della clip Die01Recover.")]
    [SerializeField, Min(1)] int getUpFrames = 52;

    Health health;
    Pushbox pushbox;

    int hitstunLeft;
    int blockstunLeft;
    int stateFrame;         // frame passati a terra o nel rialzo
    int knockbackLeft;
    float knockbackStep;
    FighterMovement attackerMovement;
    int framesSinceContact; // colpi a segno o parati: servono alla rigenerazione e alla memoria della guardia
    string pose;            // la posa da fermo che sta mostrando: Idle o Guard

    public DummyState State { get; private set; } = DummyState.Idle;

    /// <summary>Colpo a segno. Il bool è true se il manichino era ancora in hitstun: il colpo continua una combo.</summary>
    public event System.Action<AttackData, bool> OnHitTaken;

    /// <summary>Colpo parato.</summary>
    public event System.Action<AttackData> OnBlocked;

    /// <summary>Il manichino non è più in hitstun (è tornato in guardia o è andato KO): la combo, se c'era, è finita.</summary>
    public event System.Action OnRecovered;

    /// <summary>Frame di hitstun ancora da subire.</summary>
    public int HitstunLeft => hitstunLeft;

    /// <summary>Frame di blockstun ancora da subire.</summary>
    public int BlockstunLeft => blockstunLeft;

    DummyBehavior Behavior => settings != null ? settings.Behavior : DummyBehavior.Stand;
    bool CanBeKnockedOut => settings != null && settings.HealthMode == DummyHealthMode.Normal;
    int CounterCap => Mathf.Max(regenDelay, guardMemory);

    /// <summary>True se in questo momento il manichino para i colpi che arrivano fuori dall'hitstun.</summary>
    public bool IsGuarding => Behavior switch
    {
        DummyBehavior.AlwaysBlock => true,
        DummyBehavior.BlockAfterFirstHit => framesSinceContact < guardMemory,
        _ => false,
    };

    void Awake()
    {
        health = GetComponent<Health>();
        pushbox = GetComponent<Pushbox>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        animator.applyRootMotion = false;
        framesSinceContact = CounterCap;
        pose = idleState;
    }

    /// <summary>Chiamato dalla Hurtbox: il manichino decide com'è andato il colpo.</summary>
    public HitResult ReceiveHit(AttackData attack, Transform attacker)
    {
        // A terra e mentre si rialza i colpi non contano.
        if (State == DummyState.KO || State == DummyState.GetUp) return HitResult.Ignored;

        // Lo stato si guarda PRIMA di aggiornarlo: in hitstun non si para, e il colpo continua la combo.
        bool comboHit = State == DummyState.Hit;
        bool blocks = !comboHit && IsGuarding;
        framesSinceContact = 0;

        if (blocks)
        {
            Block(attack, attacker);
            return HitResult.Blocked;
        }

        TakeHit(attack, attacker, comboHit);
        return HitResult.Hit;
    }

    void TakeHit(AttackData attack, Transform attacker, bool comboHit)
    {
        // Con la vita "Infinita" non si scende sotto 1.
        health.MinHealth = CanBeKnockedOut ? 0 : 1;
        health.TakeDamage(attack.damage);
        StartKnockback(attack.knockback, attacker);

        if (health.Current == 0)
        {
            OnHitTaken?.Invoke(attack, comboHit);
            KnockOut();
            return;
        }

        // Un nuovo colpo fa ripartire la reazione: hitstun da capo e animazione dal tempo 0.
        State = DummyState.Hit;
        hitstunLeft = attack.hitstun;
        animator.Play(hitState, 0, 0f);
        OnHitTaken?.Invoke(attack, comboHit);
    }

    void Block(AttackData attack, Transform attacker)
    {
        // Parata: nessun danno, blockstun al posto dell'hitstun e knockback ridotto.
        State = DummyState.Block;
        blockstunLeft = attack.blockstun;
        StartKnockback(attack.knockback * blockKnockback, attacker);
        animator.Play(blockState, 0, 0f);
        OnBlocked?.Invoke(attack);
    }

    void KnockOut()
    {
        State = DummyState.KO;
        stateFrame = 0;
        hitstunLeft = 0;
        blockstunLeft = 0;
        animator.CrossFadeInFixedTime(koState, 0.05f);
        OnRecovered?.Invoke(); // la combo finisce con il KO
    }

    void StartKnockback(float distance, Transform attacker)
    {
        // Il knockback allontana dall'attaccante ed è distribuito su knockbackFrames frame.
        float direction = Mathf.Sign(transform.position.x - attacker.position.x);
        knockbackStep = direction * distance / knockbackFrames;
        knockbackLeft = knockbackFrames;
        attacker.TryGetComponent(out attackerMovement);
    }

    void FixedUpdate()
    {
        // Hitstop: stun, knockback, KO e rigenerazione restano fermi.
        if (hitStop != null && hitStop.IsFrozen) return;

        if (knockbackLeft > 0) ApplyKnockback();

        switch (State)
        {
            case DummyState.Hit:
                // Si torna in guardia quando l'hitstun è finito, anche se la clip GetHit non è arrivata in fondo.
                if (hitstunLeft > 0) hitstunLeft--;
                else
                {
                    ReturnToStance();
                    OnRecovered?.Invoke();
                }
                break;

            case DummyState.Block:
                if (blockstunLeft > 0) blockstunLeft--;
                else ReturnToStance();
                break;

            case DummyState.KO:
                if (++stateFrame >= koFrames)
                {
                    State = DummyState.GetUp;
                    stateFrame = 0;
                    animator.CrossFadeInFixedTime(getUpState, crossFade);
                }
                break;

            case DummyState.GetUp:
                if (++stateFrame >= getUpFrames)
                {
                    // In piedi: vita piena e guardia abbassata.
                    health.RestoreFull();
                    framesSinceContact = CounterCap;
                    ReturnToStance();
                }
                break;

            case DummyState.Idle:
                UpdatePose();
                break;
        }

        // A terra non si conta: al rialzo la vita torna comunque piena.
        if (State == DummyState.KO || State == DummyState.GetUp) return;

        // Frame senza contatti: rigenerazione e memoria della guardia ("Para dopo il primo colpo").
        if (framesSinceContact < CounterCap)
        {
            framesSinceContact++;
            if (framesSinceContact == regenDelay) health.RestoreFull();
        }
    }

    void ReturnToStance()
    {
        State = DummyState.Idle;
        pose = IsGuarding ? guardState : idleState;
        animator.CrossFadeInFixedTime(pose, crossFade);
    }

    void UpdatePose()
    {
        // Da fermo la posa segue la guardia: Defense quando para, Idle01 quando no.
        // Con "Para dopo il primo colpo" la guardia si abbassa dopo guardMemory frame senza contatti.
        string wanted = IsGuarding ? guardState : idleState;
        if (wanted == pose) return;
        pose = wanted;
        animator.CrossFadeInFixedTime(pose, crossFade);
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

    /// <summary>Torna subito in piedi e in guardia, senza stun né knockback. Lo usa il reset del TrainingManager.</summary>
    public void ResetState()
    {
        bool wasHit = State == DummyState.Hit;
        State = DummyState.Idle;
        hitstunLeft = 0;
        blockstunLeft = 0;
        stateFrame = 0;
        knockbackLeft = 0;
        attackerMovement = null;
        framesSinceContact = CounterCap;
        pose = IsGuarding ? guardState : idleState;
        animator.Play(pose, 0, 0f);
        if (wasHit) OnRecovered?.Invoke();
    }
}
