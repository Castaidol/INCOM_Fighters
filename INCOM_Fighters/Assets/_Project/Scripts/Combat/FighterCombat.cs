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

        // TODO 5a (sessione 4): libero, un pugno nel buffer fa partire l'attacco di partenza di quel tasto.
        //   AttackData attack = AttackFor(input.BufferedButton);
        //   Se attack non è null: input.ConsumeBuffer() e StartAttack(attack).
        //   Altrimenti, come prima, Walk o Idle secondo input.Horizontal (la riga qui sotto).
        State = input.Horizontal != 0f ? FighterState.Walk : FighterState.Idle;
    }

    /// <summary>L'attacco di partenza per quel tasto, o null.</summary>
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

        // TODO 5b: finestra di combo. Se current.IsInComboWindow(frame) e hitbox.HasHit sono veri,
        //   chiedi all'attacco il seguito per il tasto nel buffer: current.NextFor(input.BufferedButton).
        //   Se c'è: input.ConsumeBuffer(), StartAttack(seguito) e return (il resto del recovery si salta).
        //   Va fatto PRIMA del controllo qui sotto, che chiude l'attacco all'ultimo frame.

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
