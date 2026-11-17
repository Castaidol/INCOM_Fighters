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
        playerHitbox = player.GetComponent<Hitbox>();
    }

    void OnEnable()
    {
        dummyHealth.OnChanged += ShowDummyHealth;
        player.OnAttackStarted += ShowAttack;
        player.OnWhiff += ShowWhiff;
        playerHitbox.OnContact += ShowContact;
    }

    void OnDisable()
    {
        dummyHealth.OnChanged -= ShowDummyHealth;
        player.OnAttackStarted -= ShowAttack;
        player.OnWhiff -= ShowWhiff;
        playerHitbox.OnContact -= ShowContact;
    }

    void Start()
    {
        // All'avvio nessun evento è ancora arrivato: si parte dal valore attuale.
        ShowDummyHealth(dummyHealth.Current, dummyHealth.Max);
        frameDataText.text = "Tira un pugno per vedere i suoi frame data";
        resultText.text = "";
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
