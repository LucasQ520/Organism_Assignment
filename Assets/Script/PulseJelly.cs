using System.Collections.Generic;
using UnityEngine;

public class PulseJelly : MonoBehaviour
{
    public static readonly List<PulseJelly> Instances = new();

    public Sprite pulseJellySprite;

    private Vector2 velocity;
    private SpriteRenderer sr;
    private float lifeSpan;
    private float age;
    private float hunger;
    private float fuseCooldown;
    private float driftTimer;

    private enum State { Pulse, Drift, Feast }
    private State state = State.Pulse;

    private Vector2 target;

    void OnEnable() => Instances.Add(this);
    void OnDisable() => Instances.Remove(this);

    void Start()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = pulseJellySprite;
        sr.sortingOrder = 3;

        transform.localScale = Vector3.one * Random.Range(0.9f, 1.3f);
        velocity = Random.insideUnitCircle * 0.5f;

        lifeSpan = Random.Range(20f, 30f);
        hunger = Random.Range(3f, 8f);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age > lifeSpan)
        {
            Destroy(gameObject);
            return;
        }

        //hunger
        hunger -= Time.deltaTime;
        if (hunger < 0 && state != State.Feast)
        {
            //goes into feast state and picks a random target
            target = Random.insideUnitCircle * 5f;
            state = State.Feast;
        }

        Vector2 current = EnvironmentController.I.CurrentAt(transform.position);

        //states
        if (state == State.Pulse)
        {
            //floating with vertical motion
            velocity += current * 0.2f;
            velocity += Vector2.up * Mathf.Sin(Time.time * 3f) * 0.03f;
        }
        else if (state == State.Feast)
        {
            //swim toward a feeding target
            Vector2 directionToTarget = (target - (Vector2)transform.position).normalized;
            velocity += directionToTarget * 0.3f;

            //reset hunger and drift
            if (Vector2.Distance(transform.position, target) < 0.4f)
            {
                hunger = Random.Range(3f, 8f);
                state = State.Drift;
                driftTimer = Random.Range(5f, 8f);
            }
        }
        else if (state == State.Drift)
        {
            //random drifting
            velocity += current * 0.1f + Random.insideUnitCircle * 0.02f;

            //rotation
            transform.Rotate(Vector3.forward * Mathf.Sin(Time.time * 0.1f) * 0.1f);

            //fade color
            sr.color = Color.Lerp(sr.color, new Color(0.6f, 0.9f, 1f, 0.8f), Time.deltaTime * 0.3f);

            //emit glowing motes
            if (Random.value < 0.02f)
            {
                GameObject mote = new GameObject("DriftMote");
                mote.AddComponent<TransientFade>();

                SpriteRenderer moteSR = mote.AddComponent<SpriteRenderer>();
                moteSR.sprite = pulseJellySprite;
                moteSR.color = new Color(0.8f, 0.95f, 1f, 0.5f);

                //place near the jelly
                mote.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.3f;
                mote.transform.localScale = Vector3.one * Random.Range(0.2f, 0.4f);
            }

            //drift timer
            driftTimer -= Time.deltaTime;
            if (driftTimer <= 0f)
            {
                state = State.Pulse;
                transform.localScale = Vector3.one;
            }
        }

        //motion and visuals
        velocity = Vector2.ClampMagnitude(velocity, 1.5f);
        transform.position += (Vector3)velocity * Time.deltaTime;

        //color
        if (state == State.Feast)
        {
            //pink tone when feeding
            sr.color = new Color(1f, 0.6f, 0.8f, 0.9f);
        }
        else
        {
            //default glowing cyan
            sr.color = new Color(
                0.6f + Mathf.Sin(Time.time * 3f) * 0.2f,
                0.9f,
                1f,
                0.85f
            );
        }

        Wrap();

        //fusing with other jelly
        if (fuseCooldown > 0)
        {
            fuseCooldown -= Time.deltaTime;
        }
        else
        {
            TryFuse();
        }
    }

    //fusion
    void TryFuse()
    {
        foreach (var other in Instances)
        {
            if (other == this) continue;

            //check for distance to another jelly
            if (Vector2.Distance(transform.position, other.transform.position) < 0.6f)
            {
                //grow
                transform.localScale *= 1.15f;
                fuseCooldown = 6f;

                //split after 3 seconds
                Invoke(nameof(SplitAfterFusion), 3f);
                break;
            }
        }
    }

    void SplitAfterFusion()
    {
        //cap
        if (Instances.Count >= 10)
            return;

        //create new jelly nearby
        GameObject cloneObj = new GameObject("PulseJelly");
        PulseJelly clone = cloneObj.AddComponent<PulseJelly>();
        clone.pulseJellySprite = pulseJellySprite;

        clone.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.6f;

        //shrink after splitting
        transform.localScale *= 0.9f;
    }

    //wrap around the screen
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
