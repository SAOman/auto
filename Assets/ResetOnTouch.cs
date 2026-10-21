using UnityEngine;

// Wrzuć na First Person Controller (Add Component).
public class ResetOnTouch : MonoBehaviour
{
    // Na sztywno w kodzie, żeby stara wartość z Inspectora niczego nie psuła.
    private const string ResetTag = "Plane";  // powrót na start i reset zegara
    private const string EndTag = "end";      // stop zegara (meta)
    private const string TrapTag = "trap";    // uruchamia spadanie kul

    private Vector3 startPosition;
    private Rigidbody rb;
    private GameTimer timer;

    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        timer = GetComponent<GameTimer>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Porównanie przez .tag nie rzuca błędu, nawet gdy dany tag nie istnieje w projekcie.
        string tag = collision.gameObject.tag;

        if (tag == ResetTag)
        {
            transform.position = startPosition;
            rb.linearVelocity = Vector3.zero;
            if (timer != null) timer.ResetTimer();
            TrapSphere.RearmAll();   // kule wracają na miejsce, pułapka znów gotowa
        }
        else if (tag == EndTag)
        {
            if (timer != null) timer.StopTimer();
        }
        else if (tag == TrapTag)
        {
            TrapSphere.DropAll();
        }
    }
}