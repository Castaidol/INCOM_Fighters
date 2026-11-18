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
        // TODO 5b (sessione 7): parti dalle due radici dell'albero: Follow(player.LightAttack, "L", "", "", 0, 0)
        //   e lo stesso con player.HeavyAttack e "H". Poi scrivi in text tutte le combo separate da una riga vuota,
        //   string.Join("\n\n", combos), oppure "Nessuna combo negli AttackData." se la lista è vuota.
    }

    /// <summary>
    /// Aggiunge un attacco alla combo arrivata fin qui (tasti, nomi, danno, colpi) e prosegue con i suoi seguiti:
    /// è una funzione ricorsiva, che chiama sé stessa per ogni ramo dell'albero.
    /// Quando l'attacco non ha seguiti la combo è finita: se ha almeno due colpi entra nella lista.
    /// </summary>
    void Follow(AttackData attack, string key, string keys, string names, int damage, int hits)
    {
        if (attack == null) return;
        // TODO 5c: aggiungi l'attacco alla combo arrivata fin qui. Se è il primo colpo (hits == 0) keys diventa key
        //   e names diventa attack.name; altrimenti keys + " " + key e names + " → " + attack.name.
        //   damage aumenta di attack.damage e hits di 1.
        //   I seguiti sono attack.NextFor(AttackButton.Light) e attack.NextFor(AttackButton.Heavy). Se non ce ne sono,
        //   o se hits ha raggiunto maxHits, la combo è finita: se ha almeno 2 colpi aggiungi a combos
        //   $"<b>{keys}</b>   <color=#A0A0A0>{damage} danni</color>\n{names}" ed esci.
        //   Altrimenti chiama Follow per ognuno dei due seguiti, con "L" e con "H": è la ricorsione.
    }
}
