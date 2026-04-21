using UnityEngine;

/// <summary>
/// Script de la moneda. Afegeix punts a un comptador global al ser recollida i es destrueix.
/// Es requereix que l'objecte tingui un Collider2D en mode Trigger.
/// </summary>
public class Coin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D altre)
    {
        // Comprovem si ha estat el jugador qui ha tocat la moneda
        if (altre.CompareTag("Player"))
        {
            // Sumem punts via l'ScoreManager si existeix
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RecollirMoneda(10);
            }
            
            // Destruïm l'objecte moneda perquè desaparegui de l'escena
            Destroy(gameObject);
        }
    }
}
