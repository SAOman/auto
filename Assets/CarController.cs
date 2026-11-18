using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider wcFL, wcFR, wcRL, wcRR;

    [Header("Wheel Meshes (TiaraGT83_WheelTuner_*)")]
    public Transform meshFL, meshFR, meshRL, meshRR;

    [Header("Silnik / skręt")]
    public float motorTorque = 1800f;
    public float maxSteerAngle = 30f;
    public float maxSpeedKmh = 180f;
    public float steerSpeedReduction = 0.5f;

    [Header("Hamowanie")]
    public float brakeTorque = 3500f;
    public float handbrakeTorque = 6000f;

    [Header("Światła stop (opcjonalnie)")]
    public Renderer brakelightsRenderer;
    public Color brakeOnColor = Color.red * 3f;

    Rigidbody rb;
    float speedKmh;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.4f, 0f);
    }

    void FixedUpdate()
    {
        float throttle = Input.GetAxis("Vertical");
        float steer = Input.GetAxis("Horizontal");
        bool handbrake = Input.GetKey(KeyCode.Space);

        Vector3 vel = rb.linearVelocity;
        speedKmh = vel.magnitude * 3.6f;
        float forwardSpeed = Vector3.Dot(vel, transform.forward);

        float motor = 0f;
        float brake = 0f;

        if (throttle > 0.01f)
        {
            if (forwardSpeed < -0.5f) brake = brakeTorque;
            else if (speedKmh < maxSpeedKmh) motor = throttle * motorTorque;
        }
        else if (throttle < -0.01f)
        {
            if (forwardSpeed > 0.5f) brake = brakeTorque;
            else if (speedKmh < 50f) motor = throttle * motorTorque * 0.5f;
        }
        else
        {
            brake = 150f;
        }

        wcRL.motorTorque = motor;
        wcRR.motorTorque = motor;

        wcFL.brakeTorque = brake;
        wcFR.brakeTorque = brake;
        wcRL.brakeTorque = handbrake ? handbrakeTorque : brake;
        wcRR.brakeTorque = handbrake ? handbrakeTorque : brake;

        float steerFactor = Mathf.Lerp(1f, steerSpeedReduction, speedKmh / maxSpeedKmh);
        float angle = steer * maxSteerAngle * steerFactor;
        wcFL.steerAngle = angle;
        wcFR.steerAngle = angle;

        UpdateBrakeLights(brake > 0f || handbrake);
        UpdateWheelVisual(wcFL, meshFL);
        UpdateWheelVisual(wcFR, meshFR);
        UpdateWheelVisual(wcRL, meshRL);
        UpdateWheelVisual(wcRR, meshRR);
    }

    void UpdateWheelVisual(WheelCollider wc, Transform mesh)
    {
        if (mesh == null) return;
        wc.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.SetPositionAndRotation(pos, rot);
    }

    void UpdateBrakeLights(bool on)
    {
        if (brakelightsRenderer == null) return;
        var mat = brakelightsRenderer.material;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", on ? brakeOnColor : Color.black);
    }
}