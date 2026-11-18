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
        // Update gira anche con Time.timeScale = 0: è FixedUpdate che si ferma.
        if (playerInput.ConsumePause())
        {
            if (IsOpen) Close();
            else Open();
        }
    }

    void OnDestroy()
    {
        // Se la scena si chiude con il menu aperto, il tempo non deve restare fermo.
        if (IsOpen) Time.timeScale = 1f;
    }

    public void Open()
    {
        IsOpen = true;
        Time.timeScale = 0f; // al contrario dell'hitstop qui deve fermarsi tutto: frame, animazioni, particelle
        playerInput.SetMenuMode(true);
        panel.SetActive(true);
        RefreshLabels();
        EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }

    public void Close()
    {
        IsOpen = false;
        Time.timeScale = 1f;
        playerInput.SetMenuMode(false);
        panel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    // --- Pulsanti: ognuno è collegato al suo metodo nell'OnClick dell'Inspector ---

    public void CycleBehavior()
    {
        settings.Behavior = (DummyBehavior)Next((int)settings.Behavior, typeof(DummyBehavior));
        RefreshLabels();
    }

    public void CycleHealth()
    {
        settings.HealthMode = (DummyHealthMode)Next((int)settings.HealthMode, typeof(DummyHealthMode));
        RefreshLabels();
    }

    public void ToggleHitboxes()
    {
        settings.ShowHitboxes = !settings.ShowHitboxes;
        RefreshLabels();
    }

    public void Restart()
    {
        manager.ResetTraining();
        Close();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        // Nell'editor Application.Quit non fa niente: si ferma il Play.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void RefreshLabels()
    {
        behaviorLabel.text = $"Comportamento: {BehaviorName(settings.Behavior)}";
        healthLabel.text = $"Vita: {HealthName(settings.HealthMode)}";
        hitboxLabel.text = $"Hitbox: {(settings.ShowHitboxes ? "Sì" : "No")}";
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
