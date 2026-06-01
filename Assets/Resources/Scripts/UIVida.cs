using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crea y gestiona una barra de vida en la esquina superior izquierda.
/// Arrastra este script a un GameObject vacio en la escena.
/// Asigna al jugador en el Inspector o lo buscara automaticamente por tag "Player".
/// </summary>
public class UIVida : MonoBehaviour
{
    [Header("Referencia al Jugador")]
    [Tooltip("Arrastra aqui al jugador. Si se deja vacio, lo busca por tag 'Player'.")]
    [SerializeField] private VidaJugador vidaJugador;

    [Header("Apariencia")]
    [SerializeField] private Color colorVidaAlta = new Color(0.2f, 0.85f, 0.3f);   // Verde
    [SerializeField] private Color colorVidaMedia = new Color(1f, 0.75f, 0.1f);     // Amarillo
    [SerializeField] private Color colorVidaBaja = new Color(0.9f, 0.15f, 0.15f);   // Rojo

    // Referencias internas UI
    private Canvas canvas;
    private Image barraFondo;
    private Image barraRelleno;
    private Text textoVida;

    // ===================================================================
    //  INICIALIZACION
    // ===================================================================

    void Start()
    {
        // Buscar al jugador si no se asigno en el Inspector
        if (vidaJugador == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                vidaJugador = playerGO.GetComponent<VidaJugador>();
        }

        if (vidaJugador == null)
        {
            Debug.LogError("[UIVida] No se encontro VidaJugador. Asignalo en el Inspector o usa tag 'Player'.");
            return;
        }

        CrearUI();
        SuscribirEventos();
        ActualizarBarra(vidaJugador.VidaActual);
    }

    void OnDestroy()
    {
        DesuscribirEventos();
    }

    // ===================================================================
    //  CREAR LA UI PROGRAMATICAMENTE
    // ===================================================================

    void CrearUI()
    {
        // --- Canvas ---
        GameObject canvasGO = new GameObject("Canvas_Vida");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Contenedor (panel superior izquierdo) ---
        GameObject contenedor = CrearPanel(canvasGO.transform, "Contenedor_Vida",
            new Vector2(0, 1), new Vector2(0, 1), // Ancla: esquina superior izquierda
            new Vector2(20, -20),                  // Posicion con margen
            new Vector2(250, 50));                 // Tamano
        Image panelImg = contenedor.GetComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.6f); // Fondo semitransparente oscuro

        // Bordes redondeados (si hay sprite disponible, si no, se ve cuadrado)
        panelImg.type = Image.Type.Sliced;
        panelImg.pixelsPerUnitMultiplier = 1f;

        // --- Icono de corazon (texto emoji) ---
        GameObject iconoGO = new GameObject("Icono_Corazon");
        iconoGO.transform.SetParent(contenedor.transform, false);
        RectTransform iconoRect = iconoGO.AddComponent<RectTransform>();
        iconoRect.anchorMin = new Vector2(0, 0);
        iconoRect.anchorMax = new Vector2(0, 1);
        iconoRect.offsetMin = new Vector2(5, 5);
        iconoRect.offsetMax = new Vector2(40, -5);

        Text iconoTexto = iconoGO.AddComponent<Text>();
        iconoTexto.text = "\u2665"; // Corazon
        iconoTexto.fontSize = 28;
        iconoTexto.color = Color.red;
        iconoTexto.alignment = TextAnchor.MiddleCenter;
        iconoTexto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- Fondo de la barra ---
        GameObject fondoGO = CrearPanel(contenedor.transform, "Barra_Fondo",
            new Vector2(0, 0), new Vector2(1, 1),
            Vector2.zero, Vector2.zero);
        RectTransform fondoRect = fondoGO.GetComponent<RectTransform>();
        fondoRect.offsetMin = new Vector2(45, 10);  // Margen izquierdo (despues del icono)
        fondoRect.offsetMax = new Vector2(-10, -10); // Margen derecho y arriba
        barraFondo = fondoGO.GetComponent<Image>();
        barraFondo.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // --- Relleno de la barra ---
        GameObject rellenoGO = CrearPanel(fondoGO.transform, "Barra_Relleno",
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(2, 2), new Vector2(-2, -2));
        barraRelleno = rellenoGO.GetComponent<Image>();
        barraRelleno.color = colorVidaAlta;

        // --- Texto de vida (ej: "5 / 5") ---
        GameObject textoGO = new GameObject("Texto_Vida");
        textoGO.transform.SetParent(fondoGO.transform, false);
        RectTransform textoRect = textoGO.AddComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;

        textoVida = textoGO.AddComponent<Text>();
        textoVida.fontSize = 16;
        textoVida.color = Color.white;
        textoVida.alignment = TextAnchor.MiddleCenter;
        textoVida.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoVida.fontStyle = FontStyle.Bold;

        // Sombra para legibilidad
        Shadow sombra = textoGO.AddComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.8f);
        sombra.effectDistance = new Vector2(1, -1);
    }

    GameObject CrearPanel(Transform padre, string nombre, Vector2 anchorMin, Vector2 anchorMax,
                          Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;

        // Si anchorMin == anchorMax, usar sizeDelta + anchoredPosition
        if (anchorMin == anchorMax)
        {
            rect.pivot = anchorMin;
            rect.anchoredPosition = offsetMin;
            rect.sizeDelta = offsetMax;
        }
        else
        {
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        go.AddComponent<Image>();
        return go;
    }

    // ===================================================================
    //  EVENTOS
    // ===================================================================

    void SuscribirEventos()
    {
        if (vidaJugador == null) return;
        vidaJugador.onRecibirDano.AddListener(ActualizarBarra);
        vidaJugador.onCurarse.AddListener(ActualizarBarra);
    }

    void DesuscribirEventos()
    {
        if (vidaJugador == null) return;
        vidaJugador.onRecibirDano.RemoveListener(ActualizarBarra);
        vidaJugador.onCurarse.RemoveListener(ActualizarBarra);
    }

    // ===================================================================
    //  ACTUALIZAR BARRA
    // ===================================================================

    void ActualizarBarra(int vidaActual)
    {
        if (vidaJugador == null) return;

        float porcentaje = (float)vidaActual / vidaJugador.VidaMaxima;

        // Actualizar relleno
        if (barraRelleno != null)
        {
            barraRelleno.rectTransform.anchorMax = new Vector2(porcentaje, 1);
            barraRelleno.rectTransform.offsetMax = new Vector2(-2, -2);

            // Color segun porcentaje de vida
            if (porcentaje > 0.5f)
                barraRelleno.color = Color.Lerp(colorVidaMedia, colorVidaAlta, (porcentaje - 0.5f) * 2f);
            else
                barraRelleno.color = Color.Lerp(colorVidaBaja, colorVidaMedia, porcentaje * 2f);
        }

        // Actualizar texto
        if (textoVida != null)
        {
            textoVida.text = $"{vidaActual} / {vidaJugador.VidaMaxima}";
        }
    }
}
