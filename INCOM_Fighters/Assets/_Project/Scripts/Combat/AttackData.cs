using UnityEngine;

/// <summary>
/// Un attacco descritto come dati: tempi in frame (60 al secondo), hitbox, danno, reazione di chi lo subisce
/// e i colpi che possono seguirlo in combo.
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
    [Tooltip("Frame in cui chi viene colpito resta bloccato nella reazione.")]
    [Min(0)] public int hitstun = 16;
    // TODO 1a (sessione 5): il campo blockstun (int, [Min(0)], 10 di default, con un [Tooltip]):
    //   frame in cui chi para resta bloccato nella parata.
    [Tooltip("Metri di cui chi viene colpito arretra.")]
    [Min(0f)] public float knockback = 0.15f;

    [Header("Combo")]
    [Tooltip("Frame da cui un pugno nel buffer fa partire il colpo successivo; la finestra dura fino all'ultimo frame. 0 = nessuna finestra.")]
    [Min(0)] public int comboWindowStart;
    [Tooltip("Colpo che segue premendo il pugno leggero nella finestra (vuoto = nessuno).")]
    public AttackData nextOnLight;
    [Tooltip("Colpo che segue premendo il pugno pesante nella finestra (vuoto = nessuno).")]
    public AttackData nextOnHeavy;

    /// <summary>Durata totale dell'attacco in frame.</summary>
    public int TotalFrames => startup + active + recovery;

    /// <summary>I frame si contano da 1: i primi "startup" preparano, poi "active" frame possono colpire.</summary>
    public bool IsActiveFrame(int frame) => frame > startup && frame <= startup + active;

    // TODO 1b: tre proprietà calcolate, con il colpo a segno sul primo frame attivo:
    //   - public int FramesAfterHit => frame dell'attaccante dopo il colpo: active - 1 + recovery
    //   - public int AdvantageOnHit => hitstun - FramesAfterHit
    //   - public int AdvantageOnBlock => blockstun - FramesAfterHit

    /// <summary>True dal frame comboWindowStart fino all'ultimo frame dell'attacco.</summary>
    public bool IsInComboWindow(int frame) => comboWindowStart > 0 && frame >= comboWindowStart && frame <= TotalFrames;

    /// <summary>Il colpo che segue con quel tasto, o null se l'albero si ferma qui.</summary>
    public AttackData NextFor(AttackButton button) => button switch
    {
        AttackButton.Light => nextOnLight,
        AttackButton.Heavy => nextOnHeavy,
        _ => null,
    };
}
