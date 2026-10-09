using UnityEngine;

/// <summary>
/// Camera laterale del 2.5D: guarda lungo +Z e segue il punto medio tra i due combattenti.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FightCamera : MonoBehaviour
{
    [SerializeField] Transform fighterA;
    [SerializeField] Transform fighterB;

    [Header("Inquadratura")]
    [SerializeField] float height = 1.2f;
    [SerializeField] float distance = 7f;
    [SerializeField] float smoothTime = 0.15f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (fighterA == null || fighterB == null) return;

        // TODO 8: calcola midX, il punto medio sull'asse X tra fighterA e fighterB.
        //   Il punto da raggiungere è (midX, height, -distance).
        //   Muovi la camera con Vector3.SmoothDamp(transform.position, punto, ref velocity, smoothTime).
        //   Domanda: perché questo codice sta in LateUpdate e non in Update?
    }
}
