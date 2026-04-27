using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 🎛️ PANEL INTERACTIVO DE FÍSICA
/// Permite modificar en tiempo real: Fuerza, Gravedad, Masa y Drag del proyectil.
/// Tecla F = Mostrar/Ocultar panel con cursor desbloqueado para editar sliders.
/// REQUIERE: EventSystem en la escena (se crea automáticamente si no existe).
/// </summary>
public class PanelFisicaUI : MonoBehaviour
{
    public static PanelFisicaUI Instancia { get; private set; }

    [Header("=== PARÁMETROS EDITABLES EN TIEMPO REAL ===")]
    public float fuerzaDisparo = 30f;
    public float gravedadProyectil = 9.81f;
    public float masaProyectil = 2f;
    public float dragProyectil = 0.1f;

    // Componentes UI
    private Canvas canvas;
    private GameObject panelContenedor;
    private bool panelVisible = false;  // Empieza oculto

    // Sliders
    private Slider sliderFuerza;
    private Slider sliderGravedad;
    private Slider sliderMasa;
    private Slider sliderDrag;

    // Textos de valor
    private Text textoValorFuerza;
    private Text textoValorGravedad;
    private Text textoValorMasa;
    private Text textoValorDrag;

    // Texto informativo
    private Text textoInfoFisica;

    void Awake()
    {
        if (Instancia == null)
            Instancia = this;
        else { Destroy(gameObject); return; }

        // CRÍTICO: Crear EventSystem si no existe (sin esto los sliders no responden)
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[PanelFisica] EventSystem creado automáticamente.");
        }

        CrearPanelCompleto();
        panelContenedor.SetActive(false); // Empieza oculto
    }

    void Update()
    {
        // Toggle del panel con F (funciona siempre, incluso en pausa)
        if (Input.GetKeyDown(KeyCode.F))
        {
            panelVisible = !panelVisible;
            panelContenedor.SetActive(panelVisible);

            // Cursor: visible y libre cuando el panel está abierto
            Cursor.visible = panelVisible;
            Cursor.lockState = panelVisible ? CursorLockMode.None : CursorLockMode.None;
        }

        if (!panelVisible) return;

        // Leer valores de los sliders continuamente
        fuerzaDisparo = sliderFuerza.value;
        gravedadProyectil = sliderGravedad.value;
        masaProyectil = sliderMasa.value;
        dragProyectil = sliderDrag.value;

        // Actualizar textos
        textoValorFuerza.text = fuerzaDisparo.ToString("F1") + " N";
        textoValorGravedad.text = gravedadProyectil.ToString("F1") + " m/s²";
        textoValorMasa.text = masaProyectil.ToString("F2") + " kg";
        textoValorDrag.text = dragProyectil.ToString("F2");

        // Información calculada
        float vel = fuerzaDisparo;
        float alcance = (vel * vel) / Mathf.Max(gravedadProyectil, 0.01f);
        float energia = 0.5f * masaProyectil * vel * vel;
        textoInfoFisica.text =
            $"Vel. Inicial: {vel:F1} m/s\n" +
            $"Alcance teórico: {alcance:F1} m\n" +
            $"Energía cinética: {energia:F0} J\n" +
            $"Momento: {masaProyectil * vel:F1} kg·m/s";
    }

    private void CrearPanelCompleto()
    {
        // Canvas
        canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;

        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // Panel contenedor (derecha de la pantalla)
        panelContenedor = new GameObject("PanelFisicaContenedor");
        panelContenedor.transform.SetParent(canvas.transform, false);

        RectTransform rtPanel = panelContenedor.AddComponent<RectTransform>();
        rtPanel.anchorMin = new Vector2(1, 0.15f);
        rtPanel.anchorMax = new Vector2(1, 0.9f);
        rtPanel.pivot = new Vector2(1, 0.5f);
        rtPanel.anchoredPosition = new Vector2(-15, 0);
        rtPanel.sizeDelta = new Vector2(340, 0);

        Image fondoPanel = panelContenedor.AddComponent<Image>();
        fondoPanel.color = new Color(0.03f, 0.06f, 0.12f, 0.95f);

        VerticalLayoutGroup layout = panelContenedor.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 16, 16);
        layout.spacing = 6;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        ContentSizeFitter fitter = panelContenedor.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ═══ TÍTULO ═══
        CrearTextoLayout(panelContenedor.transform, "🎛️ PANEL DE FÍSICA", 20,
            new Color(0.2f, 0.85f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold, 32);

        CrearSeparador(panelContenedor.transform, new Color(0.2f, 0.6f, 1f, 0.5f));

        CrearTextoLayout(panelContenedor.transform, "Arrastra los sliders para cambiar la física", 12,
            new Color(0.6f, 0.6f, 0.7f), TextAnchor.MiddleCenter, FontStyle.Italic, 20);

        // ═══ SLIDER: FUERZA ═══
        sliderFuerza = CrearSliderCompleto(panelContenedor.transform,
            "⚡ FUERZA DE DISPARO", 5f, 80f, fuerzaDisparo,
            new Color(1f, 0.4f, 0.2f), out textoValorFuerza);

        // ═══ SLIDER: GRAVEDAD ═══
        sliderGravedad = CrearSliderCompleto(panelContenedor.transform,
            "🌍 GRAVEDAD", 0f, 30f, gravedadProyectil,
            new Color(0.2f, 0.7f, 1f), out textoValorGravedad);

        // ═══ SLIDER: MASA ═══
        sliderMasa = CrearSliderCompleto(panelContenedor.transform,
            "⚖️ MASA", 0.1f, 10f, masaProyectil,
            new Color(0.2f, 1f, 0.4f), out textoValorMasa);

        // ═══ SLIDER: DRAG ═══
        sliderDrag = CrearSliderCompleto(panelContenedor.transform,
            "💨 RESISTENCIA (DRAG)", 0f, 5f, dragProyectil,
            new Color(1f, 0.85f, 0.2f), out textoValorDrag);

        CrearSeparador(panelContenedor.transform, new Color(0.2f, 0.6f, 1f, 0.3f));

        // ═══ INFO CALCULADA ═══
        textoInfoFisica = CrearTextoLayout(panelContenedor.transform,
            "Dispara para ver el efecto", 13,
            new Color(0.8f, 0.9f, 1f), TextAnchor.MiddleLeft, FontStyle.Normal, 70);

        CrearSeparador(panelContenedor.transform, new Color(0.3f, 0.3f, 0.4f, 0.3f));

        // ═══ INSTRUCCIONES ═══
        CrearTextoLayout(panelContenedor.transform,
            "[F] Mostrar/Ocultar   [Espacio] Disparar", 11,
            new Color(0.4f, 0.4f, 0.5f), TextAnchor.MiddleCenter, FontStyle.Italic, 22);
    }

    /// <summary>
    /// Crea un slider con etiqueta, valor y barra de color.
    /// </summary>
    private Slider CrearSliderCompleto(Transform padre, string etiqueta,
        float min, float max, float valorInicial, Color colorBarra, out Text textoValor)
    {
        // Contenedor del slider
        GameObject contenedor = new GameObject($"Grupo_{etiqueta}");
        contenedor.transform.SetParent(padre, false);
        LayoutElement leContenedor = contenedor.AddComponent<LayoutElement>();
        leContenedor.preferredHeight = 60;
        leContenedor.minHeight = 60;

        // --- Línea superior: Etiqueta + Valor ---
        GameObject lineaSup = new GameObject("LineaSuperior");
        lineaSup.transform.SetParent(contenedor.transform, false);
        RectTransform rtLinea = lineaSup.AddComponent<RectTransform>();
        rtLinea.anchorMin = new Vector2(0, 0.55f);
        rtLinea.anchorMax = new Vector2(1, 1);
        rtLinea.offsetMin = Vector2.zero;
        rtLinea.offsetMax = Vector2.zero;

        // Etiqueta (izquierda)
        Text txtEtiqueta = CrearTextoAnclado(lineaSup.transform, etiqueta, 13,
            new Color(0.85f, 0.85f, 0.9f), TextAnchor.MiddleLeft, FontStyle.Bold,
            new Vector2(0, 0), new Vector2(0.65f, 1));

        // Valor (derecha)
        textoValor = CrearTextoAnclado(lineaSup.transform, valorInicial.ToString("F1"), 14,
            colorBarra, TextAnchor.MiddleRight, FontStyle.Bold,
            new Vector2(0.65f, 0), new Vector2(1, 1));

        // --- Slider ---
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(contenedor.transform, false);
        RectTransform rtSlider = sliderObj.AddComponent<RectTransform>();
        rtSlider.anchorMin = new Vector2(0, 0);
        rtSlider.anchorMax = new Vector2(1, 0.5f);
        rtSlider.offsetMin = new Vector2(0, 5);
        rtSlider.offsetMax = new Vector2(0, -2);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = valorInicial;
        slider.wholeNumbers = false;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform rtBg = bg.AddComponent<RectTransform>();
        rtBg.anchorMin = new Vector2(0, 0.25f);
        rtBg.anchorMax = new Vector2(1, 0.75f);
        rtBg.offsetMin = Vector2.zero;
        rtBg.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);

        // Fill Area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform rtFA = fillArea.AddComponent<RectTransform>();
        rtFA.anchorMin = new Vector2(0, 0.25f);
        rtFA.anchorMax = new Vector2(1, 0.75f);
        rtFA.offsetMin = Vector2.zero;
        rtFA.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform rtFill = fill.AddComponent<RectTransform>();
        rtFill.anchorMin = Vector2.zero;
        rtFill.anchorMax = Vector2.one;
        rtFill.offsetMin = Vector2.zero;
        rtFill.offsetMax = Vector2.zero;
        fill.AddComponent<Image>().color = colorBarra;
        slider.fillRect = rtFill;

        // Handle Area
        GameObject handleArea = new GameObject("HandleArea");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform rtHA = handleArea.AddComponent<RectTransform>();
        rtHA.anchorMin = Vector2.zero;
        rtHA.anchorMax = Vector2.one;
        rtHA.offsetMin = new Vector2(10, 0);
        rtHA.offsetMax = new Vector2(-10, 0);

        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform rtH = handle.AddComponent<RectTransform>();
        rtH.sizeDelta = new Vector2(20, 28);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        slider.handleRect = rtH;
        slider.targetGraphic = handleImg;

        return slider;
    }

    // ═══ UTILIDADES UI ═══

    private Text CrearTextoLayout(Transform padre, string contenido, int tamano,
        Color color, TextAnchor alineacion, FontStyle estilo, float altura)
    {
        GameObject obj = new GameObject("TxtLayout");
        obj.transform.SetParent(padre, false);
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = altura;
        le.minHeight = altura;

        Text txt = obj.AddComponent<Text>();
        txt.text = contenido;
        txt.fontSize = tamano;
        txt.color = color;
        txt.alignment = alineacion;
        txt.fontStyle = estilo;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        return txt;
    }

    private Text CrearTextoAnclado(Transform padre, string contenido, int tamano,
        Color color, TextAnchor alineacion, FontStyle estilo,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject("TxtAnclado");
        obj.transform.SetParent(padre, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Text txt = obj.AddComponent<Text>();
        txt.text = contenido;
        txt.fontSize = tamano;
        txt.color = color;
        txt.alignment = alineacion;
        txt.fontStyle = estilo;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        return txt;
    }

    private void CrearSeparador(Transform padre, Color color)
    {
        GameObject sep = new GameObject("Separador");
        sep.transform.SetParent(padre, false);
        LayoutElement le = sep.AddComponent<LayoutElement>();
        le.preferredHeight = 2;
        le.minHeight = 2;
        sep.AddComponent<Image>().color = color;
    }
}
