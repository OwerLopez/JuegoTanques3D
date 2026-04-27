using UnityEngine;

/// <summary>
/// Sistema de disparo del tanque del jugador.
/// Integrado con PanelFisicaUI para aplicar parámetros en tiempo real.
/// </summary>
public class SistemaDisparo : MonoBehaviour
{
    [Header("=== PROYECTIL ===")]
    public GameObject prefabProyectil;
    public Transform puntoDisparo;

    [Header("=== CONFIGURACIÓN ===")]
    public float fuerzaDisparo = 30f;
    public float tiempoEnfriamiento = 1.2f;

    [Header("=== AUDIO ===")]
    public AudioClip sonidoDisparo;

    // Variables internas
    private float tiempoUltimoDisparo = -999f;
    private AudioSource fuenteAudio;
    private ControladorTanque controlador;

    void Awake()
    {
        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null)
            fuenteAudio = gameObject.AddComponent<AudioSource>();
        fuenteAudio.playOnAwake = false;

        controlador = GetComponent<ControladorTanque>();
    }

    void Update()
    {
        // Disparar con Espacio o Clic Izquierdo
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Disparar();
        }
    }

    /// <summary>
    /// Dispara un proyectil aplicando los parámetros del panel de física.
    /// </summary>
    public void Disparar()
    {
        if (Time.time < tiempoUltimoDisparo + tiempoEnfriamiento) return;

        if (prefabProyectil == null)
        {
            Debug.LogWarning("[SistemaDisparo] Falta prefab del proyectil.");
            return;
        }
        if (puntoDisparo == null)
        {
            Debug.LogWarning("[SistemaDisparo] Falta punto de disparo.");
            return;
        }

        tiempoUltimoDisparo = Time.time;

        // Dirección de la torreta
        Vector3 dir = controlador != null ? controlador.ObtenerDireccionDisparo() : transform.forward;

        // Instanciar proyectil
        GameObject proy = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.LookRotation(dir));
        proy.SetActive(true);
        proy.name = "Proyectil_Jugador";

        Rigidbody rb = proy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Aplicar parámetros del Panel de Física si existe
            if (PanelFisicaUI.Instancia != null)
            {
                rb.mass = PanelFisicaUI.Instancia.masaProyectil;
                rb.drag = PanelFisicaUI.Instancia.dragProyectil;
                float fuerza = PanelFisicaUI.Instancia.fuerzaDisparo;
                rb.AddForce(dir * fuerza, ForceMode.VelocityChange);

                Debug.Log($"[Disparo] Fuerza={fuerza:F1} | Masa={rb.mass:F2} | Drag={rb.drag:F2}");
            }
            else
            {
                rb.AddForce(dir * fuerzaDisparo, ForceMode.VelocityChange);
            }
        }

        // Marcar como del jugador
        Proyectil script = proy.GetComponent<Proyectil>();
        if (script != null) script.esDelJugador = true;

        // Sonido
        if (sonidoDisparo != null)
            fuenteAudio.PlayOneShot(sonidoDisparo);
    }

    /// <summary>
    /// Porcentaje de enfriamiento (0 = recargando, 1 = listo).
    /// </summary>
    public float ObtenerPorcentajeEnfriamiento()
    {
        return Mathf.Clamp01((Time.time - tiempoUltimoDisparo) / tiempoEnfriamiento);
    }
}
