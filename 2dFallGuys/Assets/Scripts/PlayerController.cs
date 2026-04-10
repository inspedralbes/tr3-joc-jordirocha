using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Moviment Bàsic")]
    public float velocitat = 8f;
    private float movimentX;
    private bool mirantDreta = true; // Per controlar cap a on mira l'sprite

    [Header("Salts")]
    public float forcaSalt = 12f;
    public int saltsMaxims = 2;
    private int saltsRestants;

    [Header("Mecànica de Dash")]
    public float forcaDash = 20f;
    public float tempsDash = 0.2f;
    public float tempsRecarregaDash = 1f;
    private bool potFerDash = true;
    private bool fentDash = false;

    [Header("Detecció del Terra")]
    public Transform puntTerra;
    public float radiTerra = 0.2f;
    public LayerMask capaTerra;
    private bool tocaTerra;

    // --- REFERÈNCIES ---
    private Rigidbody2D rb;
    private Animator animator; // Referència a l'Animator

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Agafem el component
        saltsRestants = saltsMaxims;
    }

    void Update()
    {
        if (fentDash) return;

        // 1. Obtenir l'input amb el Nou Sistema
        movimentX = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movimentX = 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movimentX = -1f;
        }

        // --- ANIMACIONS: Girar l'sprite segons la direcció ---
        if (movimentX > 0 && !mirantDreta) GirarPersonatge();
        else if (movimentX < 0 && mirantDreta) GirarPersonatge();

        // 2. Comprovar terra i recarregar salts
        tocaTerra = Physics2D.OverlapCircle(puntTerra.position, radiTerra, capaTerra);
        if (tocaTerra && rb.linearVelocity.y <= 0) 
        {
            saltsRestants = saltsMaxims;
        }

        // 3. Executar Salt
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && saltsRestants > 0)
        {
            FerSalt();
        }

        // 4. Executar Dash
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame && potFerDash)
        {
            StartCoroutine(FerDashCoroutine());
        }

        // --- ANIMACIONS: Enviar paràmetres a l'Animator ---
        animator.SetFloat("velocitatX", Mathf.Abs(movimentX));
        animator.SetBool("tocaTerra", tocaTerra);
        animator.SetFloat("velocitatY", rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        if (fentDash) return;
        rb.linearVelocity = new Vector2(movimentX * velocitat, rb.linearVelocity.y);
    }

    private void FerSalt()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); 
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaSalt);
        saltsRestants--;
    }

    private IEnumerator FerDashCoroutine()
    {
        potFerDash = false;
        fentDash = true;
        
        animator.SetTrigger("dash"); // --- ANIMACIONS: Activar dash ---

        float gravetatOriginal = rb.gravityScale;
        rb.gravityScale = 0f;
        
        float direccio = mirantDreta ? 1f : -1f; // Fem dash cap a on mira el personatge
        rb.linearVelocity = new Vector2(direccio * forcaDash, 0f);

        yield return new WaitForSeconds(tempsDash);

        rb.gravityScale = gravetatOriginal;
        fentDash = false;

        yield return new WaitForSeconds(tempsRecarregaDash);
        potFerDash = true;
    }

    // --- MÈTODE PER GIRAR L'SPRITE ---
    private void GirarPersonatge()
    {
        mirantDreta = !mirantDreta;
        Vector3 escalaLocal = transform.localScale;
        escalaLocal.x *= -1f; // Invertim l'eix X
        transform.localScale = escalaLocal;
    }
}