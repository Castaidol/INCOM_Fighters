using UnityEngine;

/// <summary>Cosa sta facendo il combattente.</summary>
public enum FighterState { Idle, Walk, Attack }

/// <summary>
/// Esegue gli attacchi contando i frame in FixedUpdate (Fixed Timestep = 1/60: un FixedUpdate = un frame).
/// L'Animator mostra soltanto: è il codice a decidere quando l'attacco inizia, quando colpisce e quando finisce.
/// Le combo seguono l'albero scritto negli AttackData (nextOnLight, nextOnHeavy).
/// </summary>
[RequireComponent(typeof(FighterInput), typeof(FighterMovement), typeof(Hitbox))]
public class FighterCombat : MonoBehaviour
{
    [Header("Attacchi di partenza")]
    [SerializeField] AttackData lightAttack;
    [SerializeField] AttackData heavyAttack;

    [Header("Animazione")]
    [SerializeField] Animator animator;
    [SerializeField] string locomotionState = "Locomotion";
    [Tooltip("Secondi di dissolvenza tra le animazioni.")]
    [SerializeField] float crossFade = 0.05f;

    FighterInput input;
    FighterMovement movement;
    Hitbox hitbox;

    AttackData current;
    int frame;

    public FighterState State { get; private set; } = FighterState.Idle;

    /// <summary>Attacco in corso (null se non sta attaccando).</summary>
    public AttackData CurrentAttack => current;

    /// <summary>Frame dell'attacco in corso, da 1 a TotalFrames.</summary>
    public int CurrentFrame => frame;

    // TODO 8a (sessione 5): due eventi pubblici di tipo System.Action<AttackData> per l'HUD:
    //   - OnAttackStarted, lanciato in StartAttack subito dopo il CrossFade (anche dentro una combo)
    //   - OnWhiff, lanciato all'inizio di EndAttack se hitbox.HasHit è falso (l'attacco non ha toccato niente)

    void Awake()
    {
        input = GetComponent<FighterInput>();
        movement = GetComponent<FighterMovement>();
        hitbox = GetComponent<Hitbox>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        if (State == FighterState.Attack)
        {
            TickAttack();
            return;
        }

        // Libero: un pugno nel buffer fa partire l'attacco di partenza di quel tasto.
        AttackData attack = AttackFor(input.BufferedButton);
        if (attack != null)
        {
            input.ConsumeBuffer();
            StartAttack(attack);
        }
        else State = input.Horizontal != 0f ? FighterState.Walk : FighterState.Idle;
    }

    AttackData AttackFor(AttackButton button) => button switch
    {
        AttackButton.Light => lightAttack,
        AttackButton.Heavy => heavyAttack,
        _ => null,
    };

    void StartAttack(AttackData attack)
    {
        current = attack;
        frame = 0;
        State = FighterState.Attack;
        hitbox.Begin(attack);
        animator.CrossFadeInFixedTime(attack.stateName, crossFade);
        TickAttack(); // questo è già il frame 1 dell'attacco
    }

    void TickAttack()
    {
        frame++;
        hitbox.Tick(current.IsActiveFrame(frame), movement.Facing);

        // Finestra di combo: se il colpo ha toccato e nel buffer c'è un tasto con un seguito,
        // il colpo successivo parte subito e il resto del recovery si salta.
        if (current.IsInComboWindow(frame) && hitbox.HasHit)
        {
            AttackData next = current.NextFor(input.BufferedButton);
            if (next != null)
            {
                input.ConsumeBuffer();
                StartAttack(next);
                return;
            }
        }

        if (frame >= current.TotalFrames) EndAttack();
    }

    void EndAttack()
    {
        hitbox.End();
        current = null;
        State = FighterState.Idle;
        animator.CrossFadeInFixedTime(locomotionState, 0.1f);
    }
}
