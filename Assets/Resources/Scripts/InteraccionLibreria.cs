using UnityEngine;

public class InteraccionLibreria : MonoBehaviour
{
    [SerializeField] private GameObject cartelAviso; // Arrastra aquí el Canvas/Texto de "Presiona E"

    // Esta función la llamará el jugador
    public void ActivarInteraccion()
    {
        Debug.Log("¡Has interactuado con la librería!");
        // Aquí puedes poner que aparezca un cuadro de texto con historia
    }

    // Para que el aviso aparezca/desaparezca (Opcional si usas triggers)
    public void MostrarAviso(bool estado)
    {
        if (cartelAviso != null) cartelAviso.SetActive(estado);
    }
}