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

    // TODO 2a (sessione 2): aggiungi due campi InputAction per LightPunch e HeavyPunch
    //   e due bool, lightQueued e heavyQueued, per le pressioni in attesa.
    //   Le pressioni arrivano in Update, ma il combattimento le usa in FixedUpdate:
    //   restano "in attesa" finché qualcuno non le consuma.

    /// <summary>Direzione orizzontale nel mondo: -1 sinistra, 0 fermo, +1 destra.</summary>
    public float Horizontal { get; private set; }

    void Awake()
    {
        // PlayerInput crea una copia dell'asset per ogni giocatore: si leggono le azioni da lì.
        InputActionAsset actions = GetComponent<PlayerInput>().actions;
        moveAction = actions.FindAction("Move", throwIfNotFound: true);
        // TODO 2b: cerca anche le azioni "LightPunch" e "HeavyPunch".
    }

    void Update()
    {
        float x = moveAction.ReadValue<float>();

        // In un picchiaduro la camminata è digitale: o si cammina a velocità piena o si sta fermi.
        Horizontal = Mathf.Abs(x) < deadZone ? 0f : Mathf.Sign(x);

        // TODO 2c: se l'azione del pugno leggero è stata premuta in questo frame (WasPressedThisFrame),
        //   metti lightQueued a true. Lo stesso per il pesante.
    }

    /// <summary>True una sola volta per ogni pressione del pugno leggero.</summary>
    public bool ConsumeLight()
    {
        // TODO 2d: restituisci il valore di lightQueued e rimettilo a false,
        //   così ogni pressione viene usata una sola volta.
        return false;
    }

    /// <summary>True una sola volta per ogni pressione del pugno pesante.</summary>
    public bool ConsumeHeavy()
    {
        // TODO 2d: come ConsumeLight, ma con heavyQueued.
        return false;
    }
}
