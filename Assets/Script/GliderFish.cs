using System.Collections.Generic;
using UnityEngine;

public class GliderFish : MonoBehaviour
{
    public static readonly List<GliderFish> Instances = new();

    public Sprite gliderFishSprite;

    private enum State { Forage, School, Flee }
    private State state = State.Forage;

    private Vector2 velocity;
    private SpriteRenderer sr;
    private float lifeSpan;
    private float age;
    private float fearMemory;
    private float sparkTimer;

    void OnEnable() => Instances.Add(this);
    void OnDisable() => Instances.Remove(this);

    void Start()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = gliderFishSprite;
        sr.sortingOrder = 2;

        transform.localScale = Vector3.one * Random.Range(0.7f, 1.0f);
        velocity = Random.insideUnitCircle.normalized * Random.Range(1f, 2f);

        lifeSpan = Random.Range(20f, 30f);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age > lifeSpan)
        {
            Destroy(gameObject);
            return;
        }

        bool predatorNearby = NearJelly(2.0f);
        int nearbyFishCount = NeighborCount(2.0f);

        if (predatorNearby)
        {
            //increase fear
            fearMemory = Mathf.MoveTowards(fearMemory, 1f, Time.deltaTime * 1.2f);
        }
        else
        {
            //calm down
            fearMemory = Mathf.MoveTowards(fearMemory, 0f, Time.deltaTime * 0.5f);
        }

        if (fearMemory > 0.6f)
        {
            state = State.Flee; //panic
        }
        else if (nearbyFishCount >= 3)
        {
            state = State.School; //group
        }
        else
        {
            state = State.Forage; //explore individually
        }

        Vector2 acceleration = Vector2.zero;

        if (state == State.Forage)
        {
            //swim with current and randomness
            Vector2 current = EnvironmentController.I.CurrentAt(transform.position);
            acceleration += Random.insideUnitCircle * 0.3f + current * 0.5f;
        }
        else if (state == State.School)
        {
            acceleration += Cohesion(2.8f) * 0.6f;
            acceleration += Alignment(2.5f) * 0.5f;
            acceleration += Separation(1.2f) * 0.9f;

            //create spark motes
            if (sparkTimer <= 0f)
            {
                Spark();
                sparkTimer = Random.Range(0.3f, 0.7f);
            }
        }
        else if (state == State.Flee)
        {
            //run away from nearest jelly
            acceleration += AwayFromJelly() * 3f;
        }

        sparkTimer -= Time.deltaTime;

        float maxSpeed;
        float accelerationMultiplier;

        if (state == State.Flee)
        {
            //faster when scared
            maxSpeed = 5f;
            accelerationMultiplier = 5f;
        }
        else
        {
            // gentle motion
            maxSpeed = 2f;
            accelerationMultiplier = 2.5f;
        }

        //movement and acceleration
        velocity += acceleration * Time.deltaTime * accelerationMultiplier;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        transform.position += (Vector3)velocity * Time.deltaTime;

        //rotatation
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        //color by state
        if (state == State.Flee)
        {
            sr.color = new Color(1f, 0.4f, 0.4f); //red panic
        }
        else if (state == State.School)
        {
            sr.color = new Color(0.5f, 1f, 1f);   //cyan group
        }
        else
        {
            sr.color = new Color(0.7f, 1f, 0.9f); //teal calm
        }

        Wrap();
    }

    //spark effect
    void Spark()
    {
        GameObject spark = new GameObject("FishSpark");
        SpriteRenderer sparkSR = spark.AddComponent<SpriteRenderer>();

        sparkSR.sprite = MakeDot(32, new Color(0.8f, 1f, 1f, 0.9f));
        sparkSR.sortingOrder = 5;

        spark.transform.position = transform.position;
        spark.transform.localScale = Vector3.one * 0.3f;
        spark.AddComponent<TransientFade>();
    }

    //create a small circular glowing dot sprite
    Sprite MakeDot(int size, Color color)
    {
        Texture2D tex = new(size, size);
        Vector2 center = new(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new(x, y), center) / radius;
                float alpha = Mathf.Clamp01(1f - dist * dist);
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
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


    //check if any jelly is nearby
    bool NearJelly(float radius)
    {
        foreach (var jelly in PulseJelly.Instances)
        {
            if (Vector2.Distance(jelly.transform.position, transform.position) < radius)
                return true;
        }
        return false;
    }

    //direction away from nearest jelly
    Vector2 AwayFromJelly()
    {
        PulseJelly nearest = null;
        float closestDistance = float.MaxValue;

        foreach (var jelly in PulseJelly.Instances)
        {
            float d = Vector2.Distance(jelly.transform.position, transform.position);
            if (d < closestDistance)
            {
                closestDistance = d;
                nearest = jelly;
            }
        }

        if (nearest == null)
            return Vector2.zero;

        return ((Vector2)transform.position - (Vector2)nearest.transform.position).normalized;
    }

    //count nearby fish
    int NeighborCount(float radius)
    {
        int count = 0;
        foreach (var fish in Instances)
        {
            if (fish != this && Vector2.Distance(fish.transform.position, transform.position) < radius)
                count++;
        }
        return count;
    }

    //move toward the center of nearby fish
    Vector2 Cohesion(float radius)
    {
        Vector2 sum = Vector2.zero;
        int count = 0;

        foreach (var fish in Instances)
        {
            if (fish != this && Vector2.Distance(fish.transform.position, transform.position) < radius)
            {
                sum += (Vector2)fish.transform.position;
                count++;
            }
        }

        if (count == 0)
            return Vector2.zero;

        Vector2 average = sum / count;
        return (average - (Vector2)transform.position).normalized;
    }

    //align with nearby direction
    Vector2 Alignment(float radius)
    {
        Vector2 sum = Vector2.zero;
        int count = 0;

        foreach (var fish in Instances)
        {
            if (fish != this && Vector2.Distance(fish.transform.position, transform.position) < radius)
            {
                sum += fish.velocity;
                count++;
            }
        }

        if (count == 0)
            return Vector2.zero;

        return (sum / count).normalized;
    }

    //go away when too close
    Vector2 Separation(float radius)
    {
        Vector2 force = Vector2.zero;

        foreach (var fish in Instances)
        {
            if (fish == this) continue;

            Vector2 diff = (Vector2)transform.position - (Vector2)fish.transform.position;
            float distance = diff.magnitude;

            if (distance < radius && distance > 0)
            {
                //closer, the stronger the repulsion
                force += diff / distance;
            }
        }

        return force.normalized;
    }
}
