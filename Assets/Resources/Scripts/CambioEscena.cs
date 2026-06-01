using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena
using UnityEngine.InputSystem;    // ¡Nuevo! Necesario para el nuevo Input System

public class CambioEscena : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [SerializeField] private string nombreSiguienteEscena = "Level2"; // Nombre exacto de tu nueva escena

    private bool jugadorEstaCerca = false;

    void Update()
    {
        // Nueva forma de detectar la tecla E con el Input System moderno
        if (jugadorEstaCerca && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CargarSiguienteNivel();
        }
    }

    private void CargarSiguienteNivel()
    {
        if (!string.IsNullOrEmpty(nombreSiguienteEscena))
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
        else
        {
            Debug.LogError("¡Olvidaste escribir el nombre de la escena en el Inspector!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEstaCerca = true;
            Debug.Log("Jugador en la puerta. Presiona 'E' para avanzar.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEstaCerca = false;
            Debug.Log("Jugador se alejo de la puerta.");
        }
    }
}