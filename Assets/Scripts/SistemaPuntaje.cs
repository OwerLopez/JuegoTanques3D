using UnityEngine;

/// <summary>
/// Maneja el puntaje del jugador, combos y estadísticas.
/// Implementado como Singleton para acceso global.
/// </summary>
public class SistemaPuntaje : MonoBehaviour
{
    public static SistemaPuntaje Instancia { get; private set; }

    [Header("=== ESTADÍSTICAS ===")]
    public int puntajeActual = 0;
    public int enemigosEliminados = 0;
    public int multiplicadorCombo = 1;

    [Header("=== CONFIGURACIÓN COMBO ===")]
    [Tooltip("Tiempo máximo entre eliminaciones para mantener el combo")]
    public float tiempoCombo = 5f;

    // Variables internas
    private float tiempoUltimaEliminacion;

    void Awake()
    {
        if (Instancia == null)
            Instancia = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // Resetear combo si pasa mucho tiempo sin eliminar
        if (multiplicadorCombo > 1 && Time.time > tiempoUltimaEliminacion + tiempoCombo)
        {
            multiplicadorCombo = 1;
        }
    }

    /// <summary>
    /// Suma puntos al jugador con multiplicador de combo.
    /// </summary>
    public void SumarPuntos(int puntosBase)
    {
        // Actualizar combo
        if (Time.time <= tiempoUltimaEliminacion + tiempoCombo)
            multiplicadorCombo = Mathf.Min(multiplicadorCombo + 1, 5); // Máximo x5
        else
            multiplicadorCombo = 1;

        tiempoUltimaEliminacion = Time.time;
        enemigosEliminados++;

        int puntosFinales = puntosBase * multiplicadorCombo;
        puntajeActual += puntosFinales;

        Debug.Log($"[Puntaje] +{puntosFinales} (Combo x{multiplicadorCombo}) | Total: {puntajeActual}");
    }

    /// <summary>
    /// Reinicia todo el puntaje.
    /// </summary>
    public void ReiniciarPuntaje()
    {
        puntajeActual = 0;
        enemigosEliminados = 0;
        multiplicadorCombo = 1;
    }
}
