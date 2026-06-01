using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Puerta que requiere un item para pasar al siguiente nivel.
/// Reemplaza al antiguo CambioEscena.cs con soporte para inventario.
///
/// CONFIGURACION:
/// 1. Agregar este script a Puerta_CambioNivel (y quitar CambioEscena si lo tiene).
/// 2. El BoxCollider2D que ya tiene la puerta sirve perfectamente.
/// 3. Agregar la escena destino al Build Settings.
/// </summary>
public class PuertaNivel : MonoBehaviour, IInteractuable
{
    [Header("Requisito")]
    [Tooltip("Nombre del item necesario (ej: 'TarjetaAcceso'). Debe coincidir con el del ObjetoRecogible.")]
    [SerializeField] private string itemRequerido = "TarjetaAcceso";

    [Header("Destino")]
    [Tooltip("Nombre de la escena a cargar. Debe estar en Build Settings.")]
    [SerializeField] private string escenaDestino = "Level2";

    [Tooltip("Si se deja vacio el nombre de escena, carga la siguiente por indice.")]
    [SerializeField] private bool usarSiguienteEscena = false;

    [Header("Mensajes")]
    [SerializeField] private string mensajeSinItem = "Necesitas la Tarjeta de Acceso para pasar.";
    [SerializeField] private string mensajeConItem = "Puerta desbloqueada!";

    // ===================================================================
    //  INTERFAZ IInteractuable
    // ===================================================================

    public void Interactuar()
    {
        Inventario.AsegurarExistencia();

        if (Inventario.Instancia.TieneItem(itemRequerido))
        {
            // Tiene el item: pasar al siguiente nivel
            Debug.Log($"[Puerta] {mensajeConItem} Cargando siguiente nivel...");

            if (!string.IsNullOrEmpty(escenaDestino) && !usarSiguienteEscena)
            {
                SceneManager.LoadScene(escenaDestino);
            }
            else
            {
                int siguienteIndice = SceneManager.GetActiveScene().buildIndex + 1;
                if (siguienteIndice < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(siguienteIndice);
                }
                else
                {
                    Debug.LogWarning("[Puerta] No hay siguiente escena en Build Settings.");
                }
            }
        }
        else
        {
            // No tiene el item
            Debug.Log($"[Puerta] {mensajeSinItem}");
        }
    }

    public string ObtenerDescripcion()
    {
        return "Abrir puerta";
    }
}
