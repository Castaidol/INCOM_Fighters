using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unico script del gioco che conosce l'Input System.
/// Legge la action map "Fighter" dal PlayerInput e la traduce in comandi semplici:
/// gli altri componenti li usano senza sapere se arrivano da joypad, tastiera o (in futuro) da un'IA.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class FighterInput : MonoBehaviour
{
    [Tooltip("Sotto questo valore lo stick è considerato fermo.")]
    [SerializeField, Range(0f, 0.9f)] float deadZone = 0.4f; // SFIDA S1: dead zone regolabile

    InputAction moveAction;
    InputAction lightAction;
    InputAction heavyAction;

    // Le pressioni arrivano in Update, ma il combattimento le usa in FixedUpdate:
    // restano "in attesa" finché qualcuno non le consuma.
    bool lightQueued;
    bool heavyQueued;

    /// <summary>Direzione orizzontale nel mondo: -1 sinistra, 0 fermo, +1 destra.</summary>
    public float Horizontal { get; private set; }

    void Awake()
    {
        // PlayerInput crea una copia dell'asset per ogni giocatore: si leggono le azioni da lì.
        InputActionAsset actions = GetComponent<PlayerInput>().actions;
        moveAction = actions.FindAction("Move", throwIfNotFound: true);
        lightAction = actions.FindAction("LightPunch", throwIfNotFound: true);
        heavyAction = actions.FindAction("HeavyPunch", throwIfNotFound: true);
    }

    void Update()
    {
        float x = moveAction.ReadValue<float>();

        // In un picchiaduro la camminata è digitale: o si cammina a velocità piena o si sta fermi.
        Horizontal = Mathf.Abs(x) < deadZone ? 0f : Mathf.Sign(x);

        if (lightAction.WasPressedThisFrame()) lightQueued = true;
        if (heavyAction.WasPressedThisFrame()) heavyQueued = true;
    }

    /// <summary>True una sola volta per ogni pressione del pugno leggero.</summary>
    public bool ConsumeLight()
    {
        bool pressed = lightQueued;
        lightQueued = false;
        return pressed;
    }

    /// <summary>True una sola volta per ogni pressione del pugno pesante.</summary>
    public bool ConsumeHeavy()
    {
        bool pressed = heavyQueued;
        heavyQueued = false;
        return pressed;
    }
}
