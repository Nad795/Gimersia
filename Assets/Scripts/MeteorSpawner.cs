using UnityEngine;
using System.Collections;   

public class MeteorSpawner : MonoBehaviour
{
    public GameObject meteorPrefab;
    public GameObject warningPrefab;
    public float warningDuration = 1f;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float xMin = -7f;
    public float xMax = 7f;
    public float yOffsetAbovePlayer = 14f;   // seberapa jauh di atas player meteornya muncul

    public int maxMeteorCount = 10;

    [Header("Player Ref")]
    [SerializeField] private Transform player;

    private bool spawning = true;   // tetap dipakai untuk stop permanen (misal saat game over)
    private bool paused = false;    // dipakai untuk pause sementara (misal saat level intro)

    // simpan ketinggian tertinggi yang pernah dicapai player
    private float highestPlayerY;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
            highestPlayerY = player.position.y;

        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (player == null) return;

        // update ketinggian tertinggi
        if (player.position.y > highestPlayerY)
            highestPlayerY = player.position.y;
    }

    private IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            // Kalau sedang pause, skip spawn tapi tetap loop
            if (!paused && GameObject.FindGameObjectsWithTag("Meteor").Length < maxMeteorCount)
            {
                SpawnMeteor();
            }

            float delay = Random.Range(0.5f, spawnInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnMeteor()
    {
        float randomX = Random.Range(xMin, xMax);

        float warningPosY = Camera.main.transform.position.y + Camera.main.orthographicSize - 1f;
        Vector2 warningPosition = new Vector2(randomX, warningPosY);

        GameObject warning = Instantiate(warningPrefab, warningPosition, Quaternion.identity);

        // meteor spawn di atas ketinggian tertinggi yang pernah dicapai player
        float meteorSpawnY = highestPlayerY + yOffsetAbovePlayer;

        StartCoroutine(SpawnMeteorAfterWarning(randomX, warning, meteorSpawnY));
    }

    public void StopSpawning()
    {
        // stop permanen, dipakai misal saat game over atau menang
        spawning = false;
    }

    public void PauseSpawning()
    {
        // pause sementara (dipakai LevelIntroController)
        paused = true;
    }

    public void ResumeSpawning()
    {
        paused = false;
    }

    private IEnumerator SpawnMeteorAfterWarning(float xPos, GameObject warning, float meteorY)
    {
        yield return new WaitForSeconds(warningDuration);

        if (!paused && spawning)  // jangan spawn kalau sudah di-stop/permanen atau sedang pause
        {
            Vector2 meteorPosition = new Vector2(xPos, meteorY);
            Instantiate(meteorPrefab, meteorPosition, Quaternion.identity);
        }

        Destroy(warning);
    }
}
