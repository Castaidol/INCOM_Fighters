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
        input.OnAttackPressed += AddAttack;
    }

    void OnDisable()
    {
        input.OnAttackPressed -= AddAttack;
    }

    void Start()
    {
        text.text = "";
    }

    void FixedUpdate()
    {
        if (hitStop != null && hitStop.IsFrozen) return;
        framesSinceLast++;

        // La direzione si mostra rispetto all'avversario: → avanti, ← indietro, · fermo.
        float direction = input.Horizontal * movement.Facing;
        if (direction == lastDirection) return;
        lastDirection = direction;
        AddRow(direction > 0f ? "→" : direction < 0f ? "←" : "·");
    }

    void AddAttack(AttackButton button)
    {
        AddRow(button == AttackButton.Light ? "L" : "H");
    }

    void AddRow(string command)
    {
        string frames = framesSinceLast > 99 ? "99+" : framesSinceLast.ToString();
        rows.Insert(0, $"{command}   <color=#A0A0A0>{frames}</color>");
        if (rows.Count > maxRows) rows.RemoveAt(rows.Count - 1);
        framesSinceLast = 0;
        text.text = string.Join("\n", rows);
    }
}
