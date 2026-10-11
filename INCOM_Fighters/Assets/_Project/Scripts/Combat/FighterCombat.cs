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
        // TODO 4a: consuma SEMPRE le pressioni dei due pugni con input.ConsumeLight() e input.ConsumeHeavy().
        //   Quelle arrivate durante un attacco vanno perse: il buffer per le combo arriva nella sessione 4.

        // TODO 4b: se lo stato è Attack, fai avanzare l'attacco con TickAttack() ed esci dal metodo.

        // TODO 4c: altrimenti, se è stato premuto il pugno leggero (e lightAttack non è null) avvia lightAttack;
        //   se è stato premuto il pesante avvia heavyAttack; se nessuno dei due,
        //   State diventa Walk o Idle a seconda di input.Horizontal.
    }

    void StartAttack(AttackData attack)
    {
        // TODO 4d: salva l'attacco in current, azzera frame e metti State a Attack.
        //   Prepara la hitbox con hitbox.Begin(attack) e fai partire l'animazione:
        //   animator.CrossFadeInFixedTime(attack.stateName, crossFade);
        //   Infine chiama TickAttack(): questo è già il frame 1 dell'attacco.
    }

    void TickAttack()
    {
        // TODO 4e: incrementa frame e aggiorna la hitbox:
        //   hitbox.Tick(current.IsActiveFrame(frame), movement.Facing);
        //   Quando frame arriva a current.TotalFrames, chiama EndAttack().
    }

    void EndAttack()
    {
        // TODO 4f: chiudi la hitbox (hitbox.End()), current = null, State = Idle
        //   e torna in guardia con animator.CrossFadeInFixedTime(locomotionState, 0.1f).
    }
}
