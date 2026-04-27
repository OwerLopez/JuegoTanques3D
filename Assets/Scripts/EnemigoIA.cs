using UnityEngine;

/// <summary>
/// IA de enemigo MEJORADA: más rápida, más inteligente, comportamiento táctico.
/// Estados: Patrullando → Persiguiendo → Atacando → Flanqueando
/// </summary>
public class EnemigoIA : MonoBehaviour
{
    [Header("=== DETECCIÓN ===")]
    public float rangoDeteccion = 35f;
    public float rangoAtaque = 22f;
    public float distanciaMinima = 10f;

    [Header("=== MOVIMIENTO (AUMENTADO) ===")]
    public float velocidadMovimiento = 12f;     // Antes: 6
    public float velocidadRotacion = 6f;         // Antes: 3

    [Header("=== PATRULLA ===")]
    public float radioPatrulla = 20f;
    public float tiempoEsperaPatrulla = 1.5f;   // Antes: 3 (más activo)

    [Header("=== COMBATE ===")]
    public GameObject prefabProyectilEnemigo;
    public Transform puntoDisparoEnemigo;
    public float fuerzaDisparoEnemigo = 28f;
    public float intervaloDisparo = 2.5f;        // Antes: 3 (más agresivo)

    [Range(0f, 1f)]
    public float imprecision = 0.12f;

    [Header("=== TÁCTICAS ===")]
    [Tooltip("Tiempo entre cambios de dirección táctica")]
    public float intervaloFlanqueo = 4f;

    public AudioClip sonidoDisparoEnemigo;

    // Estados
    public enum EstadoIA { Patrullando, Persiguiendo, Atacando, Flanqueando, Muerto }

    [HideInInspector]
    public EstadoIA estadoActual = EstadoIA.Patrullando;

    // Variables internas
    private Transform jugador;
    private Rigidbody cuerpoRigido;
    private Vector3 puntoInicial;
    private Vector3 puntoPatrullaActual;
    private Vector3 puntoFlanqueo;
    private float tiempoUltimoDisparo;
    private float tiempoLlegadaPatrulla;
    private float tiempoUltimoFlanqueo;
    private bool esperandoEnPatrulla;
    private AudioSource fuenteAudio;
    private SistemaVida sistemaVida;
    private int direccionFlanqueo = 1; // 1 o -1

    void Start()
    {
        GameObject objJugador = GameObject.FindGameObjectWithTag("Player");
        if (objJugador != null) jugador = objJugador.transform;

        cuerpoRigido = GetComponent<Rigidbody>();
        if (cuerpoRigido == null) cuerpoRigido = gameObject.AddComponent<Rigidbody>();

        cuerpoRigido.mass = 35f;
        cuerpoRigido.drag = 2f;
        cuerpoRigido.angularDrag = 4f;
        cuerpoRigido.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null) fuenteAudio = gameObject.AddComponent<AudioSource>();
        fuenteAudio.playOnAwake = false;
        fuenteAudio.spatialBlend = 1f;

        sistemaVida = GetComponent<SistemaVida>();
        puntoInicial = transform.position;
        SeleccionarNuevoPuntoPatrulla();
        tiempoUltimoDisparo = -intervaloDisparo;
        tiempoUltimoFlanqueo = Time.time;
        direccionFlanqueo = Random.value > 0.5f ? 1 : -1;

        gameObject.tag = "Enemigo";
    }

    void Update()
    {
        if (sistemaVida != null && sistemaVida.vidaActual <= 0f)
        {
            estadoActual = EstadoIA.Muerto;
            return;
        }

        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        switch (estadoActual)
        {
            case EstadoIA.Patrullando:
                ComportamientoPatrulla();
                if (distancia <= rangoDeteccion)
                    estadoActual = EstadoIA.Persiguiendo;
                break;

            case EstadoIA.Persiguiendo:
                ComportamientoPersecucion();
                if (distancia <= rangoAtaque)
                {
                    // Decidir: atacar directo o flanquear
                    if (Random.value > 0.4f)
                        estadoActual = EstadoIA.Atacando;
                    else
                    {
                        estadoActual = EstadoIA.Flanqueando;
                        CalcularPuntoFlanqueo();
                    }
                }
                if (distancia > rangoDeteccion * 1.3f)
                {
                    estadoActual = EstadoIA.Patrullando;
                    SeleccionarNuevoPuntoPatrulla();
                }
                break;

            case EstadoIA.Atacando:
                ComportamientoAtaque();
                if (distancia > rangoAtaque * 1.15f)
                    estadoActual = EstadoIA.Persiguiendo;
                // Cambiar a flanqueo periódicamente para no quedarse quieto
                if (Time.time > tiempoUltimoFlanqueo + intervaloFlanqueo)
                {
                    estadoActual = EstadoIA.Flanqueando;
                    CalcularPuntoFlanqueo();
                }
                break;

            case EstadoIA.Flanqueando:
                ComportamientoFlanqueo();
                if (distancia > rangoAtaque * 1.3f)
                    estadoActual = EstadoIA.Persiguiendo;
                break;
        }
    }

    // ═══ PATRULLA: Movimiento aleatorio entre puntos ═══
    private void ComportamientoPatrulla()
    {
        float dist = Vector3.Distance(transform.position, puntoPatrullaActual);

        if (dist < 2f)
        {
            if (!esperandoEnPatrulla)
            {
                esperandoEnPatrulla = true;
                tiempoLlegadaPatrulla = Time.time;
            }
            if (Time.time - tiempoLlegadaPatrulla >= tiempoEsperaPatrulla)
            {
                SeleccionarNuevoPuntoPatrulla();
                esperandoEnPatrulla = false;
            }
        }
        else
        {
            MoverHacia(puntoPatrullaActual);
        }
    }

    // ═══ PERSECUCIÓN: Ir directo al jugador ═══
    private void ComportamientoPersecucion()
    {
        float dist = Vector3.Distance(transform.position, jugador.position);
        if (dist > distanciaMinima)
            MoverHacia(jugador.position);
        RotarHacia(jugador.position);
    }

    // ═══ ATAQUE: Disparar manteniendo distancia, moverse lateralmente ═══
    private void ComportamientoAtaque()
    {
        float dist = Vector3.Distance(transform.position, jugador.position);

        if (dist < distanciaMinima * 0.7f)
        {
            // Retroceder si está muy cerca
            Vector3 retroceso = (transform.position - jugador.position).normalized;
            MoverHacia(transform.position + retroceso * 6f);
        }
        else
        {
            // Movimiento lateral (strafe) para ser más difícil de golpear
            Vector3 lateral = Vector3.Cross(Vector3.up, (jugador.position - transform.position).normalized);
            Vector3 destino = transform.position + lateral * direccionFlanqueo * 3f;
            MoverHacia(destino);
        }

        RotarHacia(jugador.position);

        if (Time.time >= tiempoUltimoDisparo + intervaloDisparo)
            DispararAlJugador();
    }

    // ═══ FLANQUEO: Rodear al jugador para atacar desde un ángulo ═══
    private void ComportamientoFlanqueo()
    {
        float distAlPunto = Vector3.Distance(transform.position, puntoFlanqueo);

        if (distAlPunto < 3f)
        {
            // Llegó al punto de flanqueo, atacar
            estadoActual = EstadoIA.Atacando;
            tiempoUltimoFlanqueo = Time.time;
            direccionFlanqueo *= -1; // Alternar dirección
        }
        else
        {
            MoverHacia(puntoFlanqueo);
        }

        RotarHacia(jugador.position);

        // Disparar mientras flanquea (si tiene línea de visión)
        if (Time.time >= tiempoUltimoDisparo + intervaloDisparo * 1.3f)
            DispararAlJugador();
    }

    /// <summary>
    /// Calcula una posición lateral respecto al jugador para flanquearlo.
    /// </summary>
    private void CalcularPuntoFlanqueo()
    {
        Vector3 dirAlJugador = (jugador.position - transform.position).normalized;
        Vector3 lateral = Vector3.Cross(Vector3.up, dirAlJugador) * direccionFlanqueo;
        puntoFlanqueo = jugador.position + lateral * Random.Range(12f, 18f) - dirAlJugador * 5f;
        puntoFlanqueo.y = transform.position.y;
        tiempoUltimoFlanqueo = Time.time;
    }

    // ═══ UTILIDADES DE MOVIMIENTO ═══
    private void MoverHacia(Vector3 objetivo)
    {
        Vector3 dir = (objetivo - transform.position).normalized;
        dir.y = 0f;
        cuerpoRigido.MovePosition(transform.position + dir * velocidadMovimiento * Time.deltaTime);
        RotarHacia(objetivo);
    }

    private void RotarHacia(Vector3 objetivo)
    {
        Vector3 dir = (objetivo - transform.position).normalized;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, velocidadRotacion * Time.deltaTime);
        }
    }

    private void SeleccionarNuevoPuntoPatrulla()
    {
        Vector2 aleatorio = Random.insideUnitCircle * radioPatrulla;
        puntoPatrullaActual = puntoInicial + new Vector3(aleatorio.x, 0f, aleatorio.y);
    }

    // ═══ DISPARO ═══
    private void DispararAlJugador()
    {
        if (prefabProyectilEnemigo == null) return;

        tiempoUltimoDisparo = Time.time;

        Vector3 posDisparo = puntoDisparoEnemigo != null
            ? puntoDisparoEnemigo.position
            : transform.position + transform.forward * 2f + Vector3.up * 0.5f;

        Vector3 direccion = (jugador.position - posDisparo).normalized;
        direccion += new Vector3(
            Random.Range(-imprecision, imprecision),
            Random.Range(-imprecision * 0.2f, imprecision * 0.15f),
            Random.Range(-imprecision, imprecision)
        );
        direccion.Normalize();

        GameObject proy = Instantiate(prefabProyectilEnemigo, posDisparo, Quaternion.LookRotation(direccion));
        proy.SetActive(true);
        proy.name = "Proyectil_Enemigo";

        Rigidbody rb = proy.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(direccion * fuerzaDisparoEnemigo, ForceMode.VelocityChange);

        Proyectil script = proy.GetComponent<Proyectil>();
        if (script != null) script.esDelJugador = false;

        if (sonidoDisparoEnemigo != null && fuenteAudio != null)
            fuenteAudio.PlayOneShot(sonidoDisparoEnemigo);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? puntoInicial : transform.position, radioPatrulla);
        if (Application.isPlaying && estadoActual == EstadoIA.Flanqueando)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(puntoFlanqueo, 1f);
        }
    }
}
