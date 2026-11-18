using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class AE86CarController : MonoBehaviour
{
    [Header("Diagnostyka (Konsola)")]
    public bool debugLogs = true;
    public float logInterval = 0.5f;

    [Header("Auto-Dopasowanie Modelu (Przeciągnij tutaj)")]
    public Transform carModel; // Obiekt AE86Z
    public Transform wheelMeshFL; // TiaraGT83_WheelTuner_FL
    public Transform wheelMeshFR; // TiaraGT83_WheelTuner_FR
    public Transform wheelMeshRL; // TiaraGT83_WheelTuner_RL
    public Transform wheelMeshRR; // TiaraGT83_WheelTuner_RR

    [Header("Parametry Jazdy")]
    public float motorForce = 1000f;
    public float brakeForce = 4000f;
    public float maxSteerAngle = 32f;
    public Vector3 centerOfMass = new Vector3(0f, -0.55f, 0f);

    [Header("Wheel Collidery")]
    public WheelCollider wcFL;
    public WheelCollider wcFR;
    public WheelCollider wcRL;
    public WheelCollider wcRR;

    [Header("Kontenery Wizualne Kół (Vis_...)")]
    public Transform visFL;
    public Transform visFR;
    public Transform visRL;
    public Transform visRR;

    [Header("Detale Auta (Opcjonalne)")]
    public Transform caliperFL;
    public Transform caliperFR;
    public Transform steeringWheel;
    public float steeringWheelMaxAngle = 180f;
    public Vector3 steeringWheelAxis = new Vector3(0f, 0f, -1f);

    [Header("Światła Stopu")]
    public MeshRenderer brakeLightsRenderer;
    public Material brakeOffMaterial;
    public Material brakeOnMaterial;

    private Rigidbody rb;
    private float currentThrottle;
    private float currentSteer;
    private bool isBraking;
    private Quaternion initialSteeringRotation;
    private float logTimer;

    private void Awake()
    {
        AutoSetupCar();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;

        if (steeringWheel != null)
        {
            initialSteeringRotation = steeringWheel.localRotation;
        }
    }

    [ContextMenu("Auto-Dopasuj Model i Koła")]
    public void AutoSetupCar()
    {
        if (carModel == null || wheelMeshFL == null || wheelMeshRL == null)
        {
            Debug.LogWarning("Przypisz obiekt AE86Z oraz 4 obiekty kół (Wheel Mesh) w Inspektorze!");
            return;
        }

        Quaternion savedRotation = transform.rotation;
        transform.rotation = Quaternion.identity;

        Transform[] wheelMeshes = { wheelMeshFL, wheelMeshFR, wheelMeshRL, wheelMeshRR };
        foreach (var wm in wheelMeshes)
        {
            if (wm != null) wm.SetParent(carModel, true);
        }

        Vector3 posFL = GetVisualCenter(wheelMeshFL);
        Vector3 posRL = GetVisualCenter(wheelMeshRL);
        Vector3 flatForward = Vector3.ProjectOnPlane(posFL - posRL, Vector3.up).normalized;

        if (flatForward.sqrMagnitude > 0.001f)
        {
            float angleToForward = Vector3.SignedAngle(flatForward, Vector3.forward, Vector3.up);
            carModel.RotateAround(transform.position, Vector3.up, angleToForward);
        }

        SetupSingleWheel(wcFL, visFL, wheelMeshFL);
        SetupSingleWheel(wcFR, visFR, wheelMeshFR);
        SetupSingleWheel(wcRL, visRL, wheelMeshRL);
        SetupSingleWheel(wcRR, visRR, wheelMeshRR);

        transform.rotation = savedRotation;
    }

    private void SetupSingleWheel(WheelCollider wc, Transform vis, Transform mesh)
    {
        if (wc == null || vis == null || mesh == null) return;

        Renderer rend = mesh.GetComponentInChildren<Renderer>();
        Vector3 center = rend != null ? rend.bounds.center : mesh.position;
        float radius = rend != null ? rend.bounds.extents.y : 0.35f;

        wc.transform.position = center;
        wc.transform.localRotation = Quaternion.identity;
        wc.radius = radius;
        wc.suspensionDistance = 0.15f;

        vis.position = center;
        vis.localRotation = Quaternion.identity;
        mesh.SetParent(vis, true);
    }

    private Vector3 GetVisualCenter(Transform target)
    {
        Renderer rend = target.GetComponentInChildren<Renderer>();
        return rend != null ? rend.bounds.center : target.position;
    }

    private void Update()
    {
        ReadInput();
        UpdateVisuals();

        if (debugLogs)
        {
            logTimer += Time.deltaTime;
            if (logTimer >= logInterval)
            {
                logTimer = 0f;
                PrintDiagnostics();
            }
        }
    }

    private void FixedUpdate()
    {
        ApplyPhysics();
    }

    private void ReadInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float targetThrottle = 0f;
        if (kb.wKey != null && kb.wKey.isPressed) targetThrottle += 1f;
        if (kb.sKey != null && kb.sKey.isPressed) targetThrottle -= 1f;

        float targetSteer = 0f;
        if (kb.dKey != null && kb.dKey.isPressed) targetSteer += 1f;
        if (kb.aKey != null && kb.aKey.isPressed) targetSteer -= 1f;

        currentThrottle = Mathf.MoveTowards(currentThrottle, targetThrottle, Time.deltaTime * 4f);
        currentSteer = Mathf.MoveTowards(currentSteer, targetSteer, Time.deltaTime * 6f);
        isBraking = kb.spaceKey != null && kb.spaceKey.isPressed;
    }

    private void ApplyPhysics()
    {
        float steerAngle = currentSteer * maxSteerAngle;
        wcFL.steerAngle = steerAngle;
        wcFR.steerAngle = steerAngle;

        float torque = isBraking ? 0f : currentThrottle * motorForce;
        wcRL.motorTorque = torque;
        wcRR.motorTorque = torque;

        float brake = isBraking ? brakeForce : 0f;
        wcFL.brakeTorque = brake;
        wcFR.brakeTorque = brake;
        wcRL.brakeTorque = brake;
        wcRR.brakeTorque = brake;
    }

    private void UpdateVisuals()
    {
        SyncWheel(wcFL, visFL);
        SyncWheel(wcFR, visFR);
        SyncWheel(wcRL, visRL);
        SyncWheel(wcRR, visRR);

        float steerAngle = currentSteer * maxSteerAngle;
        if (caliperFL != null) caliperFL.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
        if (caliperFR != null) caliperFR.localRotation = Quaternion.Euler(0f, steerAngle, 0f);

        if (steeringWheel != null)
        {
            Quaternion turn = Quaternion.AngleAxis(currentSteer * steeringWheelMaxAngle, steeringWheelAxis);
            steeringWheel.localRotation = initialSteeringRotation * turn;
        }

        if (brakeLightsRenderer != null && brakeOnMaterial != null && brakeOffMaterial != null)
        {
            brakeLightsRenderer.sharedMaterial = isBraking ? brakeOnMaterial : brakeOffMaterial;
        }
    }

    private void SyncWheel(WheelCollider col, Transform vis)
    {
        if (col == null || vis == null) return;
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        vis.position = pos;
        vis.rotation = rot;
    }

    private void PrintDiagnostics()
    {
        if (rb == null || wcFL == null || wcRL == null) return;

        float speedKmh = rb.linearVelocity.magnitude * 3.6f;

        // 1.0 = auto jedzie idealnie przodem, -1.0 = tyłem, 0.0 = ślizga się bokiem
        float moveAlignment = rb.linearVelocity.magnitude > 0.1f
            ? Vector3.Dot(rb.linearVelocity.normalized, transform.forward)
            : 0f;

        // 1.0 = WheelCollider skierowany przodem do auta, 0.0 = ustawiony w poprzek
        float colliderAlignment = Vector3.Dot(wcFL.transform.forward, transform.forward);

        string visRot = visFL != null ? visFL.localEulerAngles.ToString("F0") : "Brak";
        string meshRot = wheelMeshFL != null ? wheelMeshFL.localEulerAngles.ToString("F0") : "Brak";

        Debug.Log(
            $"[DIAGNOSTYKA AUTA] Prędkość: {speedKmh:F1} km/h | Gaz: {currentThrottle:F2} | Skręt: {wcFL.steerAngle:F1}°\n" +
            $"Obroty (RPM) -> FL: {wcFL.rpm:F0} (Ziemia: {wcFL.isGrounded}) | FR: {wcFR.rpm:F0} | RL: {wcRL.rpm:F0} (Ziemia: {wcRL.isGrounded}) | RR: {wcRR.rpm:F0}\n" +
            $"Kierunek -> Ruch auta wzg. przodu: {moveAlignment:F2} | Oś WheelCollidera: {colliderAlignment:F2} | Rotacja Vis_FL: {visRot} | Rotacja Mesh_FL: {meshRot}"
        );
    }
}