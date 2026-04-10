using UnityEngine;

/// <summary>
/// Script de la moneda. Afegeix punts a un comptador global al ser recollida i es destrueix.
/// Es requereix que l'objecte tingui un Collider2D en mode Trigger.
/// </summary>
public class Coin : MonoBehaviour
{
    // Comptador global estàtic de punts per a tot el joc
    public static int puntsTotals = 0;

    private void OnTriggerEnter2D(Collider2D altre)
    {
        // Comprovem si ha estat el jugador qui ha tocat la moneda
        if (altre.CompareTag("Player"))
        {
            // Sumem punts al comptador global
            puntsTotals += 10;
            Debug.Log("[Coin] Moneda recollida! Punts actuals: " + puntsTotals);
            
            // Aquí es podria afegir un efecte visual o de so abans de destruir l'objecte
            
            // Destruïm l'objecte moneda perquè desaparegui de l'escena
            Destroy(gameObject);
        }
    }
}
