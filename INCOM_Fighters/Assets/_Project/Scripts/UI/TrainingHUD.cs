using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD dell'allenamento. Per ora la barra vita del manichino, che ascolta Health.OnChanged:
/// non sa chi ha tolto o ridato vita, le basta sapere quanta ce n'è.
/// Nelle sessioni 5 e 6 si aggiungono frame data e contatore combo.
/// </summary>
public class TrainingHUD : MonoBehaviour
{
    [Header("Vita del manichino")]
    [SerializeField] Health dummyHealth;
    [Tooltip("Image con Image Type = Filled e Fill Method = Horizontal (serve una Source Image).")]
    [SerializeField] Image dummyHealthFill;

    void OnEnable()
    {
        // TODO 8a: iscriviti a dummyHealth.OnChanged con ShowDummyHealth.
    }

    void OnDisable()
    {
        // TODO 8a: togli l'iscrizione.
    }

    void Start()
    {
        // All'avvio nessun evento è ancora arrivato: si parte dal valore attuale.
        ShowDummyHealth(dummyHealth.Current, dummyHealth.Max);
    }

    void ShowDummyHealth(int current, int max)
    {
        // TODO 8b: imposta dummyHealthFill.fillAmount alla frazione di vita rimasta, da 0 a 1.
        //   Attenzione: tra due int la divisione dà sempre 0 o 1, serve un (float).
    }
}
