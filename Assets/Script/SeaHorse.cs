using System.Collections.Generic;
using UnityEngine;

public class Seahorse : MonoBehaviour
{
    public static readonly List<Seahorse> Instances = new();
    public Sprite seahorseSprite;

    private enum State { Drift, Anchor, Explore }
    private State state = State.Drift;

    private Vector2 velocity;
    private SpriteRenderer sr;
    private float lifeSpan;
    private float age;
    private float timer;

    void OnEnable() => Instances.Add(this);
    void OnDisable() => Instances.Remove(this);

    void Start()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = seahorseSprite;
        sr.sortingOrder = 4;

        transform.localScale = Vector3.one * Random.Range(0.9f, 1.2f);
        velocity = Random.insideUnitCircle * 0.3f;
        lifeSpan = Random.Range(60f, 100f);
        timer = Random.Range(3f, 6f);
    }

    void Update()
    {
        //destroy when lifespan ends
        age += Time.deltaTime;
        if (age > lifeSpan)
        {
            Destroy(gameObject);
            return;
        }

        timer -= Time.deltaTime;

        //detect nearby jellyfish
        bool jellyNearby = false;
        foreach (var jelly in PulseJelly.Instances)
        {
            if (Vector2.Distance(jelly.transform.position, transform.position) < 2.5f)
            {
                jellyNearby = true;
                break;
            }
        }

        //mood affects color
        float mood = Mathf.PerlinNoise(Time.time * 0.2f, transform.position.x * 0.1f);

        //change color when close to jelly
        if (jellyNearby)
        {
            //pink when near jellyfish
            sr.color = Color.Lerp(new Color(0.9f, 0.8f, 0.6f), new Color(1f, 0.6f, 0.9f), 0.7f);
        }
        else
        {
            sr.color = Color.Lerp(new Color(0.9f, 0.8f, 0.6f), new Color(1f, 0.6f, 0.9f), mood * 0.4f);
        }

        //sstate transitions
        if (state == State.Drift && (timer <= 0f || jellyNearby))
        {
            state = State.Anchor;
            timer = Random.Range(3f, 6f);
        }
        else if (state == State.Anchor && timer <= 0f && !jellyNearby)
        {
            state = State.Explore;
            timer = Random.Range(3f, 6f);
        }
        else if (state == State.Explore && timer <= 0f)
        {
            state = State.Drift;
            timer = Random.Range(3f, 6f);
        }

        if (state == State.Drift)
        {
            //float with currents
            Vector2 current = EnvironmentController.I.CurrentAt(transform.position);
            velocity = Vector2.Lerp(velocity, current * 0.6f, Time.deltaTime * 0.8f);
        }
        else if (state == State.Anchor)
        {
            //stay mostly still
            velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 1.5f);

            //spawn green glowing kelp motes
            if (Random.value < 0.01f)
            {
                GameObject kelpSpark = new GameObject("KelpSpark");
                SpriteRenderer sparkRenderer = kelpSpark.AddComponent<SpriteRenderer>();
                sparkRenderer.sprite = MakeDot(32, new Color(0.6f, 1f, 0.7f, 0.9f));
                sparkRenderer.sortingOrder = 3;

                kelpSpark.transform.position = transform.position + Vector3.down * 0.3f;
                kelpSpark.transform.localScale = Vector3.one * 0.5f;
                kelpSpark.AddComponent<TransientFade>();
            }
        }
        else if (state == State.Explore)
        {
            velocity = Vector2.Lerp(velocity, Random.insideUnitCircle * 1.2f, Time.deltaTime);
        }

        //movement and slight rotation sway
        transform.position += (Vector3)velocity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2f) * 10f);

        Wrap();
    }

    //create a small circular glowing dot sprite
    Sprite MakeDot(int size, Color color)
    {
        Texture2D texture = new(size, size);
        Vector2 center = new(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new(x, y), center) / radius;
                float alpha = Mathf.Clamp01(1f - distance * distance);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
    }

    //to wrap around the screen
    void Wrap()
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);

        if (vp.x < 0) vp.x = 1;
        else if (vp.x > 1) vp.x = 0;

        if (vp.y < 0) vp.y = 1;
        else if (vp.y > 1) vp.y = 0;

        transform.position = Camera.main.ViewportToWorldPoint(vp);
    }
}
