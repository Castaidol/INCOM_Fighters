using UnityEngine;

/// <summary>Cosa sta facendo il combattente.</summary>
public enum FighterState { Idle, Walk, Attack }

/// <summary>
/// Esegue gli attacchi contando i frame in FixedUpdate (Fixed Timestep = 1/60: un FixedUpdate = un frame).
/// L'Animator mostra soltanto: è il codice a decidere quando l'attacco inizia, quando colpisce e quando finisce.
/// </summary>
[RequireComponent(typeof(FighterInput), typeof(FighterMovement), typeof(Hitbox))]
public class FighterCombat : MonoBehaviour
{
    [Header("Attacchi")]
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
        // Le pressioni si consumano a ogni frame: quelle arrivate durante un attacco vanno perse.
        // Nella sessione 4 un buffer le terrà per qualche frame, per le combo.
        bool light = input.ConsumeLight();
        bool heavy = input.ConsumeHeavy();

        if (State == FighterState.Attack)
        {
            TickAttack();
            return;
        }

        if (light && lightAttack != null) StartAttack(lightAttack);
        else if (heavy && heavyAttack != null) StartAttack(heavyAttack);
        else State = input.Horizontal != 0f ? FighterState.Walk : FighterState.Idle;
    }

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
