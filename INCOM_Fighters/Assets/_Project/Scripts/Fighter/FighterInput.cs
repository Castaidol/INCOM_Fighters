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

    InputAction moveAction;
    InputAction lightAction;
    InputAction heavyAction;
    InputAction resetAction;

    // Input buffer (sessione 4): un solo posto. L'ultimo pugno premuto sostituisce il precedente
    // e resta valido per bufferFrames frame, finché FighterCombat non lo usa.
    AttackButton buffered;
    int bufferAge;

    // Il reset non ha bisogno di buffer: resta in attesa finché qualcuno non lo consuma.
    bool resetQueued;

    /// <summary>Direzione orizzontale nel mondo: -1 sinistra, 0 fermo, +1 destra.</summary>
    public float Horizontal { get; private set; }

    /// <summary>Ultimo pugno premuto e non ancora usato (None se il buffer è vuoto o è scaduto).</summary>
    public AttackButton BufferedButton => buffered;

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

        // TODO 4a (sessione 4): le pressioni non vanno più in lightQueued e heavyQueued ma nel buffer.
        //   Se il pugno leggero è stato premuto in questo frame (WasPressedThisFrame) chiama Buffer(AttackButton.Light);
        //   lo stesso per il pesante con AttackButton.Heavy.
        if (resetAction.WasPressedThisFrame()) resetQueued = true;
    }

    void FixedUpdate()
    {
        // TODO 4c: un FixedUpdate è un frame. Se il buffer non è vuoto (None), aumenta bufferAge di 1;
        //   quando bufferAge supera bufferFrames il pugno è scaduto: rimetti buffered a None.
    }

    void Buffer(AttackButton button)
    {
        // TODO 4b: metti button in buffered e riporta bufferAge a 0 (ogni pressione riparte da capo).
    }

    /// <summary>Svuota il buffer: chi ha usato il pugno lo chiama, così vale una volta sola.</summary>
    public void ConsumeBuffer()
    {
        // TODO 4d: rimetti buffered a None.
    }

    /// <summary>True una sola volta per ogni pressione di Reset (View/Select o R).</summary>
    public bool ConsumeReset()
    {
        bool pressed = resetQueued;
        resetQueued = false;
        return pressed;
    }
}
