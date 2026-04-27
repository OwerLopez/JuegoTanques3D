using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor principal del juego con FEEDBACK DE OLEADAS.
/// Muestra "🔥 OLEADA X 🔥" con animación fade in/out al cambiar de oleada.
/// </summary>
public class GestorJuego : MonoBehaviour
{
    public static GestorJuego Instancia { get; private set; }

    public enum EstadoJuego { Menu, Jugando, Pausa, Victoria, Derrota }

    [Header("=== ESTADO ===")]
    public EstadoJuego estadoActual = EstadoJuego.Jugando;

    [Header("=== OLEADAS ===")]
    public GameObject prefabEnemigo;
    public Transform[] puntosSpawn;
    public int enemigosPorOleada = 3;
    public int incrementoPorOleada = 1;
    public float tiempoEntreOleadas = 6f;
    public int maximoOleadas = 5;

    [Header("=== REFERENCIAS ===")]
    public Transform jugadorTransform;
    public InterfazUsuario interfazUsuario;

    [Header("=== AUDIO ===")]
    public AudioClip musicaFondo;
    public AudioClip sonidoVictoria;
    public AudioClip sonidoDerrota;
    public AudioClip sonidoNuevaOleada;

    // Variables internas
    private int oleadaActual = 0;
    private int enemigosVivosEnOleada = 0;
    private float tiempoInicioJuego;
    private bool oleadaEnCurso = false;
    private AudioSource fuenteMusica;

    // === ANUNCIO DE OLEADA ===
    private Text textoAnuncioOleada;
    private float tiempoInicioAnuncio;
    private bool mostrandoAnuncio = false;
    private float duracionAnuncio = 3f;

    void Awake()
    {
        if (Instancia == null)
            Instancia = this;
        else { Destroy(gameObject); return; }

        fuenteMusica = GetComponent<AudioSource>();
        if (fuenteMusica == null) fuenteMusica = gameObject.AddComponent<AudioSource>();
        fuenteMusica.loop = true;
        fuenteMusica.volume = 0.3f;

        if (musicaFondo != null) { fuenteMusica.clip = musicaFondo; fuenteMusica.Play(); }

        // Crear el texto de anuncio de oleada
        CrearTextoAnuncioOleada();
    }

    void Start()
    {
        tiempoInicioJuego = Time.time;
        IniciarJuego();
    }

    void Update()
    {
        // Escape: Pausa (funciona siempre)
        if (Input.GetKeyDown(KeyCode.Escape))
            AlternarPausa();

        // R: Reiniciar (cuando no está jugando activamente)
        if (Input.GetKeyDown(KeyCode.R) && estadoActual != EstadoJuego.Jugando)
            ReiniciarJuego();

        // Animar anuncio de oleada
        if (mostrandoAnuncio)
            AnimarAnuncioOleada();

        if (estadoActual != EstadoJuego.Jugando) return;

        // Verificar fin de oleada
        if (oleadaEnCurso && enemigosVivosEnOleada <= 0)
        {
            oleadaEnCurso = false;
            if (maximoOleadas > 0 && oleadaActual >= maximoOleadas)
                Victoria();
            else
                Invoke(nameof(IniciarSiguienteOleada), tiempoEntreOleadas);
        }

        ActualizarUI();
    }

    public void IniciarJuego()
    {
        estadoActual = EstadoJuego.Jugando;
        oleadaActual = 0;
        Time.timeScale = 1f;

        if (SistemaPuntaje.Instancia != null)
            SistemaPuntaje.Instancia.ReiniciarPuntaje();

        IniciarSiguienteOleada();
    }

    private void IniciarSiguienteOleada()
    {
        if (estadoActual != EstadoJuego.Jugando) return;

        oleadaActual++;
        int cantidad = enemigosPorOleada + (oleadaActual - 1) * incrementoPorOleada;
        enemigosVivosEnOleada = cantidad;
        oleadaEnCurso = true;

        Debug.Log($"[GestorJuego] === OLEADA {oleadaActual} === ({cantidad} enemigos)");

        // 🔥 MOSTRAR ANUNCIO DE OLEADA
        MostrarAnuncioOleada(oleadaActual);

        if (sonidoNuevaOleada != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(sonidoNuevaOleada, Camera.main.transform.position);

        for (int i = 0; i < cantidad; i++)
            SpawnearEnemigo(i);

        ActualizarUI();
    }

    private void SpawnearEnemigo(int indice)
    {
        if (prefabEnemigo == null) return;

        Vector3 pos;
        if (puntosSpawn != null && puntosSpawn.Length > 0)
        {
            int idx = indice % puntosSpawn.Length;
            pos = puntosSpawn[idx].position;
            pos += new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f));
        }
        else
        {
            float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(30f, 45f);
            pos = new Vector3(Mathf.Cos(ang) * dist, 0.8f, Mathf.Sin(ang) * dist);
        }

        GameObject nuevo = Instantiate(prefabEnemigo, pos, Quaternion.identity);
        nuevo.SetActive(true);
        nuevo.name = $"Enemigo_O{oleadaActual}_{indice + 1}";

        SistemaVida vida = nuevo.GetComponent<SistemaVida>();
        if (vida != null)
        {
            vida.vidaMaxima = 80f + (oleadaActual - 1) * 15f;
            vida.vidaActual = vida.vidaMaxima;
        }

        EnemigoIA ia = nuevo.GetComponent<EnemigoIA>();
        if (ia != null)
        {
            ia.velocidadMovimiento = 8f + (oleadaActual - 1) * 1f;     // Más rápidos
            ia.intervaloDisparo = Mathf.Max(1.2f, 2.5f - (oleadaActual - 1) * 0.3f);
            ia.imprecision = Mathf.Max(0.05f, 0.12f - (oleadaActual - 1) * 0.02f);
        }
    }

    public void EnemigoEliminado()
    {
        enemigosVivosEnOleada = Mathf.Max(0, enemigosVivosEnOleada - 1);
    }

    public void JugadorMuerto()
    {
        if (estadoActual == EstadoJuego.Derrota) return;
        estadoActual = EstadoJuego.Derrota;
        Debug.Log("[GestorJuego] === DERROTA ===");

        if (sonidoDerrota != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(sonidoDerrota, Camera.main.transform.position);
        if (fuenteMusica != null) fuenteMusica.Stop();
        if (interfazUsuario != null) interfazUsuario.MostrarPantallaDerrota();

        Time.timeScale = 0f;
    }

    private void Victoria()
    {
        estadoActual = EstadoJuego.Victoria;
        Debug.Log("[GestorJuego] === VICTORIA ===");

        if (sonidoVictoria != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(sonidoVictoria, Camera.main.transform.position);
        if (interfazUsuario != null) interfazUsuario.MostrarPantallaVictoria();

        Time.timeScale = 0f;
    }

    public void AlternarPausa()
    {
        if (estadoActual == EstadoJuego.Jugando)
        {
            estadoActual = EstadoJuego.Pausa;
            Time.timeScale = 0f;
            if (interfazUsuario != null) interfazUsuario.MostrarPantalaPausa(true);
        }
        else if (estadoActual == EstadoJuego.Pausa)
        {
            estadoActual = EstadoJuego.Jugando;
            Time.timeScale = 1f;
            if (interfazUsuario != null) interfazUsuario.MostrarPantalaPausa(false);
        }
    }

    public void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ═══════════════════════════════════════════════════
    // 🔥 SISTEMA DE ANUNCIO DE OLEADA (fade in → hold → fade out)
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Crea el texto grande de anuncio en el centro de la pantalla.
    /// </summary>
    private void CrearTextoAnuncioOleada()
    {
        // Buscar o crear un Canvas para el anuncio
        GameObject canvasObj = new GameObject("CanvasAnuncio");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500; // Por encima de todo

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Texto grande central
        GameObject txtObj = new GameObject("TextoAnuncioOleada");
        txtObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.35f);
        rt.anchorMax = new Vector2(0.9f, 0.65f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        textoAnuncioOleada = txtObj.AddComponent<Text>();
        textoAnuncioOleada.text = "";
        textoAnuncioOleada.fontSize = 72;
        textoAnuncioOleada.color = new Color(1f, 0.85f, 0.2f, 0f); // Dorado, invisible al inicio
        textoAnuncioOleada.alignment = TextAnchor.MiddleCenter;
        textoAnuncioOleada.fontStyle = FontStyle.Bold;
        textoAnuncioOleada.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoAnuncioOleada.horizontalOverflow = HorizontalWrapMode.Overflow;
        textoAnuncioOleada.verticalOverflow = VerticalWrapMode.Overflow;

        // Sombra para legibilidad
        Shadow sombra = txtObj.AddComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.9f);
        sombra.effectDistance = new Vector2(3, -3);

        // Outline para más impacto
        Outline outline = txtObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.8f, 0.3f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2, -2);
    }

    /// <summary>
    /// Muestra el anuncio de nueva oleada con animación.
    /// </summary>
    private void MostrarAnuncioOleada(int numeroOleada)
    {
        if (textoAnuncioOleada == null) return;

        textoAnuncioOleada.text = $"OLEADA {numeroOleada}";
        tiempoInicioAnuncio = Time.time;
        mostrandoAnuncio = true;
    }

    /// <summary>
    /// Anima el texto del anuncio: fade in → hold → fade out.
    /// </summary>
    private void AnimarAnuncioOleada()
    {
        if (textoAnuncioOleada == null) return;

        float transcurrido = Time.time - tiempoInicioAnuncio;
        float alpha = 0f;

        // Repartir la duración total (3s por defecto)
        float fadeIn = duracionAnuncio * 0.15f;  // 15% del tiempo
        float fadeOut = duracionAnuncio * 0.35f; // 35% del tiempo
        float hold = duracionAnuncio - fadeIn - fadeOut;

        if (transcurrido < fadeIn)
        {
            alpha = transcurrido / fadeIn;
        }
        else if (transcurrido < fadeIn + hold)
        {
            alpha = 1f;
        }
        else if (transcurrido < fadeIn + hold + fadeOut)
        {
            float t = (transcurrido - fadeIn - hold) / fadeOut;
            alpha = 1f - t;
        }
        else
        {
            alpha = 0f;
            mostrandoAnuncio = false;
        }

        // Escala pulsante durante el hold
        float escala = 1f;
        if (transcurrido >= fadeIn && transcurrido < fadeIn + hold)
        {
            float pulso = Mathf.Sin((transcurrido - fadeIn) * 4f) * 0.05f;
            escala = 1f + pulso;
        }

        textoAnuncioOleada.color = new Color(1f, 0.85f, 0.2f, alpha);
        textoAnuncioOleada.transform.localScale = Vector3.one * escala;
    }

    // ═══ ACTUALIZACIÓN DE UI ═══
    private void ActualizarUI()
    {
        if (interfazUsuario == null) return;

        if (jugadorTransform != null)
        {
            SistemaVida vida = jugadorTransform.GetComponent<SistemaVida>();
            if (vida != null)
            {
                interfazUsuario.ActualizarBarraVida(vida.ObtenerPorcentajeVida());
                interfazUsuario.ActualizarTextoVida(vida.vidaActual, vida.vidaMaxima);
            }
        }

        if (SistemaPuntaje.Instancia != null)
        {
            interfazUsuario.ActualizarPuntaje(SistemaPuntaje.Instancia.puntajeActual);
            interfazUsuario.ActualizarCombo(SistemaPuntaje.Instancia.multiplicadorCombo);
            interfazUsuario.ActualizarEliminaciones(SistemaPuntaje.Instancia.enemigosEliminados);
        }

        interfazUsuario.ActualizarOleada(oleadaActual, enemigosVivosEnOleada);

        if (jugadorTransform != null)
        {
            SistemaDisparo disparo = jugadorTransform.GetComponent<SistemaDisparo>();
            if (disparo != null)
                interfazUsuario.ActualizarEnfriamiento(disparo.ObtenerPorcentajeEnfriamiento());
        }
    }

    public float ObtenerTiempoJuego() => Time.time - tiempoInicioJuego;
    public int ObtenerOleadaActual() => oleadaActual;
}
