using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Tasto d'attacco nel buffer.</summary>
public enum AttackButton { None, Light, Heavy }

/// <summary>
/// Unico script del gioco che conosce l'Input System.
/// Legge la action map "Fighter" dal PlayerInput e la traduce in comandi semplici:
/// gli altri componenti li usano senza sapere se arrivano da joypad, tastiera o (in futuro) da un'IA.
/// </summary>
// Il buffer invecchia prima che FighterCombat lo legga: ogni pugno resta valido esattamente bufferFrames frame.
[DefaultExecutionOrder(-20)]
[RequireComponent(typeof(PlayerInput))]
public class FighterInput : MonoBehaviour
{
    [Tooltip("Sotto questo valore lo stick è considerato fermo.")]
    [SerializeField, Range(0f, 0.9f)] float deadZone = 0.4f; // SFIDA S1: dead zone regolabile

    [Tooltip("Per quanti frame (60 al secondo) un pugno premuto resta valido.")]
    [SerializeField, Range(1, 20)] int bufferFrames = 8;

    [Tooltip("Durante l'hitstop il buffer non invecchia: il pugno premuto resta valido per quando il gioco riparte.")]
    [SerializeField] HitStop hitStop;

    InputAction moveAction;
    InputAction lightAction;
    InputAction heavyAction;
    InputAction resetAction;

    // Input buffer: un solo posto. L'ultimo pugno premuto sostituisce il precedente
    // e resta valido per bufferFrames frame, finché FighterCombat non lo usa.
    AttackButton buffered;
    int bufferAge;

    // Il reset non ha bisogno di buffer: resta in attesa finché qualcuno non lo consuma.
    bool resetQueued;

    /// <summary>Direzione orizzontale nel mondo: -1 sinistra, 0 fermo, +1 destra.</summary>
    public float Horizontal { get; private set; }

    /// <summary>Ultimo pugno premuto e non ancora usato (None se il buffer è vuoto o è scaduto).</summary>
    public AttackButton BufferedButton => buffered;

    /// <summary>È stato premuto un pugno (per l'input display).</summary>
    public event System.Action<AttackButton> OnAttackPressed;

    void Awake()
    {
        // PlayerInput crea una copia dell'asset per ogni giocatore: si leggono le azioni da lì.
        InputActionAsset actions = GetComponent<PlayerInput>().actions;
        moveAction = actions.FindAction("Move", throwIfNotFound: true);
        lightAction = actions.FindAction("LightPunch", throwIfNotFound: true);
        heavyAction = actions.FindAction("HeavyPunch", throwIfNotFound: true);
        resetAction = actions.FindAction("Reset", throwIfNotFound: true);
    }

    void Update()
    {
        float x = moveAction.ReadValue<float>();

        // In un picchiaduro la camminata è digitale: o si cammina a velocità piena o si sta fermi.
        Horizontal = Mathf.Abs(x) < deadZone ? 0f : Mathf.Sign(x);

        // Le pressioni arrivano in Update; il combattimento le legge in FixedUpdate dal buffer.
        if (lightAction.WasPressedThisFrame()) Buffer(AttackButton.Light);
        if (heavyAction.WasPressedThisFrame()) Buffer(AttackButton.Heavy);
        if (resetAction.WasPressedThisFrame()) resetQueued = true;
    }

    void FixedUpdate()
    {
        // Un FixedUpdate è un frame: il pugno nel buffer invecchia di uno e dopo bufferFrames scade.
        if (buffered == AttackButton.None) return;
        if (hitStop != null && hitStop.IsFrozen) return;
        bufferAge++;
        if (bufferAge > bufferFrames) buffered = AttackButton.None;
    }

    void Buffer(AttackButton button)
    {
        buffered = button;
        bufferAge = 0;
        OnAttackPressed?.Invoke(button);
    }

    /// <summary>Svuota il buffer: chi ha usato il pugno lo chiama, così vale una volta sola.</summary>
    public void ConsumeBuffer()
    {
        buffered = AttackButton.None;
    }

    /// <summary>True una sola volta per ogni pressione di Reset (View/Select o R).</summary>
    public bool ConsumeReset()
    {
        bool pressed = resetQueued;
        resetQueued = false;
        return pressed;
    }
}
