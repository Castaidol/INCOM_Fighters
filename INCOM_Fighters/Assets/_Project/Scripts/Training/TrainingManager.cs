using UnityEngine;

/// <summary>
/// Regole della modalità allenamento. Per ora il reset: View/Select (o R) rimette player e manichino
/// dove erano all'inizio, con la vita piena. Nella sessione 7 lo stesso metodo lo chiamerà il menu.
/// </summary>
public class TrainingManager : MonoBehaviour
{
    [SerializeField] FighterInput playerInput;
    [SerializeField] TrainingDummy dummy;

    Transform player;
    Health dummyHealth;
    Vector3 playerStart;
    Vector3 dummyStart;

    void Awake()
    {
        player = playerInput.transform;
        dummyHealth = dummy.GetComponent<Health>();
        playerStart = player.position;
        dummyStart = dummy.transform.position;
    }

    void Update()
    {
        if (playerInput.ConsumeReset()) ResetTraining();
    }

    public void ResetTraining()
    {
        player.position = playerStart;
        dummy.transform.position = dummyStart;
        dummy.ResetState();
        dummyHealth.RestoreFull();
        Debug.Log("Allenamento: reset delle posizioni e della vita");
    }
}
