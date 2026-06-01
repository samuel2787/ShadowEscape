using UnityEngine;

/// <summary>
/// Interfaz que deben implementar los objetos que pueden recibir daño.
/// Implementa esto en tu script de vida del jugador, enemigos, etc.
/// </summary>
public interface IDanable
{
    void RecibirDano(int cantidad);
    void RecibirDano(int cantidad, Vector2 direccionAtaque);
}
