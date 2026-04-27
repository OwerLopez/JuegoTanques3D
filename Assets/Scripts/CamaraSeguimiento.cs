using UnityEngine;

/// <summary>
/// Cámara que sigue al tanque del jugador con suavizado.
/// Mantiene una posición y rotación relativa constante.
/// </summary>
public class CamaraSeguimiento : MonoBehaviour
{
    [Header("=== CONFIGURACIÓN ===")]
    [Tooltip("Transform del objetivo a seguir (tanque)")]
    public Transform objetivo;

    [Tooltip("Desplazamiento relativo de la cámara respecto al objetivo")]
    public Vector3 desplazamiento = new Vector3(0f, 3.5f, -7f);

    [Tooltip("Velocidad de suavizado del seguimiento")]
    public float velocidadSuavizado = 5f;

    [Tooltip("Velocidad de suavizado de la rotación")]
    public float velocidadRotacion = 3f;

    [Header("=== ZOOM ===")]
    [Tooltip("Distancia mínima de zoom")]
    public float zoomMinimo = 5f;

    [Tooltip("Distancia máxima de zoom")]
    public float zoomMaximo = 20f;

    [Tooltip("Velocidad de zoom con la rueda del mouse")]
    public float velocidadZoom = 3f;

    // Variables internas
    private float zoomActual = 1f;
    private Vector3 desplazamientoBase;

    void Start()
    {
        desplazamientoBase = desplazamiento;
        zoomActual = 1f;

        // Si no hay objetivo asignado, buscar al jugador
        if (objetivo == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                objetivo = jugador.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        // Zoom con rueda del mouse
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            zoomActual -= scrollInput * velocidadZoom;
            zoomActual = Mathf.Clamp(zoomActual, 0.5f, 2f);
            desplazamiento = desplazamientoBase * zoomActual;
        }

        // Calcular posición deseada
        Vector3 posicionDeseada = objetivo.position + objetivo.TransformDirection(desplazamiento);

        // Suavizar movimiento
        transform.position = Vector3.Lerp(
            transform.position,
            posicionDeseada,
            velocidadSuavizado * Time.deltaTime
        );

        // Mirar hacia el objetivo con suavizado
        Vector3 direccionAlObjetivo = objetivo.position - transform.position;
        if (direccionAlObjetivo.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionAlObjetivo);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionDeseada,
                velocidadRotacion * Time.deltaTime
            );
        }
    }
}
