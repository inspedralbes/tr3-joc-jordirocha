using UnityEngine;

/// <summary>
/// Controlador de la càmera: segueix el jugador de forma suau usant SmoothDamp.
/// Adjunta aquest script a l'objecte Camera de l'escena.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Objectiu")]
    [Tooltip("Arrossega aquí el Transform del jugador.")]
    public Transform objectiuJugador;

    [Header("Suavitzat")]
    [Tooltip("Temps de suavitzat: com més alt, més lenta és la càmera.")]
    public float tempsSuavitzat = 0.15f;

    [Header("Desplaçament")]
    [Tooltip("Offset opcional per centrar/desplaçar la càmera respecte al jugador.")]
    public Vector3 desplacament = new Vector3(0f, 2f, -10f);

    // Velocitat interna que SmoothDamp actualitza automàticament
    private Vector3 velocitatActual = Vector3.zero;

    void LateUpdate()
    {
        // Si no hi ha objectiu assignat, no fem res
        if (objectiuJugador == null) return;

        // Posició destí = posició del jugador + offset configurat
        Vector3 posicioDesti = objectiuJugador.position + desplacament;

        // Suavitzem el moviment de la càmera cap a la posició destí
        transform.position = Vector3.SmoothDamp(
            transform.position,
            posicioDesti,
            ref velocitatActual,
            tempsSuavitzat
        );
    }
}
