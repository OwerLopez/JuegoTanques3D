using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interfaz de usuario del juego.
/// Muestra vida, puntaje, oleada, combo, y pantallas de victoria/derrota.
/// Se crea enteramente por código (no requiere Canvas prefabricado).
/// </summary>
public class InterfazUsuario : MonoBehaviour
{
    [Header("=== REFERENCIAS UI (Auto-generadas) ===")]
    private Canvas canvas;
    private Text textoPuntaje;
    private Text textoVida;
    private Text textoOleada;
    private Text textoCombo;
    private Text textoEliminaciones;
    private Text textoEnfriamiento;
    private Image barraVidaFondo;
    private Image barraVidaRelleno;
    private Image barraEnfriamientoRelleno;

    // Paneles
    private GameObject panelHUD;
    private GameObject panelPausa;
    private GameObject panelDerrota;
    private GameObject panelVictoria;

    // Textos de paneles
    private Text textoPuntajeFinal;
    private Text textoEstadisticas;

    void Awake()
    {
        CrearInterfazCompleta();
    }

    /// <summary>
    /// Crea toda la interfaz de usuario programáticamente.
    /// </summary>
    private void CrearInterfazCompleta()
    {
        // Crear Canvas
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        gameObject.AddComponent<GraphicRaycaster>();

        // Crear HUD principal
        CrearHUD();
        // Crear pantalla de pausa
        CrearPanelPausa();
        // Crear pantalla de derrota
        CrearPanelDerrota();
        // Crear pantalla de victoria
        CrearPanelVictoria();
    }

    private void CrearHUD()
    {
        panelHUD = CrearPanel("PanelHUD", canvas.transform);

        // === BARRA DE VIDA (arriba izquierda) ===
        GameObject contenedorVida = CrearPanel("ContenedorVida", panelHUD.transform);
        RectTransform rtVida = contenedorVida.GetComponent<RectTransform>();
        rtVida.anchorMin = new Vector2(0, 1);
        rtVida.anchorMax = new Vector2(0, 1);
        rtVida.pivot = new Vector2(0, 1);
        rtVida.anchoredPosition = new Vector2(20, -20);
        rtVida.sizeDelta = new Vector2(350, 80);

        // Texto "VIDA"
        textoVida = CrearTexto("TextoVida", contenedorVida.transform,
            "VIDA: 100 / 100", 20, TextAnchor.UpperLeft,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(5, -5), new Vector2(340, 30));
        textoVida.color = Color.white;
        textoVida.fontStyle = FontStyle.Bold;

        // Fondo barra vida
        barraVidaFondo = CrearImagen("BarraVidaFondo", contenedorVida.transform,
            new Color(0.2f, 0.2f, 0.2f, 0.8f));
        RectTransform rtFondo = barraVidaFondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = new Vector2(0, 0);
        rtFondo.anchorMax = new Vector2(0, 0);
        rtFondo.pivot = new Vector2(0, 0);
        rtFondo.anchoredPosition = new Vector2(5, 10);
        rtFondo.sizeDelta = new Vector2(320, 25);

        // Relleno barra vida
        barraVidaRelleno = CrearImagen("BarraVidaRelleno", barraVidaFondo.transform,
            new Color(0.2f, 0.85f, 0.3f, 1f));
        RectTransform rtRelleno = barraVidaRelleno.GetComponent<RectTransform>();
        rtRelleno.anchorMin = Vector2.zero;
        rtRelleno.anchorMax = new Vector2(1, 1);
        rtRelleno.offsetMin = new Vector2(2, 2);
        rtRelleno.offsetMax = new Vector2(-2, -2);

        // === PUNTAJE (arriba derecha) ===
        textoPuntaje = CrearTexto("TextoPuntaje", panelHUD.transform,
            "PUNTOS: 0", 28, TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -20), new Vector2(300, 40));
        textoPuntaje.color = new Color(1f, 0.85f, 0f, 1f); // Dorado
        textoPuntaje.fontStyle = FontStyle.Bold;

        // Combo
        textoCombo = CrearTexto("TextoCombo", panelHUD.transform,
            "", 22, TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -60), new Vector2(200, 30));
        textoCombo.color = new Color(1f, 0.5f, 0f, 1f); // Naranja

        // Eliminaciones
        textoEliminaciones = CrearTexto("TextoEliminaciones", panelHUD.transform,
            "Eliminaciones: 0", 18, TextAnchor.UpperRight,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -90), new Vector2(250, 25));
        textoEliminaciones.color = Color.white;

        // === OLEADA (arriba centro) ===
        textoOleada = CrearTexto("TextoOleada", panelHUD.transform,
            "OLEADA 1", 24, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -20), new Vector2(300, 50));
        textoOleada.color = Color.white;
        textoOleada.fontStyle = FontStyle.Bold;

        // === ENFRIAMIENTO DISPARO (abajo centro) ===
        GameObject contenedorEnfriamiento = CrearPanel("ContenedorEnfriamiento", panelHUD.transform);
        RectTransform rtEnf = contenedorEnfriamiento.GetComponent<RectTransform>();
        rtEnf.anchorMin = new Vector2(0.5f, 0);
        rtEnf.anchorMax = new Vector2(0.5f, 0);
        rtEnf.pivot = new Vector2(0.5f, 0);
        rtEnf.anchoredPosition = new Vector2(0, 30);
        rtEnf.sizeDelta = new Vector2(200, 30);

        Image fondoEnf = CrearImagen("FondoEnf", contenedorEnfriamiento.transform,
            new Color(0.2f, 0.2f, 0.2f, 0.6f));
        RectTransform rtFondoEnf = fondoEnf.GetComponent<RectTransform>();
        rtFondoEnf.anchorMin = Vector2.zero;
        rtFondoEnf.anchorMax = Vector2.one;
        rtFondoEnf.offsetMin = Vector2.zero;
        rtFondoEnf.offsetMax = Vector2.zero;

        barraEnfriamientoRelleno = CrearImagen("RellenoEnf", fondoEnf.transform,
            new Color(0.3f, 0.7f, 1f, 0.9f));
        RectTransform rtRellenoEnf = barraEnfriamientoRelleno.GetComponent<RectTransform>();
        rtRellenoEnf.anchorMin = Vector2.zero;
        rtRellenoEnf.anchorMax = new Vector2(1, 1);
        rtRellenoEnf.offsetMin = new Vector2(2, 2);
        rtRellenoEnf.offsetMax = new Vector2(-2, -2);

        textoEnfriamiento = CrearTexto("TextoEnfriamiento", contenedorEnfriamiento.transform,
            "RECARGA", 14, TextAnchor.MiddleCenter,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        RectTransform rtTextoEnf = textoEnfriamiento.GetComponent<RectTransform>();
        rtTextoEnf.offsetMin = Vector2.zero;
        rtTextoEnf.offsetMax = Vector2.zero;
        textoEnfriamiento.color = Color.white;
        textoEnfriamiento.fontStyle = FontStyle.Bold;

        // === CONTROLES (abajo izquierda) ===
        Text textoControles = CrearTexto("TextoControles", panelHUD.transform,
            "WASD: Mover | Q/E: Torreta | Espacio/Clic: Disparar | Esc: Pausa",
            14, TextAnchor.LowerLeft,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(20, 10), new Vector2(600, 25));
        textoControles.color = new Color(1f, 1f, 1f, 0.6f);
    }

    private void CrearPanelPausa()
    {
        panelPausa = CrearPanelConFondo("PanelPausa", canvas.transform, new Color(0, 0, 0, 0.7f));
        panelPausa.SetActive(false);

        CrearTexto("TituloPausa", panelPausa.transform,
            "PAUSA", 48, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 60), new Vector2(400, 70)).color = Color.white;

        CrearTexto("SubtituloPausa", panelPausa.transform,
            "Presiona ESC para continuar\nR para reiniciar",
            24, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -20), new Vector2(500, 80)).color = new Color(0.8f, 0.8f, 0.8f);
    }

    private void CrearPanelDerrota()
    {
        panelDerrota = CrearPanelConFondo("PanelDerrota", canvas.transform, new Color(0.3f, 0, 0, 0.8f));
        panelDerrota.SetActive(false);

        CrearTexto("TituloDerrota", panelDerrota.transform,
            "DERROTA", 56, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 80), new Vector2(500, 80)).color = new Color(1f, 0.3f, 0.3f);

        textoPuntajeFinal = CrearTexto("PuntajeFinalDerrota", panelDerrota.transform,
            "Puntaje: 0", 32, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 10), new Vector2(400, 50));
        textoPuntajeFinal.color = Color.white;

        textoEstadisticas = CrearTexto("EstadisticasDerrota", panelDerrota.transform,
            "", 20, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -40), new Vector2(500, 60));
        textoEstadisticas.color = new Color(0.8f, 0.8f, 0.8f);

        CrearTexto("ReiniciarDerrota", panelDerrota.transform,
            "Presiona R para reiniciar", 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -100), new Vector2(400, 40)).color = new Color(1f, 0.85f, 0f);
    }

    private void CrearPanelVictoria()
    {
        panelVictoria = CrearPanelConFondo("PanelVictoria", canvas.transform, new Color(0, 0.2f, 0, 0.8f));
        panelVictoria.SetActive(false);

        CrearTexto("TituloVictoria", panelVictoria.transform,
            "¡VICTORIA!", 56, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 80), new Vector2(500, 80)).color = new Color(0.3f, 1f, 0.3f);

        CrearTexto("SubVictoria", panelVictoria.transform,
            "¡Has sobrevivido todas las oleadas!", 24, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 20), new Vector2(500, 40)).color = Color.white;

        CrearTexto("ReiniciarVictoria", panelVictoria.transform,
            "Presiona R para jugar de nuevo", 22, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -60), new Vector2(400, 40)).color = new Color(1f, 0.85f, 0f);
    }

    void Update()
    {
        // Reiniciar con R
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (GestorJuego.Instancia != null)
            {
                GestorJuego.Instancia.ReiniciarJuego();
            }
        }
    }

    // ===================== MÉTODOS PÚBLICOS DE ACTUALIZACIÓN =====================

    public void ActualizarBarraVida(float porcentaje)
    {
        if (barraVidaRelleno != null)
        {
            barraVidaRelleno.fillAmount = porcentaje;

            // Cambiar color según vida
            if (porcentaje > 0.6f)
                barraVidaRelleno.color = new Color(0.2f, 0.85f, 0.3f);
            else if (porcentaje > 0.3f)
                barraVidaRelleno.color = new Color(1f, 0.8f, 0.2f);
            else
                barraVidaRelleno.color = new Color(1f, 0.2f, 0.2f);
        }
    }

    public void ActualizarTextoVida(float actual, float maxima)
    {
        if (textoVida != null)
            textoVida.text = $"VIDA: {Mathf.CeilToInt(actual)} / {Mathf.CeilToInt(maxima)}";
    }

    public void ActualizarPuntaje(int puntos)
    {
        if (textoPuntaje != null)
            textoPuntaje.text = $"PUNTOS: {puntos}";
    }

    public void ActualizarCombo(int combo)
    {
        if (textoCombo != null)
        {
            textoCombo.text = combo > 1 ? $"¡COMBO x{combo}!" : "";
        }
    }

    public void ActualizarEliminaciones(int eliminaciones)
    {
        if (textoEliminaciones != null)
            textoEliminaciones.text = $"Eliminaciones: {eliminaciones}";
    }

    public void ActualizarOleada(int oleada, int enemigosRestantes)
    {
        if (textoOleada != null)
            textoOleada.text = $"OLEADA {oleada} | Enemigos: {enemigosRestantes}";
    }

    public void ActualizarEnfriamiento(float porcentaje)
    {
        if (barraEnfriamientoRelleno != null)
        {
            barraEnfriamientoRelleno.fillAmount = porcentaje;
            barraEnfriamientoRelleno.color = porcentaje >= 1f
                ? new Color(0.3f, 1f, 0.3f, 0.9f)
                : new Color(0.3f, 0.7f, 1f, 0.9f);
        }
        if (textoEnfriamiento != null)
        {
            textoEnfriamiento.text = porcentaje >= 1f ? "LISTO" : "RECARGA";
        }
    }

    public void MostrarPantalaPausa(bool mostrar)
    {
        if (panelPausa != null) panelPausa.SetActive(mostrar);
    }

    public void MostrarPantallaDerrota()
    {
        if (panelDerrota != null)
        {
            panelDerrota.SetActive(true);
            if (textoPuntajeFinal != null && SistemaPuntaje.Instancia != null)
            {
                textoPuntajeFinal.text = $"Puntaje Final: {SistemaPuntaje.Instancia.puntajeActual}";
                textoEstadisticas.text = $"Enemigos eliminados: {SistemaPuntaje.Instancia.enemigosEliminados}\n"
                    + $"Oleada alcanzada: {GestorJuego.Instancia.ObtenerOleadaActual()}";
            }
        }
    }

    public void MostrarPantallaVictoria()
    {
        if (panelVictoria != null) panelVictoria.SetActive(true);
    }

    // ===================== UTILIDADES PARA CREAR UI =====================

    private GameObject CrearPanel(string nombre, Transform padre)
    {
        GameObject panel = new GameObject(nombre);
        panel.transform.SetParent(padre, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private GameObject CrearPanelConFondo(string nombre, Transform padre, Color color)
    {
        GameObject panel = CrearPanel(nombre, padre);
        Image img = panel.AddComponent<Image>();
        img.color = color;
        return panel;
    }

    private Text CrearTexto(string nombre, Transform padre, string contenido,
        int tamanio, TextAnchor alineacion,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 posicion, Vector2 tamanioRect)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamanioRect;

        Text texto = obj.AddComponent<Text>();
        texto.text = contenido;
        texto.fontSize = tamanio;
        texto.alignment = alineacion;
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.horizontalOverflow = HorizontalWrapMode.Overflow;
        texto.verticalOverflow = VerticalWrapMode.Overflow;

        // Agregar sombra para legibilidad
        Shadow sombra = obj.AddComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.8f);
        sombra.effectDistance = new Vector2(1, -1);

        return texto;
    }

    private Image CrearImagen(string nombre, Transform padre, Color color)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        RectTransform rt = obj.AddComponent<RectTransform>();

        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;

        return img;
    }
}
