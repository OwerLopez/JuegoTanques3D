using UnityEngine;

/// <summary>
/// Script de configuración automática de la escena completa.
/// Genera todo el contenido del juego al presionar Play.
/// Adjuntar a un GameObject vacío llamado "ConfiguradorEscena".
/// </summary>
public class ConfiguradorEscena : MonoBehaviour
{
    [Header("=== CONFIGURACIÓN ===")]
    [Tooltip("Marcar para regenerar la escena al iniciar (Play Mode)")]
    public bool generarAlIniciar = true;

    [Tooltip("¿Limpiar objetos viejos antes de generar?")]
    public bool limpiarAlGenerar = true;

    // Referencia global al proyectil (se crea primero)
    private GameObject moldeProyectil;

    void Awake()
    {
        if (generarAlIniciar)
        {
            GenerarEscenaCompleta();
        }
    }

    /// <summary>
    /// Genera toda la escena desde código en el orden correcto.
    /// HAZ CLIC DERECHO EN EL SCRIPT Y ELIGE "Generar Escena Permanente"
    /// </summary>
    [ContextMenu("Generar Escena Permanente")]
    public void GenerarEscenaCompleta()
    {
        if (limpiarAlGenerar) LimpiarEscenaExistente();

        Debug.Log("=== GENERANDO ESCENA DE COMBATE DE TANQUES ===");

        // PASO 1: Crear el molde del proyectil PRIMERO
        moldeProyectil = CrearMoldeProyectil();

        // PASO 2: Terreno
        CrearSuelo();

        // PASO 3: Iluminación
        ConfigurarIluminacion();

        // PASO 4: Tanque del jugador (usa moldeProyectil)
        GameObject tanqueJugador = CrearTanqueJugador();

        // PASO 5: Obstáculos
        CrearObstaculos();

        // PASO 6: Muros perimetrales
        CrearMurosPerimetro();

        // PASO 7: Prefab del enemigo (usa moldeProyectil)
        GameObject prefabEnemigo = CrearPrefabEnemigo();

        // PASO 8: Puntos de spawn
        Transform[] puntosSpawn = CrearPuntosSpawn();

        // PASO 9: Cámara
        ConfigurarCamara(tanqueJugador.transform);

        // PASO 10: Sistemas del juego (Puntaje, UI, Gestor, Panel Física)
        CrearSistemasJuego(tanqueJugador, prefabEnemigo, puntosSpawn);

        Debug.Log("=== ESCENA GENERADA EXITOSAMENTE ===");
    }

    // ==================== PROYECTIL (SE CREA PRIMERO) ====================
    private GameObject CrearMoldeProyectil()
    {
        GameObject proy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        proy.name = "MoldeProyectil";
        proy.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        proy.transform.position = new Vector3(0, -500, 0); // Oculto

        // Material brillante dorado
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.8f, 0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f) * 2f);
        proy.GetComponent<Renderer>().material = mat;

        // Física
        Rigidbody rb = proy.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // CRÍTICO: Desactivar ANTES de añadir Proyectil.cs
        // Si el objeto está activo, Proyectil.Awake() ejecuta Destroy(gameObject, 6)
        // lo que destruye el molde después de 6 segundos y las balas dejan de funcionar.
        proy.SetActive(false);

        // Ahora sí añadir el script (Awake NO se ejecuta en objetos inactivos)
        proy.AddComponent<Proyectil>();

        Debug.Log("[Configurador] Molde de proyectil creado correctamente.");
        return proy;
    }

    // ==================== SUELO ====================
    private void CrearSuelo()
    {
        GameObject suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = "Terreno";
        suelo.transform.position = Vector3.zero;
        suelo.transform.localScale = new Vector3(10, 1, 10);

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.35f, 0.5f, 0.3f);
        suelo.GetComponent<Renderer>().material = mat;
        suelo.isStatic = true;
    }

    // ==================== ILUMINACIÓN ====================
    private void ConfigurarIluminacion()
    {
        GameObject luzObj = new GameObject("LuzSolar");
        Light luz = luzObj.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.color = new Color(1f, 0.95f, 0.85f);
        luz.intensity = 1.2f;
        luz.shadows = LightShadows.Soft;
        luzObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientLight = new Color(0.4f, 0.45f, 0.5f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.7f, 0.75f, 0.8f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 120f;
    }

    // ==================== TANQUE DEL JUGADOR ====================
    private GameObject CrearTanqueJugador()
    {
        GameObject tanque = new GameObject("TanqueJugador");
        tanque.tag = "Player";
        tanque.transform.position = new Vector3(0f, 0.8f, 0f);

        // Chasis
        GameObject chasis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chasis.name = "Chasis";
        chasis.transform.SetParent(tanque.transform);
        chasis.transform.localPosition = Vector3.zero;
        chasis.transform.localScale = new Vector3(2.5f, 0.8f, 3.5f);

        Material matVerde = new Material(Shader.Find("Standard"));
        matVerde.color = new Color(0.25f, 0.4f, 0.25f);
        chasis.GetComponent<Renderer>().material = matVerde;

        // Torreta
        GameObject torreta = new GameObject("Torreta");
        torreta.transform.SetParent(tanque.transform);
        torreta.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        // Cabina
        GameObject cabina = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabina.name = "Cabina";
        cabina.transform.SetParent(torreta.transform);
        cabina.transform.localPosition = Vector3.zero;
        cabina.transform.localScale = new Vector3(1.8f, 0.6f, 1.8f);

        Material matTorreta = new Material(Shader.Find("Standard"));
        matTorreta.color = new Color(0.2f, 0.35f, 0.2f);
        cabina.GetComponent<Renderer>().material = matTorreta;

        // Cañón
        GameObject canon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        canon.name = "Canon";
        canon.transform.SetParent(torreta.transform);
        canon.transform.localPosition = new Vector3(0f, 0.1f, 1.5f);
        canon.transform.localScale = new Vector3(0.25f, 1.2f, 0.25f);
        canon.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        Material matCanon = new Material(Shader.Find("Standard"));
        matCanon.color = new Color(0.15f, 0.15f, 0.15f);
        canon.GetComponent<Renderer>().material = matCanon;

        // Limpiar colliders internos
        Destroy(cabina.GetComponent<Collider>());
        Destroy(canon.GetComponent<Collider>());

        // Punto de disparo (al final del cañón)
        GameObject puntoDisparo = new GameObject("PuntoDisparo");
        puntoDisparo.transform.SetParent(torreta.transform);
        puntoDisparo.transform.localPosition = new Vector3(0f, 0.1f, 3.2f);

        // BoxCollider del tanque
        BoxCollider col = tanque.AddComponent<BoxCollider>();
        col.size = new Vector3(2.5f, 1.5f, 3.5f);
        col.center = new Vector3(0f, 0.2f, 0f);

        // Rigidbody
        tanque.AddComponent<Rigidbody>();

        // Script de movimiento
        ControladorTanque ctrl = tanque.AddComponent<ControladorTanque>();
        ctrl.torreta = torreta.transform;

        // Script de disparo — ASIGNACIÓN CRÍTICA del proyectil
        SistemaDisparo disparo = tanque.AddComponent<SistemaDisparo>();
        disparo.puntoDisparo = puntoDisparo.transform;
        disparo.prefabProyectil = moldeProyectil;  // ← AQUÍ se conecta

        Debug.Log($"[Configurador] Disparo configurado: prefab={moldeProyectil != null}, punto={puntoDisparo != null}");

        // Sistema de vida
        SistemaVida vida = tanque.AddComponent<SistemaVida>();
        vida.vidaMaxima = 150f;
        vida.regeneraVida = true;
        vida.velocidadRegeneracion = 3f;
        vida.retardoRegeneracion = 8f;

        return tanque;
    }

    // ==================== PREFAB ENEMIGO ====================
    private GameObject CrearPrefabEnemigo()
    {
        GameObject enemigo = new GameObject("PrefabEnemigo");
        enemigo.tag = "Enemigo";
        enemigo.transform.position = new Vector3(0, -500, 0); // Oculto

        // Chasis enemigo
        GameObject cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cuerpo.name = "ChasisEnemigo";
        cuerpo.transform.SetParent(enemigo.transform);
        cuerpo.transform.localPosition = Vector3.zero;
        cuerpo.transform.localScale = new Vector3(2.2f, 0.7f, 3f);

        Material matRojo = new Material(Shader.Find("Standard"));
        matRojo.color = new Color(0.5f, 0.2f, 0.15f);
        cuerpo.GetComponent<Renderer>().material = matRojo;

        // Torreta enemiga
        GameObject torretaEnem = new GameObject("TorretaEnemiga");
        torretaEnem.transform.SetParent(enemigo.transform);
        torretaEnem.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        GameObject cabinaEnem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabinaEnem.name = "CabinaEnemiga";
        cabinaEnem.transform.SetParent(torretaEnem.transform);
        cabinaEnem.transform.localPosition = Vector3.zero;
        cabinaEnem.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);

        Material matTorrEnem = new Material(Shader.Find("Standard"));
        matTorrEnem.color = new Color(0.45f, 0.15f, 0.1f);
        cabinaEnem.GetComponent<Renderer>().material = matTorrEnem;

        // Cañón enemigo
        GameObject canonEnem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        canonEnem.name = "CanonEnemigo";
        canonEnem.transform.SetParent(torretaEnem.transform);
        canonEnem.transform.localPosition = new Vector3(0f, 0f, 1.2f);
        canonEnem.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
        canonEnem.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        Material matCanonE = new Material(Shader.Find("Standard"));
        matCanonE.color = new Color(0.1f, 0.1f, 0.1f);
        canonEnem.GetComponent<Renderer>().material = matCanonE;

        // Limpiar colliders internos
        Destroy(cabinaEnem.GetComponent<Collider>());
        Destroy(canonEnem.GetComponent<Collider>());

        // Punto de disparo
        GameObject puntoDisparoEnem = new GameObject("PuntoDisparoEnemigo");
        puntoDisparoEnem.transform.SetParent(torretaEnem.transform);
        puntoDisparoEnem.transform.localPosition = new Vector3(0f, 0f, 2.5f);

        // Collider
        BoxCollider colEnem = enemigo.AddComponent<BoxCollider>();
        colEnem.size = new Vector3(2.2f, 1.3f, 3f);
        colEnem.center = new Vector3(0f, 0.15f, 0f);

        // Rigidbody
        enemigo.AddComponent<Rigidbody>();

        // IA — ASIGNACIÓN CRÍTICA del proyectil
        EnemigoIA ia = enemigo.AddComponent<EnemigoIA>();
        ia.puntoDisparoEnemigo = puntoDisparoEnem.transform;
        ia.prefabProyectilEnemigo = moldeProyectil;  // ← AQUÍ se conecta

        Debug.Log($"[Configurador] IA Enemiga configurada: proyectil={moldeProyectil != null}");

        // Vida
        SistemaVida vidaEnem = enemigo.AddComponent<SistemaVida>();
        vidaEnem.vidaMaxima = 80f;

        // Desactivar (es un molde para instanciar)
        enemigo.SetActive(false);

        return enemigo;
    }

    // ==================== OBSTÁCULOS ====================
    private void CrearObstaculos()
    {
        GameObject grupo = new GameObject("Obstaculos");

        Material matCaja = new Material(Shader.Find("Standard"));
        matCaja.color = new Color(0.6f, 0.5f, 0.35f);

        Material matRoca = new Material(Shader.Find("Standard"));
        matRoca.color = new Color(0.45f, 0.42f, 0.38f);

        Material matEdif = new Material(Shader.Find("Standard"));
        matEdif.color = new Color(0.5f, 0.47f, 0.43f);

        // Cajas
        Vector3[] posCajas = {
            new Vector3(10f, 1f, 10f), new Vector3(-10f, 1f, 15f),
            new Vector3(15f, 1f, -10f), new Vector3(-15f, 1f, -15f),
            new Vector3(5f, 1f, -20f), new Vector3(-5f, 1f, 25f),
            new Vector3(20f, 1f, 5f), new Vector3(-20f, 1f, -5f),
        };
        for (int i = 0; i < posCajas.Length; i++)
        {
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = $"Caja_{i + 1}";
            c.transform.SetParent(grupo.transform);
            c.transform.position = posCajas[i];
            float s = Random.Range(1.5f, 3f);
            c.transform.localScale = new Vector3(s, Random.Range(1.5f, 3f), s);
            c.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 90f), 0f);
            c.GetComponent<Renderer>().material = matCaja;
            c.isStatic = true;
            c.AddComponent<Rigidbody>().isKinematic = true;
        }

        // Rocas
        Vector3[] posRocas = {
            new Vector3(25f, 0.8f, 20f), new Vector3(-25f, 0.8f, -20f),
            new Vector3(30f, 0.8f, -15f), new Vector3(-30f, 0.8f, 10f),
        };
        for (int i = 0; i < posRocas.Length; i++)
        {
            GameObject r = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            r.name = $"Roca_{i + 1}";
            r.transform.SetParent(grupo.transform);
            r.transform.position = posRocas[i];
            float sr = Random.Range(2f, 3.5f);
            r.transform.localScale = new Vector3(sr * 1.1f, sr * 0.6f, sr * 1.1f);
            r.GetComponent<Renderer>().material = matRoca;
            r.isStatic = true;
            r.AddComponent<Rigidbody>().isKinematic = true;
        }

        // Edificios
        Vector3[] posEdif = {
            new Vector3(35f, 2.5f, 0f), new Vector3(-35f, 2.5f, 0f),
            new Vector3(0f, 2.5f, -35f), new Vector3(20f, 2.5f, 25f),
        };
        for (int i = 0; i < posEdif.Length; i++)
        {
            GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
            e.name = $"Edificio_{i + 1}";
            e.transform.SetParent(grupo.transform);
            e.transform.position = posEdif[i];
            e.transform.localScale = new Vector3(Random.Range(4f, 8f), Random.Range(3f, 6f), Random.Range(4f, 8f));
            e.GetComponent<Renderer>().material = matEdif;
            e.isStatic = true;
            e.AddComponent<Rigidbody>().isKinematic = true;
        }
    }

    // ==================== MUROS ====================
    private void CrearMurosPerimetro()
    {
        GameObject muros = new GameObject("MurosPerimetro");
        float tam = 48f;
        float alt = 4f;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.35f, 0.33f, 0.3f);

        CrearMuro(muros.transform, "MuroNorte", new Vector3(0, alt / 2, tam), new Vector3(tam * 2, alt, 1), mat);
        CrearMuro(muros.transform, "MuroSur", new Vector3(0, alt / 2, -tam), new Vector3(tam * 2, alt, 1), mat);
        CrearMuro(muros.transform, "MuroEste", new Vector3(tam, alt / 2, 0), new Vector3(1, alt, tam * 2), mat);
        CrearMuro(muros.transform, "MuroOeste", new Vector3(-tam, alt / 2, 0), new Vector3(1, alt, tam * 2), mat);
    }

    private void CrearMuro(Transform padre, string nombre, Vector3 pos, Vector3 esc, Material mat)
    {
        GameObject m = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m.name = nombre;
        m.transform.SetParent(padre);
        m.transform.position = pos;
        m.transform.localScale = esc;
        m.GetComponent<Renderer>().material = mat;
        m.isStatic = true;
        m.AddComponent<Rigidbody>().isKinematic = true;
    }

    // ==================== PUNTOS DE SPAWN ====================
    private Transform[] CrearPuntosSpawn()
    {
        GameObject grupo = new GameObject("PuntosSpawn");
        Vector3[] posiciones = {
            new Vector3(35f, 0.8f, 35f), new Vector3(-35f, 0.8f, 35f),
            new Vector3(35f, 0.8f, -35f), new Vector3(-35f, 0.8f, -35f),
            new Vector3(0f, 0.8f, 40f), new Vector3(40f, 0.8f, 0f),
        };

        Transform[] puntos = new Transform[posiciones.Length];
        for (int i = 0; i < posiciones.Length; i++)
        {
            GameObject p = new GameObject($"Spawn_{i + 1}");
            p.transform.SetParent(grupo.transform);
            p.transform.position = posiciones[i];
            puntos[i] = p.transform;
        }
        return puntos;
    }

    // ==================== CÁMARA ====================
    private void ConfigurarCamara(Transform objetivo)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("CamaraPrincipal");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
        }

        CamaraSeguimiento seg = cam.gameObject.GetComponent<CamaraSeguimiento>();
        if (seg == null)
            seg = cam.gameObject.AddComponent<CamaraSeguimiento>();

        seg.objetivo = objetivo;
        cam.transform.position = objetivo.position + new Vector3(0f, 12f, -8f);
        cam.transform.LookAt(objetivo);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.5f, 0.6f, 0.75f);
    }

    // ==================== SISTEMAS DEL JUEGO ====================
    private void CrearSistemasJuego(GameObject tanqueJugador, GameObject prefabEnemigo, Transform[] puntosSpawn)
    {
        // Puntaje
        new GameObject("SistemaPuntaje").AddComponent<SistemaPuntaje>();

        // UI del juego
        GameObject objUI = new GameObject("InterfazUsuario");
        objUI.AddComponent<InterfazUsuario>();

        // Gestor del juego
        GestorJuego gestor = new GameObject("GestorJuego").AddComponent<GestorJuego>();
        gestor.jugadorTransform = tanqueJugador.transform;
        gestor.prefabEnemigo = prefabEnemigo;
        gestor.puntosSpawn = puntosSpawn;
        gestor.interfazUsuario = objUI.GetComponent<InterfazUsuario>();
        gestor.enemigosPorOleada = 3;
        gestor.incrementoPorOleada = 1;
        gestor.tiempoEntreOleadas = 8f;
        gestor.maximoOleadas = 5;

        // 🎛️ PANEL DE FÍSICA INTERACTIVO
        new GameObject("PanelFisicaUI").AddComponent<PanelFisicaUI>();

        Debug.Log("[Configurador] Todos los sistemas creados correctamente.");
    }

    /// <summary>
    /// Elimina objetos generados previamente para evitar duplicados.
    /// </summary>
    private void LimpiarEscenaExistente()
    {
        string[] nombresAborrar = {
            "Terreno", "LuzSolar", "TanqueJugador", "Obstaculos",
            "MurosPerimetro", "PrefabEnemigo", "PuntosSpawn",
            "SistemaPuntaje", "InterfazUsuario", "GestorJuego",
            "PanelFisicaUI", "MoldeProyectil", "CanvasAnuncio", "EventSystem"
        };

        foreach (string nombre in nombresAborrar)
        {
            GameObject obj = GameObject.Find(nombre);
            if (obj != null) DestroyImmediate(obj);
        }
    }
}
