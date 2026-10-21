using System.Collections.Generic;
using UnityEngine;

// Wrzuć na każdą kulę (sphere). Rigidbody doda się sam.
// Kula wisi w miejscu, dopóki gracz nie wejdzie na obiekt z tagiem "trap".
[RequireComponent(typeof(Rigidbody))]
public class TrapSphere : MonoBehaviour
{
    private static readonly List<TrapSphere> all = new List<TrapSphere>();

    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb.isKinematic = true;   // wisi w powietrzu
    }

    void OnEnable()  { all.Add(this); }
    void OnDisable() { all.Remove(this); }

    private void Drop()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.WakeUp();
    }

    private void Rearm()
    {
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = true;
        transform.SetPositionAndRotation(startPosition, startRotation);
    }

    public static void DropAll()
    {
        foreach (TrapSphere s in all) s.Drop();
    }

    public static void RearmAll()
    {
        foreach (TrapSphere s in all) s.Rearm();
    }
}
