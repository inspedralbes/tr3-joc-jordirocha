using UnityEngine;

/// <summary>
/// Checkpoint per desar la posició de reaparició del jugador quan hi passa pel damunt.
/// Requereix un Collider2D en mode Trigger.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    private bool activat = false;

    private void OnTriggerEnter2D(Collider2D altre)
    {
        // Comprovem si ha estat el jugador i si no està activat encara
        if (!activat && altre.CompareTag("Player"))
        {
            activat = true;
            Debug.Log("[Checkpoint] Has arribat a un checkpoint! Nova posició desada: " + transform.position);
            
            // Actualitzem la variable global en DeathZone per establir el nou punt d'inici
            DeathZone.posicioReaparicioActiva = transform.position;

            // Aquí pots afegir un efecte de partícules o canviar el color/sprite del Checkpoint per indicar que s'ha activat
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Canvia a color verd quan s'activa com a feeback visual bàsic
                sr.color = Color.green;
            }
        }
    }
}
