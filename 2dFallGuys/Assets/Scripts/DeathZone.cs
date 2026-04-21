using UnityEngine;

/// <summary>
/// Zona de mort: detecta quan el jugador (tag "Player") xoca amb pinxos o forats.
/// Quan hi cau, el jugador reapareix a la posició inicial definida.
/// Adjunta aquest script a cada zona perillosa (Trigger 2D activat).
/// </summary>
public class DeathZone : MonoBehaviour
{
    [Header("Punt de Reaparició Per Defecte")]
    [Tooltip("Posició on reapareixerà el jugador en morir si no ha agafat cap checkpoint.")]
    public Vector3 posicioInici = new Vector3(0f, 1f, 0f);

    // Variable global estàtica que els Checkpoints i LevelGenerator actualitzaran
    public static Vector3? posicioReaparicioActiva = null;

    /// <summary>
    /// S'executa automàticament quan un altre collider entra al trigger.
    /// </summary>
    /// <param name="altre">El collider que ha entrat a la zona de mort.</param>
    private void OnTriggerEnter2D(Collider2D altre)
    {
        // Comprovem si l'objecte que ha entrat té el tag "Player"
        if (altre.CompareTag("Player"))
        {
            RespawnarJugador(altre.gameObject);
        }
    }

    /// <summary>
    /// Trasllada el jugador a la posició inicial o a l'últim checkpoint, i reinicia la seva velocitat.
    /// </summary>
    /// <param name="jugador">El GameObject del jugador.</param>
    private void RespawnarJugador(GameObject jugador)
    {
        // Determinem el destí: utilitzem el checkpoint si existeix, sinó la posició inicial per defecte.
        Vector3 desti = posicioReaparicioActiva ?? posicioInici;
        jugador.transform.position = desti;

        // Si el jugador té Rigidbody2D, reiniciem la velocitat per evitar moviment residual
        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log("[DeathZone] El jugador ha mort i ha reaparegut a: " + desti);
    }
    
    /// <summary>
    /// Mètode opcional per esborrar l'últim checkpoint guardat (ex: en canviar d'escena)
    /// </summary>
    public static void NetejarCheckpoint()
    {
        posicioReaparicioActiva = null;
    }
}
