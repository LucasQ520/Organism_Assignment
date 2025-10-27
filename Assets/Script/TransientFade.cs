using UnityEngine;

public class TransientFade : MonoBehaviour
{
    float life = 1f;
    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        life -= Time.deltaTime;
        if (sr) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, life);
        if (life <= 0) Destroy(gameObject);
    }
}
