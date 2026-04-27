using UnityEngine;

/// <summary>
/// Controlador del tanque del jugador.
/// MEJORADO: Movimiento más rápido, rotación más ágil, respuesta inmediata.
/// </summary>
public class ControladorTanque : MonoBehaviour
{
    [Header("=== MOVIMIENTO (AUMENTADO) ===")]
    public float velocidadMovimiento = 12f;   // Antes: 12
    public float velocidadRotacion = 100f;     // Antes: 100

    [Header("=== TORRETA ===")]
    public Transform torreta;
    public float velocidadTorreta = 160f;      // Antes: 120

    [Header("=== AUDIO ===")]
    public AudioClip sonidoMotor;

    private Rigidbody cuerpoRigido;
    private AudioSource fuenteAudioMotor;
    private float entradaMovimiento;
    private float entradaRotacion;

    void Awake()
    {
        cuerpoRigido = GetComponent<Rigidbody>();
        if (cuerpoRigido == null)
            cuerpoRigido = gameObject.AddComponent<Rigidbody>();

        // Tanque más ágil: menos masa, menos fricción
        cuerpoRigido.mass = 40f;               // Antes: 50
        cuerpoRigido.drag = 1.5f;              // Antes: 2
        cuerpoRigido.angularDrag = 4f;          // Antes: 5
        cuerpoRigido.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        cuerpoRigido.interpolation = RigidbodyInterpolation.Interpolate;

        fuenteAudioMotor = GetComponent<AudioSource>();
        if (fuenteAudioMotor == null)
            fuenteAudioMotor = gameObject.AddComponent<AudioSource>();

        if (sonidoMotor != null)
        {
            fuenteAudioMotor.clip = sonidoMotor;
            fuenteAudioMotor.loop = true;
            fuenteAudioMotor.volume = 0.3f;
            fuenteAudioMotor.Play();
        }
    }

    void Update()
    {
        entradaMovimiento = Input.GetAxis("Vertical");
        entradaRotacion = Input.GetAxis("Horizontal");

        // Torreta con Q/E
        float rotTorreta = 0f;
        if (Input.GetKey(KeyCode.Q)) rotTorreta = -1f;
        if (Input.GetKey(KeyCode.E)) rotTorreta = 1f;

        if (torreta != null)
            torreta.Rotate(Vector3.up, rotTorreta * velocidadTorreta * Time.deltaTime);

        // Audio dinámico
        if (fuenteAudioMotor != null && fuenteAudioMotor.isPlaying)
        {
            float vel = cuerpoRigido.velocity.magnitude;
            fuenteAudioMotor.pitch = Mathf.Lerp(0.8f, 1.5f, vel / velocidadMovimiento);
        }
    }

    void FixedUpdate()
    {
        // Movimiento más directo y responsivo
        Vector3 mov = transform.forward * entradaMovimiento * velocidadMovimiento;
        cuerpoRigido.MovePosition(cuerpoRigido.position + mov * Time.fixedDeltaTime);

        // Rotación más rápida
        float rot = entradaRotacion * velocidadRotacion * Time.fixedDeltaTime;
        cuerpoRigido.MoveRotation(cuerpoRigido.rotation * Quaternion.Euler(0f, rot, 0f));
    }

    public Vector3 ObtenerDireccionDisparo()
    {
        return torreta != null ? torreta.forward : transform.forward;
    }
}
