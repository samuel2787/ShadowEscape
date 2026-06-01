using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Inventario del jugador (Singleton).
/// Crea su propio Canvas dedicado para mostrar items recogidos.
/// </summary>
public class Inventario : MonoBehaviour
{
    public static Inventario Instancia { get; private set; }

    private List<ItemInventario> items = new List<ItemInventario>();

    // UI - Canvas propio
    private GameObject canvasGO;
    private GameObject panelGO;

    [System.Serializable]
    public class ItemInventario
    {
        public string nombre;
        public Sprite icono;

        public ItemInventario(string nombre, Sprite icono)
        {
            this.nombre = nombre;
            this.icono = icono;
        }
    }

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    void Start()
    {
        CrearCanvasPropio();
    }

    // ===================================================================
    //  METODOS PUBLICOS
    // ===================================================================

    public void AgregarItem(string nombre, Sprite icono)
    {
        items.Add(new ItemInventario(nombre, icono));
        Debug.Log($"[Inventario] Item recogido: {nombre} | Total: {items.Count}");
        ReconstruirUI();
    }

    public bool TieneItem(string nombre)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].nombre == nombre) return true;
        }
        return false;
    }

    public void RemoverItem(string nombre)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].nombre == nombre)
            {
                items.RemoveAt(i);
                ReconstruirUI();
                return;
            }
        }
    }

    // ===================================================================
    //  CREAR CANVAS PROPIO (separado del de UIVida)
    // ===================================================================

    void CrearCanvasPropio()
    {
        // Canvas dedicado solo para el inventario
        canvasGO = new GameObject("Canvas_Inventario_Dedicado");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // Por encima de todo

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel contenedor
        panelGO = new GameObject("Panel_Items");
        panelGO.transform.SetParent(canvasGO.transform, false);

        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1); // Esquina superior izquierda
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -85);
        panelRect.sizeDelta = new Vector2(300, 75);

        Image panelBg = panelGO.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.75f);

        HorizontalLayoutGroup layout = panelGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(8, 8, 5, 5);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        // Ocultar hasta que haya items
        panelGO.SetActive(false);
    }

    // ===================================================================
    //  RECONSTRUIR UI
    // ===================================================================

    void ReconstruirUI()
    {
        if (panelGO == null) return;

        // Borrar hijos anteriores
        for (int c = panelGO.transform.childCount - 1; c >= 0; c--)
        {
            Destroy(panelGO.transform.GetChild(c).gameObject);
        }

        panelGO.SetActive(items.Count > 0);

        for (int i = 0; i < items.Count; i++)
        {
            CrearSlot(items[i]);
        }
    }

    void CrearSlot(ItemInventario item)
    {
        // --- Slot contenedor ---
        GameObject slotGO = new GameObject($"Slot_{item.nombre}");
        slotGO.transform.SetParent(panelGO.transform, false);

        RectTransform slotRect = slotGO.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(65, 65);

        // Fondo del slot
        Image slotBg = slotGO.AddComponent<Image>();
        slotBg.color = new Color(0.15f, 0.18f, 0.25f, 0.95f);

        // --- Icono ---
        GameObject iconoGO = new GameObject("Icono");
        iconoGO.transform.SetParent(slotGO.transform, false);

        RectTransform iconoRect = iconoGO.AddComponent<RectTransform>();
        iconoRect.anchorMin = new Vector2(0.1f, 0.25f);
        iconoRect.anchorMax = new Vector2(0.9f, 0.95f);
        iconoRect.offsetMin = Vector2.zero;
        iconoRect.offsetMax = Vector2.zero;

        Image iconoImg = iconoGO.AddComponent<Image>();
        iconoImg.raycastTarget = false;

        if (item.icono != null)
        {
            iconoImg.sprite = item.icono;
            iconoImg.type = Image.Type.Simple;
            iconoImg.preserveAspect = true;
            iconoImg.color = Color.white;
            Debug.Log($"[Inventario] Sprite asignado: {item.icono.name} ({item.icono.rect.width}x{item.icono.rect.height})");
        }
        else
        {
            // Sin sprite: mostrar cuadro de color
            iconoImg.sprite = null;
            iconoImg.color = new Color(1f, 0.7f, 0.2f, 0.9f);
            Debug.LogWarning($"[Inventario] Item '{item.nombre}' no tiene icono.");
        }

        // --- Nombre del item ---
        GameObject textoGO = new GameObject("Texto");
        textoGO.transform.SetParent(slotGO.transform, false);

        RectTransform textoRect = textoGO.AddComponent<RectTransform>();
        textoRect.anchorMin = new Vector2(0, 0);
        textoRect.anchorMax = new Vector2(1, 0.25f);
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;

        Text texto = textoGO.AddComponent<Text>();
        texto.text = item.nombre;
        texto.fontSize = 10;
        texto.color = new Color(0.8f, 0.85f, 1f);
        texto.alignment = TextAnchor.MiddleCenter;
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.raycastTarget = false;
    }

    // ===================================================================
    //  INICIALIZACION AUTOMATICA
    // ===================================================================

    public static void AsegurarExistencia()
    {
        if (Instancia == null)
        {
            GameObject go = new GameObject("Inventario");
            go.AddComponent<Inventario>();
        }
    }
}
