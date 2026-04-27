using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de salud genérico para jugador y enemigos.
/// Maneja daño, curación, muerte y regeneración.
/// </summary>
public class SistemaVida : MonoBehaviour
{
    [Header("=== ESTADÍSTICAS ===")]
    public float vidaMaxima = 100f;
    public float vidaActual;

    [Header("=== REGENERACIÓN ===")]
    public bool regeneraVida = false;
    public float velocidadRegeneracion = 5f;
    public float retardoRegeneracion = 5f;

    [Header("=== EVENTOS ===")]
    public UnityEvent alRecibirDano;
    public UnityEvent alMorir;
    public UnityEvent alCurar;

    // Variables internas
    private float tiempoUltimoDano;
    private bool estaMuerto = false;

    void Awake()
    {
        vidaActual = vidaMaxima;
    }

    void Update()
    {
        // Regeneración de vida
        if (regeneraVida && !estaMuerto && vidaActual < vidaMaxima)
        {
            if (Time.time >= tiempoUltimoDano + retardoRegeneracion)
            {
                vidaActual += velocidadRegeneracion * Time.deltaTime;
                vidaActual = Mathf.Min(vidaActual, vidaMaxima);
            }
        }
    }

    /// <summary>
    /// Aplica daño a la entidad.
    /// </summary>
    public void RecibirDano(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        tiempoUltimoDano = Time.time;

        alRecibirDano?.Invoke();

        if (vidaActual <= 0f)
        {
            vidaActual = 0f;
            Morir();
        }
    }

    /// <summary>
    /// Cura a la entidad.
    /// </summary>
    public void Curar(float cantidad)
    {
        if (estaMuerto) return;
        vidaActual += cantidad;
        vidaActual = Mathf.Min(vidaActual, vidaMaxima);
        alCurar?.Invoke();
    }

    /// <summary>
    /// Procesa la muerte de la entidad.
    /// </summary>
    private void Morir()
    {
        estaMuerto = true;
        alMorir?.Invoke();

        Debug.Log($"[SistemaVida] {gameObject.name} ha muerto.");

        // Lógica específica según tag
        if (gameObject.CompareTag("Enemigo"))
        {
            if (GestorJuego.Instancia != null)
                GestorJuego.Instancia.EnemigoEliminado();

            if (SistemaPuntaje.Instancia != null)
                SistemaPuntaje.Instancia.SumarPuntos(100);

            Destroy(gameObject, 0.15f);
        }
        else if (gameObject.CompareTag("Player"))
        {
            if (GestorJuego.Instancia != null)
                GestorJuego.Instancia.JugadorMuerto();
        }
    }

    /// <summary>
    /// Retorna el porcentaje de vida actual (0 a 1).
    /// </summary>
    public float ObtenerPorcentajeVida()
    {
        return Mathf.Clamp01(vidaActual / vidaMaxima);
    }
}
