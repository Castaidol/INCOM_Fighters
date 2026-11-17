using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD dell'allenamento: la barra vita del manichino, i frame data dell'ultimo attacco del player,
/// il contatore combo e la scritta PARATO, che svaniscono con una dissolvenza.
/// Ascolta solo eventi: Health.OnChanged, FighterCombat, Hitbox e ComboTracker.
/// </summary>
public class TrainingHUD : MonoBehaviour
{
    [Header("Vita del manichino")]
    [SerializeField] Health dummyHealth;
    [Tooltip("Image con Image Type = Filled e Fill Method = Horizontal (serve una Source Image).")]
    [SerializeField] Image dummyHealthFill;

    [Header("Frame data dell'ultimo attacco")]
    [SerializeField] FighterCombat player;
    [Tooltip("Nome, startup, active, recovery e vantaggio a segno e in parata.")]
    [SerializeField] Text frameDataText;
    [Tooltip("Com'è andato l'ultimo colpo: A SEGNO, PARATO, IGNORATO o A VUOTO.")]
    [SerializeField] Text resultText;

    [Header("Contatore combo")]
    [SerializeField] ComboTracker comboTracker;
    [SerializeField] CanvasGroup comboGroup;
    [SerializeField] Text comboText;

    [Header("Scritta PARATO")]
    [SerializeField] CanvasGroup blockedGroup;

    [Header("Dissolvenza")]
    [Tooltip("Secondi in cui la scritta resta piena prima di svanire.")]
    [SerializeField] float holdSeconds = 1f;
    [Tooltip("Secondi della dissolvenza.")]
    [SerializeField] float fadeSeconds = 0.5f;

    Hitbox playerHitbox;
    bool comboRunning;
    float comboTimer;   // secondi dalla fine della combo
    float blockedTimer; // secondi dall'ultima parata

    void Awake()
    {
        playerHitbox = player.GetComponent<Hitbox>();
    }

    void OnEnable()
    {
        dummyHealth.OnChanged += ShowDummyHealth;
        player.OnAttackStarted += ShowAttack;
        player.OnWhiff += ShowWhiff;
        playerHitbox.OnContact += ShowContact;
        // TODO 6b (sessione 6): iscriviti a comboTracker.OnComboChanged (ShowCombo) e a comboTracker.OnComboEnded (EndCombo).
    }

    void OnDisable()
    {
        dummyHealth.OnChanged -= ShowDummyHealth;
        player.OnAttackStarted -= ShowAttack;
        player.OnWhiff -= ShowWhiff;
        playerHitbox.OnContact -= ShowContact;
        // TODO 6b: togli le due iscrizioni.
    }

    void Start()
    {
        // All'avvio nessun evento è ancora arrivato: si parte dal valore attuale.
        ShowDummyHealth(dummyHealth.Current, dummyHealth.Max);
        frameDataText.text = "Tira un pugno per vedere i suoi frame data";
        resultText.text = "";

        // TODO 6c: contatore e PARATO partono già svaniti: comboTimer e blockedTimer valgono holdSeconds + fadeSeconds
        //   e l'alpha dei due CanvasGroup è 0.
    }

    // TODO 6d: aggiungi Update. Le dissolvenze usano il tempo normale (Time.deltaTime), così l'hitstop non le ferma.
    //   Se la combo non è in corso, comboTimer aumenta e comboGroup.alpha = FadeOut(comboTimer).
    //   blockedTimer aumenta sempre e blockedGroup.alpha = FadeOut(blockedTimer).

    /// <summary>1 per holdSeconds secondi, poi scende a 0 in fadeSeconds secondi.</summary>
    float FadeOut(float seconds)
    {
        // TODO 6d: restituisci 1 meno quanto è avanzata la dissolvenza: (seconds - holdSeconds) / fadeSeconds,
        //   tenuto tra 0 e 1 con Mathf.Clamp01. Prima di holdSeconds vale 1, dopo holdSeconds + fadeSeconds vale 0.
        return 1f;
    }

    void ShowCombo(int hits, int damage)
    {
        // TODO 6e: la combo è in corso: comboRunning vero, comboGroup.alpha a 1 e in comboText i colpi e,
        //   su una seconda riga più piccola, il danno: $"{hits} COLPI\n<size=30>{damage} danni</size>".
    }

    void EndCombo(int hits, int damage)
    {
        // TODO 6e: la combo è finita: comboRunning falso e comboTimer a 0. Il contatore resta ancora un attimo, poi svanisce.
    }

    void ShowDummyHealth(int current, int max)
    {
        // (float): con due int la divisione darebbe sempre 0 o 1.
        dummyHealthFill.fillAmount = (float)current / max;
    }

    void ShowAttack(AttackData attack)
    {
        // Il vantaggio viene dai dati: colpo sul primo frame attivo, hitstun o blockstun meno i frame che restano.
        frameDataText.text =
            $"<b>{attack.name}</b>   startup {attack.startup}   active {attack.active}   recovery {attack.recovery}\n" +
            $"a segno {Signed(attack.AdvantageOnHit)}   in parata {Signed(attack.AdvantageOnBlock)}";
        resultText.text = "";
    }

    void ShowContact(AttackData attack, HitResult result)
    {
        resultText.text = result switch
        {
            HitResult.Hit => "<color=#FFD24D>A SEGNO</color>",
            HitResult.Blocked => "<color=#6FB7FF>PARATO</color>",
            _ => "<color=#A0A0A0>IGNORATO</color>",
        };
        // TODO 6e: se il colpo è parato azzera blockedTimer: la scritta PARATO riappare piena.
    }

    void ShowWhiff(AttackData attack)
    {
        resultText.text = "<color=#A0A0A0>A VUOTO</color>";
    }

    /// <summary>+5 in verde, -3 in rosso: a colpo d'occhio chi torna libero prima.</summary>
    static string Signed(int frames) => frames switch
    {
        > 0 => $"<color=#7CDB6A>+{frames}</color>",
        < 0 => $"<color=#FF6B6B>{frames}</color>",
        _ => "0",
    };
}
