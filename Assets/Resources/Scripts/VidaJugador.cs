using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // <─── Añadido para el control de escenas
using System.Collections;

public class VidaJugador : MonoBehaviour, IDanable
{
    // ─── Configuración ────────────────────────────────────────────────
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 5;
    [SerializeField] private int vidaActual;

    [Header("Invencibilidad tras recibir daño")]
    [Tooltip("Segundos de invencibilidad después de recibir un golpe.")]
    [SerializeField] private float tiempoInvencible = 1f;
    [Tooltip("Velocidad del parpadeo visual durante la invencibilidad.")]
    [SerializeField] private float velocidadParpadeo = 0.1f;

    [Header("Eventos")]
    [Tooltip("Se invoca al recibir daño. Pasa la vida actual.")]
    public UnityEvent<int> onRecibirDano;
    [Tooltip("Se invoca al curarse. Pasa la vida actual.")]
    public UnityEvent<int> onCurarse;
    [Tooltip("Se invoca cuando la vida llega a 0.")]
    public UnityEvent onMorir;

    // ─── Estado interno ───────────────────────────────────────────────
    private bool esInvencible = false;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    // ─── Propiedades públicas (solo lectura) ──────────────────────────
    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool EstaVivo => vidaActual > 0;
    public bool EsInvencible => esInvencible;

    // ===================================================================
    //  INICIALIZACIÓN
    // ===================================================================

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        vidaActual = vidaMaxima;
    }

    // ===================================================================
    //  INTERFAZ IDanable
    // ===================================================================

    public void RecibirDano(int cantidad)
    {
        RecibirDano(cantidad, Vector2.zero);
    }

    public void RecibirDano(int cantidad, Vector2 direccionAtaque)
    {
        if (esInvencible || !EstaVivo) return;
        if (cantidad <= 0) return;

        vidaActual = Mathf.Max(vidaActual - cantidad, 0);

        Debug.Log($"[Jugador] Recibió {cantidad} de daño. Vida: {vidaActual}/{vidaMaxima}");

        onRecibirDano?.Invoke(vidaActual);

        if (anim != null)
        {
            if (direccionAtaque != Vector2.zero)
            {
                anim.SetFloat("LastInputX", direccionAtaque.x);
                anim.SetFloat("LastInputY", direccionAtaque.y);
            }
            anim.SetTrigger("Hurt");
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(RutinaInvencibilidad());
        }
    }

    // ===================================================================
    //  CURACIÓN
    // ===================================================================

    public void Curar(int cantidad)
    {
        if (!EstaVivo || cantidad <= 0) return;

        vidaActual = Mathf.Min(vidaActual + cantidad, vidaMaxima);
        Debug.Log($"[Jugador] Se curó {cantidad}. Vida: {vidaActual}/{vidaMaxima}");
        onCurarse?.Invoke(vidaActual);
    }

    public void RestaurarVidaCompleta()
    {
        vidaActual = vidaMaxima;
        onCurarse?.Invoke(vidaActual);
    }

    // ===================================================================
    //  MUERTE (MODIFICADO)
    // ===================================================================

    void Morir()
    {
        Debug.Log("[Jugador] ¡Ha muerto! Reiniciando nivel...");

        // Notificar a otros sistemas por si acaso
        onMorir?.Invoke();

        // Obtiene el índice de la escena activa y la vuelve a cargar desde cero
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ===================================================================
    //  INVENCIBILIDAD CON PARPADEO
    // ===================================================================

    IEnumerator RutinaInvencibilidad()
    {
        esInvencible = true;

        if (spriteRenderer != null)
        {
            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < tiempoInvencible)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(velocidadParpadeo);
                tiempoTranscurrido += velocidadParpadeo;
            }

            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(tiempoInvencible);
        }

        esInvencible = false;
    }
}