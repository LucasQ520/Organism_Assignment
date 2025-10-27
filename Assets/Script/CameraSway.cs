using UnityEngine;

public class CameraSway : MonoBehaviour
{
    Vector3 basePos;

    void Start() => basePos = transform.position;

    void Update()
    {
        transform.position = basePos + new Vector3(Mathf.Sin(Time.time * 0.2f) * 0.2f, Mathf.Cos(Time.time * 0.25f) * 0.2f, 0);
    }
}
