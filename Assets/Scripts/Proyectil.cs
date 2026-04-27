using UnityEngine;

/// <summary>
/// Maneja la física y colisión de los proyectiles.
/// Implementa daño directo, daño por radio de explosión y fuerzas de onda expansiva.
/// Integrado con PanelFisicaUI para demostrar conceptos de física en tiempo real.
/// </summary>
public class Proyectil : MonoBehaviour
{
    [Header("=== DAÑO ===")]
    [Tooltip("Daño directo al impactar")]
    public float danoDirecto = 40f;

    [Tooltip("Radio de la explosión al impactar")]
    public float radioExplosion = 5f;

    [Tooltip("Fuerza de la explosión (empuje físico)")]
    public float fuerzaExplosion = 500f;

    [Header("=== CONFIGURACIÓN ===")]
    [Tooltip("Tiempo de vida antes de autodestruirse")]
    public float tiempoVida = 6f;

    [Header("=== EFECTOS ===")]
    public GameObject efectoExplosion;
    public AudioClip sonidoExplosion;

    [Header("=== ESTADO ===")]
    public bool esDelJugador = true;

    // Variables internas
    private bool haExplotado = false;
    private Rigidbody rb;
    private TrailRenderer estela;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Crear estela visual para ver la trayectoria
        // Verificar si ya existe una (puede venir del molde clonado)
        estela = GetComponent<TrailRenderer>();
        if (estela == null)
            estela = gameObject.AddComponent<TrailRenderer>();

        estela.time = 1.5f;
        estela.startWidth = 0.15f;
        estela.endWidth = 0.02f;
        estela.material = new Material(Shader.Find("Sprites/Default"));
        estela.startColor = new Color(1f, 0.6f, 0f, 0.8f);
        estela.endColor = new Color(1f, 0.2f, 0f, 0f);
        estela.minVertexDistance = 0.1f;

        // Auto-destrucción después de su tiempo de vida
        Destroy(gameObject, tiempoVida);
    }

    void FixedUpdate()
    {
        // Gravedad personalizada desde el panel de física
        if (PanelFisicaUI.Instancia != null && rb != null)
        {
            // Desactivar gravedad de Unity y usar la personalizada
            rb.useGravity = false;
            float gravedadCustom = PanelFisicaUI.Instancia.gravedadProyectil;
            rb.AddForce(Vector3.down * gravedadCustom * rb.mass, ForceMode.Force);
        }

        // Orientar el proyectil hacia su dirección de vuelo
        if (rb != null && rb.velocity.sqrMagnitude > 0.5f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    void OnCollisionEnter(Collision colision)
    {
        if (haExplotado) return;

        // No colisionar con el que lo disparó
        if (esDelJugador && colision.gameObject.CompareTag("Player")) return;
        if (!esDelJugador && colision.gameObject.CompareTag("Enemigo")) return;

        Explotar(colision);
    }

    /// <summary>
    /// Gestiona la explosión: daño directo, daño en área, fuerzas físicas y efectos.
    /// </summary>
    private void Explotar(Collision colision)
    {
        haExplotado = true;

        // 1. Daño directo al objeto impactado
        SistemaVida vidaImpacto = colision.gameObject.GetComponent<SistemaVida>();
        if (vidaImpacto != null)
        {
            vidaImpacto.RecibirDano(danoDirecto);
        }

        // 2. Daño por radio de explosión (OverlapSphere)
        Collider[] objetosCercanos = Physics.OverlapSphere(transform.position, radioExplosion);
        foreach (Collider obj in objetosCercanos)
        {
            // No repetir daño al impacto directo
            if (obj.gameObject != colision.gameObject)
            {
                SistemaVida vidaArea = obj.GetComponent<SistemaVida>();
                if (vidaArea != null)
                {
                    float distancia = Vector3.Distance(transform.position, obj.transform.position);
                    float factorDano = 1f - Mathf.Clamp01(distancia / radioExplosion);
                    vidaArea.RecibirDano(danoDirecto * factorDano * 0.6f);
                }
            }

            // Aplicar fuerza de explosión (demostración de física)
            Rigidbody rbCercano = obj.GetComponent<Rigidbody>();
            if (rbCercano != null && !rbCercano.isKinematic)
            {
                rbCercano.AddExplosionForce(fuerzaExplosion, transform.position, radioExplosion);
            }
        }

        // 3. Efectos visuales
        CrearEfectosExplosion();

        // 4. Destruir proyectil
        Destroy(gameObject);
    }

    /// <summary>
    /// Crea efectos visuales y sonoros de la explosión.
    /// </summary>
    private void CrearEfectosExplosion()
    {
        if (efectoExplosion != null)
        {
            Instantiate(efectoExplosion, transform.position, Quaternion.identity);
        }
        else
        {
            // Efecto de explosión procedimental (sin prefab)
            // Esfera de fuego
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "ExplosionFlash";
            flash.transform.position = transform.position;
            flash.transform.localScale = Vector3.one * 3f;
            Destroy(flash.GetComponent<Collider>());

            Material matFlash = new Material(Shader.Find("Standard"));
            matFlash.color = new Color(1f, 0.5f, 0f);  // Naranja (sin usar Color.orange)
            matFlash.EnableKeyword("_EMISSION");
            matFlash.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f) * 3f);
            flash.GetComponent<Renderer>().material = matFlash;
            Destroy(flash, 0.2f);

            // Segundo anillo de humo
            GameObject humo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            humo.name = "ExplosionHumo";
            humo.transform.position = transform.position;
            humo.transform.localScale = Vector3.one * 1.5f;
            Destroy(humo.GetComponent<Collider>());

            Material matHumo = new Material(Shader.Find("Standard"));
            matHumo.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            humo.GetComponent<Renderer>().material = matHumo;
            Destroy(humo, 0.4f);
        }

        if (sonidoExplosion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoExplosion, transform.position, 1f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}
