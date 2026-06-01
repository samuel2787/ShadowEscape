using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Sistema de interaccion del jugador.
/// Detecta objetos interactuables cercanos, muestra/oculta el indicador "Press E"
/// sobre la cabeza del jugador, y ejecuta la interaccion cuando pulsa E.
/// 
/// CONFIGURACION:
/// 1. Agregar este script al jugador (AxelCane).
/// 2. PressE debe ser HIJO del jugador (ya posicionado sobre su cabeza).
/// 3. En el Player Input, conectar la accion "Interact" al metodo "OnInteractuar".
/// </summary>
public class SistemaInteraccion : MonoBehaviour
{
    [Header("Indicador Visual")]
    [Tooltip("Arrastra aqui el objeto PressE (hijo del jugador). Si no se asigna, lo busca como hijo.")]
    [SerializeField] private GameObject indicadorPressE;

    [Header("Deteccion")]
    [Tooltip("Radio en el que se detectan objetos interactuables. Valor recomendado: 1.5 a 2.")]
    [SerializeField] private float radioInteraccion = 2f;

    [Tooltip("Capas en las que buscar interactuables.")]
    [SerializeField] private LayerMask capasInteraccion = ~0;

    // Estado interno
    private IInteractuable objetoCercano;
    private Transform transformObjetoCercano;
    private ContactFilter2D filtro;
    private List<Collider2D> resultados = new List<Collider2D>();

    void Start()
    {
        // Asegurar que el inventario existe
        Inventario.AsegurarExistencia();

        // Configurar filtro de deteccion (Unity 6 API)
        filtro = new ContactFilter2D();
        filtro.SetLayerMask(capasInteraccion);
        filtro.useLayerMask = true;
        filtro.useTriggers = true;

        // Si no se asigno PressE, buscarlo como hijo
        if (indicadorPressE == null)
        {
            Transform hijo = transform.Find("PressE");
            if (hijo != null)
            {
                indicadorPressE = hijo.gameObject;
                Debug.Log("[Interaccion] PressE encontrado como hijo del jugador.");
            }
            else
            {
                Debug.LogWarning("[Interaccion] No se encontro PressE como hijo. Asignalo en el Inspector.");
            }
        }

        // Ocultar indicador al inicio
        if (indicadorPressE != null)
        {
            indicadorPressE.SetActive(false);
        }
    }

    void Update()
    {
        BuscarObjetoCercano();
        ActualizarIndicador();
    }

    // ===================================================================
    //  INPUT SYSTEM
    // ===================================================================

    public void OnInteractuar(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        EjecutarInteraccion();
    }

    public void EjecutarInteraccion()
    {
        if (objetoCercano != null)
        {
            Debug.Log($"[Interaccion] Interactuando con: {transformObjetoCercano.name}");
            objetoCercano.Interactuar();

            // Verificar si el objeto fue destruido (ej: item recogido)
            if (transformObjetoCercano == null)
            {
                objetoCercano = null;
                transformObjetoCercano = null;
                if (indicadorPressE != null)
                    indicadorPressE.SetActive(false);
            }
        }
    }

    // ===================================================================
    //  DETECCION (Unity 6 API)
    // ===================================================================

    void BuscarObjetoCercano()
    {
        resultados.Clear();

        int cantidad = Physics2D.OverlapCircle(
            transform.position, radioInteraccion, filtro, resultados);

        float distanciaMinima = float.MaxValue;
        IInteractuable mejorCandidato = null;
        Transform mejorTransform = null;

        for (int i = 0; i < cantidad; i++)
        {
            // No detectarse a si mismo ni a hijos (PressE, etc.)
            if (resultados[i].transform == transform) continue;
            if (resultados[i].transform.IsChildOf(transform)) continue;

            IInteractuable interactuable = resultados[i].GetComponent<IInteractuable>();
            if (interactuable != null)
            {
                float dist = Vector2.Distance(transform.position, resultados[i].transform.position);
                if (dist < distanciaMinima)
                {
                    distanciaMinima = dist;
                    mejorCandidato = interactuable;
                    mejorTransform = resultados[i].transform;
                }
            }
        }

        objetoCercano = mejorCandidato;
        transformObjetoCercano = mejorTransform;
    }

    // ===================================================================
    //  INDICADOR "PRESS E" - Se muestra/oculta sobre la cabeza del jugador
    // ===================================================================

    void ActualizarIndicador()
    {
        if (indicadorPressE == null) return;

        // Solo mostrar/ocultar. NO mover posicion porque PressE
        // es hijo del jugador y ya esta posicionado sobre su cabeza.
        if (objetoCercano != null && transformObjetoCercano != null)
        {
            indicadorPressE.SetActive(true);
        }
        else
        {
            indicadorPressE.SetActive(false);
        }
    }

    // ===================================================================
    //  GIZMOS
    // ===================================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}
