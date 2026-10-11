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
        dummyHealth.OnChanged += ShowDummyHealth;
    }

    void OnDisable()
    {
        dummyHealth.OnChanged -= ShowDummyHealth;
    }

    void Start()
    {
        // All'avvio nessun evento è ancora arrivato: si parte dal valore attuale.
        ShowDummyHealth(dummyHealth.Current, dummyHealth.Max);
    }

    void ShowDummyHealth(int current, int max)
    {
        // (float): con due int la divisione darebbe sempre 0 o 1.
        dummyHealthFill.fillAmount = (float)current / max;
    }
}
