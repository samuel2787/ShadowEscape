/// <summary>
/// Interfaz para cualquier objeto con el que el jugador puede interactuar.
/// Ponla en la tarjeta de acceso, puertas, libreros, etc.
/// </summary>
public interface IInteractuable
{
    /// <summary> Se llama cuando el jugador pulsa E cerca del objeto. </summary>
    void Interactuar();

    /// <summary> Texto opcional que describe la interaccion (ej: "Recoger", "Abrir"). </summary>
    string ObtenerDescripcion();
}
