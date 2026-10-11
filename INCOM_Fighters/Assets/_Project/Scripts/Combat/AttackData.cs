using UnityEngine;

/// <summary>
/// Un attacco descritto come dati: tempi in frame (60 al secondo), hitbox e danno.
/// Lo stesso asset funziona con qualsiasi set di animazioni: dell'Animator conosce solo il nome dello stato.
/// </summary>
[CreateAssetMenu(menuName = "INCOM Fighters/Attack Data", fileName = "NuovoAttacco")]
public class AttackData : ScriptableObject
{
    [Tooltip("Nome dello stato dell'Animator che mostra l'attacco.")]
    public string stateName = "Jab";

    [Header("Frame data (60 fps)")]
    [Tooltip("Frame prima che la hitbox si accenda.")]
    [Min(1)] public int startup = 4;
    [Tooltip("Frame in cui la hitbox può colpire.")]
    [Min(1)] public int active = 2;
    [Tooltip("Frame per tornare in guardia dopo i frame attivi.")]
    [Min(0)] public int recovery = 10;

    [Header("Hitbox (metri, dai piedi; X positivo = verso l'avversario)")]
    public Vector3 hitboxOffset = new(0.9f, 1.4f, 0f);
    public Vector3 hitboxSize = new(0.3f, 0.25f, 0.5f);

    [Header("Effetto")]
    [Min(0)] public int damage = 5;

    /// <summary>Durata totale dell'attacco in frame.</summary>
    public int TotalFrames
    {
        get
        {
            // TODO 1a: restituisci la somma di startup, active e recovery.
            return 0;
        }
    }

    /// <summary>True se nel frame indicato la hitbox può colpire.</summary>
    public bool IsActiveFrame(int frame)
    {
        // TODO 1b: i frame si contano da 1. I primi "startup" frame preparano il colpo,
        //   i successivi "active" frame possono colpire: restituisci true solo per quelli.
        //   Esempio con startup 4 e active 2: attivi i frame 5 e 6.
        return false;
    }
}
