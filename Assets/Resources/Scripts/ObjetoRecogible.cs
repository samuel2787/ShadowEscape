using UnityEngine;

/// <summary>
/// Objeto que el jugador puede recoger (tarjeta de acceso, llaves, etc.).
/// Implementa IInteractuable.
///
/// CONFIGURACION:
/// 1. Agregar este script al objeto recogible (ej: tarjetaAcceso).
/// 2. Asignar un nombre (ej: "TarjetaAcceso") y el Sprite/icono.
/// 3. Asegurarse de que el objeto tenga un Collider2D (puede ser trigger).
/// </summary>
public class ObjetoRecogible : MonoBehaviour, IInteractuable
{
    [Header("Datos del Item")]
    [Tooltip("Nombre interno del item (ej: 'TarjetaAcceso'). Debe coincidir con lo que pide la puerta.")]
    [SerializeField] private string nombreItem = "TarjetaAcceso";

    [Tooltip("Icono que se mostrara en el inventario. Arrastra el sprite aqui.")]
    [SerializeField] private Sprite iconoInventario;

    [Header("Efectos")]
    [Tooltip("Mensaje en consola al recoger.")]
    [SerializeField] private string mensajeAlRecoger = "Has recogido la Tarjeta de Acceso!";

    // ===================================================================
    //  INTERFAZ IInteractuable
    // ===================================================================

    public void Interactuar()
    {
        // Asegurar que existe el inventario
        Inventario.AsegurarExistencia();

        // Si no tiene icono asignado, buscarlo automaticamente
        Sprite icono = iconoInventario;
        if (icono == null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) icono = sr.sprite;
        }

        if (icono == null)
        {
            Debug.LogWarning($"[Item] No se encontro icono para '{nombreItem}'. Asigna el sprite en el campo 'Icono Inventario'.");
        }

        // Agregar al inventario
        Inventario.Instancia.AgregarItem(nombreItem, icono);

        Debug.Log($"[Item] {mensajeAlRecoger}");

        // Destruir el objeto de la escena
        Destroy(gameObject);
    }

    public string ObtenerDescripcion()
    {
        return "Recoger " + nombreItem;
    }
}
