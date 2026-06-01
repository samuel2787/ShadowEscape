using UnityEngine;
using UnityEngine.Events;
public class MovimientoGuardia : MonoBehaviour
{
    // ─── Estado del Guardia ────────────────────────────────────────────
    public enum EstadoGuardia { Idle, Patrullando, Persiguiendo, Atacando }
    [Header("Estado Actual (solo lectura)")]
    [SerializeField] private EstadoGuardia estadoActual = EstadoGuardia.Idle;
    // ─── Movimiento ───────────────────────────────────────────────────
    [Header("Movimiento")]
    [SerializeField] private float velocidadPatrulla = 0.7f;
    [SerializeField] private float velocidadPersecucion = 1.3f;
    [Tooltip("Qué tan rápido gira el guardia hacia una nueva dirección (grados/seg).")]
    [SerializeField] private float suavizadoDireccion = 8f;
    // ─── Detección y Ataque ───────────────────────────────────────────
    [Header("Detección y Ataque")]
    [SerializeField] private float radioDeteccion = 1.4f;
    [Tooltip("Radio extra para dejar de perseguir (histéresis). Debe ser > radioDeteccion.")]
    [SerializeField] private float radioPerderInteres = 2.0f;
    [SerializeField] private float rangoAtaque = 0.35f;
    [SerializeField] private float tiempoEntreAtaques = 1.5f;
    [SerializeField] private int danoAtaque = 1;
    [Header("Línea de Visión (opcional)")]
    [Tooltip("Si está activo, el guardia necesita ver al jugador para perseguirlo.")]
    [SerializeField] private bool requiereLineaDeVision = false;
    [SerializeField] private LayerMask capaObstaculos;
    // ─── Patrulla ─────────────────────────────────────────────────────
    [Header("Patrulla")]
    [SerializeField] private float tiempoCambioDireccion = 2f;
    [Tooltip("Tiempo que el guardia se detiene entre tramos de patrulla.")]
    [SerializeField] private float tiempoPausaIdle = 1f;
    [Tooltip("Distancia máxima que puede alejarse de su punto de origen.")]
    [SerializeField] private float radioPatrullaMax = 5f;
    // ─── Eventos ──────────────────────────────────────────────────────
    [Header("Eventos")]
    [Tooltip("Se invoca cada vez que el guardia realiza un ataque.")]
    public UnityEvent<Transform> onAtacar;
    [Tooltip("Se invoca cuando el guardia detecta al jugador.")]
    public UnityEvent<Transform> onDetectarJugador;
    [Tooltip("Se invoca cuando el guardia pierde de vista al jugador.")]
    public UnityEvent onPerderJugador;
    // ─── Referencias privadas ─────────────────────────────────────────
    private Transform jugador;
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 puntoDeOrigen;
    // ─── Variables de estado interno ──────────────────────────────────
    private Vector2 direccionActual;
    private Vector2 direccionDeseada;
    private float cronometroPatrulla;
    private float cronometroIdle;
    private float cronometroAtaque;
    // ===================================================================
    //  INICIALIZACIÓN
    // ===================================================================
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        puntoDeOrigen = transform.position;
        // Aseguramos que el Rigidbody esté configurado correctamente
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }
    void Start()
    {
        BuscarJugador();
        ElegirNuevaDireccion();
        CambiarEstado(EstadoGuardia.Idle);
    }
    // ===================================================================
    //  BUCLE PRINCIPAL
    // ===================================================================
    void Update()
    {
        // Si perdimos la referencia al jugador, intentamos buscarla de nuevo
        if (jugador == null)
        {
            BuscarJugador();
            // Si sigue sin encontrarse, patrullamos sin perseguir
        }
        switch (estadoActual)
        {
            case EstadoGuardia.Idle:
                TickIdle();
                break;
            case EstadoGuardia.Patrullando:
                TickPatrullar();
                break;
            case EstadoGuardia.Persiguiendo:
                TickPerseguir();
                break;
            case EstadoGuardia.Atacando:
                TickAtacar();
                break;
        }
        // Suavizamos la dirección visual
        if (direccionDeseada != Vector2.zero)
        {
            direccionActual = Vector2.Lerp(direccionActual, direccionDeseada, suavizadoDireccion * Time.deltaTime);
        }
    }
    void FixedUpdate()
    {
        // Movimiento basado en física — solo en estados que se mueven
        if (rb == null) return;
        float velocidad = 0f;
        switch (estadoActual)
        {
            case EstadoGuardia.Patrullando:
                velocidad = velocidadPatrulla;
                break;
            case EstadoGuardia.Persiguiendo:
                velocidad = velocidadPersecucion;
                break;
            default:
                rb.linearVelocity = Vector2.zero;
                return;
        }
        rb.linearVelocity = direccionActual.normalized * velocidad;
    }
    // ===================================================================
    //  ESTADOS
    // ===================================================================
    void TickIdle()
    {
        ActualizarAnimaciones(Vector2.zero, false);
        // Verificar si el jugador está cerca
        if (PuedeDetectarJugador())
        {
            CambiarEstado(EstadoGuardia.Persiguiendo);
            onDetectarJugador?.Invoke(jugador);
            return;
        }
        cronometroIdle += Time.deltaTime;
        if (cronometroIdle >= tiempoPausaIdle)
        {
            ElegirNuevaDireccion();
            CambiarEstado(EstadoGuardia.Patrullando);
        }
    }
    void TickPatrullar()
    {
        // Verificar si el jugador está cerca
        if (PuedeDetectarJugador())
        {
            CambiarEstado(EstadoGuardia.Persiguiendo);
            onDetectarJugador?.Invoke(jugador);
            return;
        }
        cronometroPatrulla += Time.deltaTime;
        // Si se alejó demasiado de su origen, volver
        float distanciaAlOrigen = Vector2.Distance(transform.position, puntoDeOrigen);
        if (distanciaAlOrigen > radioPatrullaMax)
        {
            direccionDeseada = (puntoDeOrigen - (Vector2)transform.position).normalized;
        }
        // Cambiar dirección periódicamente
        if (cronometroPatrulla >= tiempoCambioDireccion)
        {
            CambiarEstado(EstadoGuardia.Idle);
            return;
        }
        ActualizarAnimaciones(direccionActual, true);
    }
    void TickPerseguir()
    {
        if (jugador == null)
        {
            CambiarEstado(EstadoGuardia.Idle);
            onPerderJugador?.Invoke();
            return;
        }
        float distancia = Vector2.Distance(transform.position, jugador.position);
        // ¿Entró en rango de ataque?
        if (distancia <= rangoAtaque)
        {
            CambiarEstado(EstadoGuardia.Atacando);
            return;
        }
        // ¿Perdió el interés? (histéresis para evitar flickeo)
        if (distancia > radioPerderInteres || (requiereLineaDeVision && !TieneLineaDeVision()))
        {
            CambiarEstado(EstadoGuardia.Idle);
            onPerderJugador?.Invoke();
            return;
        }
        direccionDeseada = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
        ActualizarAnimaciones(direccionActual, true);
    }
    void TickAtacar()
    {
        if (jugador == null)
        {
            CambiarEstado(EstadoGuardia.Idle);
            return;
        }
        float distancia = Vector2.Distance(transform.position, jugador.position);
        // Si el jugador salió del rango de ataque, perseguir de nuevo
        if (distancia > rangoAtaque * 1.3f)
        {
            CambiarEstado(EstadoGuardia.Persiguiendo);
            return;
        }
        // Mirar al jugador mientras ataca
        Vector2 dirAlJugador = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
        direccionDeseada = dirAlJugador;
        ActualizarAnimaciones(dirAlJugador, false);
        cronometroAtaque += Time.deltaTime;
        if (cronometroAtaque >= tiempoEntreAtaques)
        {
            EjecutarAtaque();
            cronometroAtaque = 0f;
        }
    }
    // ===================================================================
    //  LÓGICA DE APOYO
    // ===================================================================
    void CambiarEstado(EstadoGuardia nuevoEstado)
    {
        // Reset de cronómetros al entrar en un estado nuevo
        estadoActual = nuevoEstado;
        switch (nuevoEstado)
        {
            case EstadoGuardia.Idle:
                cronometroIdle = 0f;
                if (rb != null) rb.linearVelocity = Vector2.zero;
                break;
            case EstadoGuardia.Patrullando:
                cronometroPatrulla = 0f;
                break;
            case EstadoGuardia.Atacando:
                cronometroAtaque = tiempoEntreAtaques; // Atacar inmediatamente al entrar
                if (rb != null) rb.linearVelocity = Vector2.zero;
                break;
        }
    }
    void ElegirNuevaDireccion()
    {
        // Si estamos lejos del origen, sesgamos la dirección hacia él
        float distanciaAlOrigen = Vector2.Distance(transform.position, puntoDeOrigen);
        if (distanciaAlOrigen > radioPatrullaMax * 0.6f)
        {
            Vector2 haciaOrigen = (puntoDeOrigen - (Vector2)transform.position).normalized;
            float angulo = Mathf.Atan2(haciaOrigen.y, haciaOrigen.x);
            // Añadimos algo de variación (±45°)
            angulo += Random.Range(-0.785f, 0.785f);
            direccionDeseada = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)).normalized;
        }
        else
        {
            float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            direccionDeseada = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)).normalized;
        }
    }
    bool PuedeDetectarJugador()
    {
        if (jugador == null) return false;
        float distancia = Vector2.Distance(transform.position, jugador.position);
        if (distancia > radioDeteccion) return false;
        if (requiereLineaDeVision && !TieneLineaDeVision()) return false;
        return true;
    }
    bool TieneLineaDeVision()
    {
        if (jugador == null) return false;
        Vector2 direccion = jugador.position - transform.position;
        float distancia = direccion.magnitude;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion.normalized, distancia, capaObstaculos);
        // Si no golpeó nada en la capa de obstáculos, hay línea de visión
        return hit.collider == null;
    }
    void EjecutarAtaque()
    {
        // Intentamos hacer daño al jugador vía interfaz IDanable (si existe)
        if (jugador != null)
        {
            IDanable objetivo = jugador.GetComponent<IDanable>();
            if (objetivo != null)
            {
                // Calcular direccion del ataque (del guardia hacia el jugador)
                Vector2 direccionAtaque = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
                objetivo.RecibirDano(danoAtaque, direccionAtaque);
            }
        }
        // Disparar evento para efectos de sonido, partículas, etc.
        onAtacar?.Invoke(jugador);
        Debug.Log($"[Guardia] ¡Ataque! Daño: {danoAtaque}");
    }
    void BuscarJugador()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) jugador = playerGO.transform;
    }
    // ===================================================================
    //  ANIMACIONES
    // ===================================================================
    void ActualizarAnimaciones(Vector2 direccion, bool estaMoviendose)
    {
        if (anim == null) return;
        anim.SetBool("Moving", estaMoviendose);
        anim.SetInteger("Estado", (int)estadoActual);
        if (direccion != Vector2.zero)
        {
            anim.SetFloat("X", direccion.x);
            anim.SetFloat("Y", direccion.y);
        }
    }
    // ===================================================================
    //  GIZMOS
    // ===================================================================
    private void OnDrawGizmosSelected()
    {
        Vector2 origen = Application.isPlaying ? puntoDeOrigen : (Vector2)transform.position;
        // Radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
        // Radio de perder interés (histéresis)
        Gizmos.color = new Color(1f, 0.65f, 0f, 0.4f); // naranja semitransparente
        Gizmos.DrawWireSphere(transform.position, radioPerderInteres);
        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        // Límite de patrulla
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(origen, radioPatrullaMax);
    }
}