using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lista delle combo, generata dagli AttackData: parte dagli attacchi di partenza del player e segue
/// l'albero nextOnLight / nextOnHeavy fino in fondo. Ogni ramo che finisce è una combo.
/// Se si aggiunge un attacco o si cambia un seguito, la lista si aggiorna da sola.
/// </summary>
public class ComboList : MonoBehaviour
{
    [SerializeField] FighterCombat player;
    [SerializeField] Text text;
    [Tooltip("Colpi massimi di una combo: ferma la ricerca se un attacco rimanda a uno precedente.")]
    [SerializeField, Range(2, 10)] int maxHits = 6;

    readonly List<string> combos = new();

    void Start()
    {
        combos.Clear();
        Follow(player.LightAttack, "L", "", "", 0, 0);
        Follow(player.HeavyAttack, "H", "", "", 0, 0);
        text.text = combos.Count > 0 ? string.Join("\n\n", combos) : "Nessuna combo negli AttackData.";
    }

    /// <summary>
    /// Aggiunge un attacco alla combo arrivata fin qui (tasti, nomi, danno, colpi) e prosegue con i suoi seguiti:
    /// è una funzione ricorsiva, che chiama sé stessa per ogni ramo dell'albero.
    /// Quando l'attacco non ha seguiti la combo è finita: se ha almeno due colpi entra nella lista.
    /// </summary>
    void Follow(AttackData attack, string key, string keys, string names, int damage, int hits)
    {
        if (attack == null) return;
        keys = hits == 0 ? key : $"{keys} {key}";
        names = hits == 0 ? attack.name : $"{names} → {attack.name}";
        damage += attack.damage;
        hits++;

        AttackData onLight = attack.NextFor(AttackButton.Light);
        AttackData onHeavy = attack.NextFor(AttackButton.Heavy);
        if ((onLight == null && onHeavy == null) || hits >= maxHits)
        {
            if (hits >= 2) combos.Add($"<b>{keys}</b>   <color=#A0A0A0>{damage} danni</color>\n{names}");
            return;
        }
        Follow(onLight, "L", keys, names, damage, hits);
        Follow(onHeavy, "H", keys, names, damage, hits);
    }
}
