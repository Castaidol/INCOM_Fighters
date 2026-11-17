using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD dell'allenamento: la barra vita del manichino e i frame data dell'ultimo attacco del player.
/// Ascolta solo eventi: Health.OnChanged per la barra, FighterCombat e Hitbox per i frame data.
/// Nella sessione 6 si aggiunge il contatore combo.
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

    Hitbox playerHitbox;

    void Awake()
    {
        // TODO 8b (sessione 5): playerHitbox = la Hitbox del player (player.GetComponent<Hitbox>()).
    }

    void OnEnable()
    {
        dummyHealth.OnChanged += ShowDummyHealth;
        // TODO 8b: iscriviti a player.OnAttackStarted (ShowAttack), player.OnWhiff (ShowWhiff)
        //   e playerHitbox.OnContact (ShowContact).
    }

    void OnDisable()
    {
        dummyHealth.OnChanged -= ShowDummyHealth;
        // TODO 8b: togli le tre iscrizioni.
    }

    void Start()
    {
        // All'avvio nessun evento è ancora arrivato: si parte dal valore attuale.
        ShowDummyHealth(dummyHealth.Current, dummyHealth.Max);
        // TODO 8b: scrivi in frameDataText "Tira un pugno per vedere i suoi frame data" e svuota resultText.
    }

    void ShowDummyHealth(int current, int max)
    {
        // (float): con due int la divisione darebbe sempre 0 o 1.
        dummyHealthFill.fillAmount = (float)current / max;
    }

    void ShowAttack(AttackData attack)
    {
        // TODO 8c: in frameDataText su due righe (\n) il nome dell'attacco, startup, active e recovery,
        //   poi "a segno" con Signed(attack.AdvantageOnHit) e "in parata" con Signed(attack.AdvantageOnBlock).
        //   Svuota resultText: l'esito arriva dopo.
    }

    void ShowContact(AttackData attack, HitResult result)
    {
        // TODO 8d: in resultText "A SEGNO" per Hit, "PARATO" per Blocked e "IGNORATO" per Ignored.
        //   Con il rich text puoi colorarli, per esempio "<color=#6FB7FF>PARATO</color>".
    }

    void ShowWhiff(AttackData attack)
    {
        // TODO 8d: in resultText "A VUOTO".
    }

    /// <summary>+5 in verde, -3 in rosso: a colpo d'occhio chi torna libero prima.</summary>
    static string Signed(int frames) => frames switch
    {
        > 0 => $"<color=#7CDB6A>+{frames}</color>",
        < 0 => $"<color=#FF6B6B>{frames}</color>",
        _ => "0",
    };
}
