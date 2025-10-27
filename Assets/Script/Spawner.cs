using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Population Targets")]
    public int fishTarget = 15;
    public int jellyTarget = 5;
    public int seahorseTarget = 8;
    public int kelpTarget = 10;

    [Header("Sprites")]
    public Sprite gliderFishSprite;
    public Sprite pulseJellySprite;
    public Sprite seahorseSprite;
    public Sprite kelpFrondSprite;

    [Header("Kelp Placement")]
    public Vector2 kelpXRange = new Vector2(-6f, 6f);
    public float kelpY = -3.8f;

    void Start()
    {
        new GameObject("Environment").AddComponent<EnvironmentController>();

        for (int i = 0; i < kelpTarget; i++)
        {
            float x = Mathf.Lerp(kelpXRange.x, kelpXRange.y, i / (float)(kelpTarget - 1)) + Random.Range(-0.3f, 0.3f);
            float y = kelpY + Random.Range(-0.2f, 0.2f);
            var k = new GameObject("KelpFrond").AddComponent<KelpFrond>();
            k.kelpFrondSprite = kelpFrondSprite;
            k.transform.position = new Vector3(x, y, 0);
        }
    }

    void Update()
    {
        while (GliderFish.Instances.Count < fishTarget)
        {
            var go = new GameObject("GliderFish");
            var fish = go.AddComponent<GliderFish>();
            fish.gliderFishSprite = gliderFishSprite;
            go.transform.position = Random.insideUnitCircle * 4f;
        }

        while (PulseJelly.Instances.Count < jellyTarget)
        {
            var go = new GameObject("PulseJelly");
            var jelly = go.AddComponent<PulseJelly>();
            jelly.pulseJellySprite = pulseJellySprite;
            go.transform.position = Random.insideUnitCircle * 4f;
        }

        while (Seahorse.Instances.Count < seahorseTarget)
        {
            var go = new GameObject("Seahorse");
            var sh = go.AddComponent<Seahorse>();
            sh.seahorseSprite = seahorseSprite;
            go.transform.position = Random.insideUnitCircle * 4f;
        }
    }
}
