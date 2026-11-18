using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Menu di pausa dell'allenamento. Start o Esc lo apre: il gioco si ferma con Time.timeScale = 0
/// e il joypad passa alla mappa "UI" per muoversi tra i pulsanti. B, Start o Esc lo chiudono.
/// Le righe delle opzioni sono pulsanti che fanno scorrere i valori di TrainingSettings:
/// il menu non tiene una copia delle opzioni, le scrive dove le leggono tutti gli altri.
/// </summary>
public class TrainingMenu : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] FighterInput playerInput;
    [SerializeField] TrainingSettings settings;
    [SerializeField] TrainingManager manager;

    [Header("Interfaccia")]
    [Tooltip("Il pannello del menu: acceso solo durante la pausa.")]
    [SerializeField] GameObject panel;
    [Tooltip("Pulsante selezionato all'apertura: senza, il joypad non sa da dove partire.")]
    [SerializeField] Button firstButton;
    [SerializeField] Text behaviorLabel;
    [SerializeField] Text healthLabel;
    [SerializeField] Text hitboxLabel;

    /// <summary>True mentre il menu è aperto e il gioco è in pausa.</summary>
    public bool IsOpen { get; private set; }

    void Start()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        // TODO 4b (sessione 7): Update gira anche con Time.timeScale = 0 (è FixedUpdate che si ferma).
        //   Se playerInput.ConsumePause() è vero, chiudi il menu se è aperto, altrimenti aprilo.
    }

    void OnDestroy()
    {
        // Se la scena si chiude con il menu aperto, il tempo non deve restare fermo.
        if (IsOpen) Time.timeScale = 1f;
    }

    public void Open()
    {
        // TODO 4c: IsOpen vero e Time.timeScale a 0: al contrario dell'hitstop, qui deve fermarsi tutto.
        //   Poi playerInput.SetMenuMode(true), pannello acceso, RefreshLabels() e, per il joypad, il primo pulsante
        //   selezionato: EventSystem.current.SetSelectedGameObject(firstButton.gameObject).
    }

    public void Close()
    {
        // TODO 4d: il contrario di Open: IsOpen falso, Time.timeScale a 1, playerInput.SetMenuMode(false),
        //   pannello spento e nessun pulsante selezionato: EventSystem.current.SetSelectedGameObject(null).
    }

    // --- Pulsanti: ognuno va collegato al suo metodo nell'OnClick dell'Inspector ---

    public void CycleBehavior()
    {
        // TODO 4e: settings.Behavior diventa il valore successivo, poi RefreshLabels().
        //   Next lavora con gli int: (DummyBehavior)Next((int)settings.Behavior, typeof(DummyBehavior)).
    }

    public void CycleHealth()
    {
        // TODO 4e: lo stesso per settings.HealthMode, con DummyHealthMode.
    }

    public void ToggleHitboxes()
    {
        // TODO 4e: inverti settings.ShowHitboxes, poi RefreshLabels().
    }

    public void Restart()
    {
        // TODO 4e: manager.ResetTraining(), poi chiudi il menu.
    }

    public void Quit()
    {
        // TODO 4e: nella build Application.Quit(). Nell'editor Application.Quit non fa niente, quindi lì si ferma il Play:
        //   #if UNITY_EDITOR
        //       UnityEditor.EditorApplication.isPlaying = false;
        //   #else
        //       Application.Quit();
        //   #endif
    }

    void RefreshLabels()
    {
        // TODO 4f: scrivi i valori attuali nei tre Text: behaviorLabel.text = $"Comportamento: {BehaviorName(settings.Behavior)}",
        //   healthLabel.text con "Vita: " e HealthName(settings.HealthMode), hitboxLabel.text con "Hitbox: Sì" o "Hitbox: No".
    }

    /// <summary>Il valore successivo di un enum: dopo l'ultimo si torna al primo.</summary>
    static int Next(int value, System.Type enumType) => (value + 1) % System.Enum.GetValues(enumType).Length;

    static string BehaviorName(DummyBehavior behavior) => behavior switch
    {
        DummyBehavior.Stand => "Fermo",
        DummyBehavior.AlwaysBlock => "Para sempre",
        DummyBehavior.BlockAfterFirstHit => "Para dopo il primo colpo",
        _ => behavior.ToString(),
    };

    static string HealthName(DummyHealthMode mode) => mode switch
    {
        DummyHealthMode.Infinite => "Infinita",
        DummyHealthMode.Normal => "Normale",
        _ => mode.ToString(),
    };
}
