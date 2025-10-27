using UnityEngine;

public class KelpFrond : MonoBehaviour
{
    public Sprite kelpFrondSprite;
    SpriteRenderer sr;
    float swayOffset;

    void Start()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = kelpFrondSprite;
        sr.sortingOrder = 0;
        transform.localScale = Vector3.one * Random.Range(0.4f, 1.4f);
        swayOffset = Random.value * 30f;
    }

    void Update()
    {
        float sway = Mathf.Sin(Time.time * 0.6f + swayOffset) * 5f;
        transform.rotation = Quaternion.Euler(0, 0, sway);
    }
}
