using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Input display (facoltativo): gli ultimi comandi del player, dal più recente, con i frame passati
/// dal comando precedente. Serve a vedere il buffer e le combo premute troppo tardi.
/// I frame dell'hitstop non contano, così i numeri si confrontano con i frame data.
/// </summary>
public class InputDisplay : MonoBehaviour
{
    [SerializeField] FighterInput input;
    [SerializeField] FighterMovement movement;
    [SerializeField] HitStop hitStop;
    [SerializeField] Text text;
    [SerializeField, Range(3, 20)] int maxRows = 10;

    readonly List<string> rows = new();
    float lastDirection;  // -1 indietro, 0 fermo, +1 avanti
    int framesSinceLast = 99;

    void OnEnable()
    {
        // TODO 7b (sessione 6): iscriviti a input.OnAttackPressed con il metodo AddAttack.
    }

    void OnDisable()
    {
        // TODO 7b: togli l'iscrizione.
    }

    void Start()
    {
        text.text = "";
    }

    void FixedUpdate()
    {
        // TODO 7c: durante l'hitstop esci subito; altrimenti framesSinceLast aumenta di 1.
        //   La direzione rispetto all'avversario è input.Horizontal * movement.Facing: +1 avanti, -1 indietro, 0 fermo.
        //   Se è diversa da lastDirection, salvala in lastDirection e aggiungi una riga con AddRow:
        //   "→" avanti, "←" indietro, "·" fermo.
    }

    void AddAttack(AttackButton button)
    {
        // TODO 7b: aggiungi una riga "L" per il pugno leggero, "H" per il pesante.
    }

    void AddRow(string command)
    {
        // TODO 7d: inserisci la riga in cima alla lista con rows.Insert(0, ...): il comando e, in grigio, i frame
        //   dal comando precedente ("99+" se framesSinceLast è più di 99), per esempio
        //   $"{command}   <color=#A0A0A0>{frames}</color>".
        //   Se le righe sono più di maxRows togli l'ultima. Poi azzera framesSinceLast e scrivi in text
        //   tutte le righe, una sotto l'altra: string.Join("\n", rows).
    }
}
