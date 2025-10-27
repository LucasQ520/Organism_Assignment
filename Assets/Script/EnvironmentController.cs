using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    public static EnvironmentController I;
    [Header("Current Settings")]
    public float baseStrength = 0.6f;
    public float tidePeriod = 180f;
    public float noiseScale = 0.07f;

    float t0;

    void Awake()
    {
        if (I == null) I = this;
        else Destroy(gameObject);
        t0 = Time.time;
    }

    public Vector2 CurrentAt(Vector2 worldPos)
    {
        float t = Time.time * 0.1f;
        float nx = Mathf.PerlinNoise(worldPos.y * noiseScale + t, 0.1234f) - 0.5f;
        float ny = Mathf.PerlinNoise(worldPos.x * noiseScale, t + 0.5678f) - 0.5f;
        Vector2 v = new Vector2(nx, ny).normalized * baseStrength;
        return v;
    }
}
