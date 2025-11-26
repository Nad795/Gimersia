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
    public float yMax;

    public int maxMeteorCount = 10;

    private bool spawning = true;

    void Start()
    {
        StartCoroutine(SpawnLoop());
        yMax = Camera.main.transform.position.y + Camera.main.orthographicSize + 5f;
    }

    private IEnumerator SpawnLoop()
    {
        while (spawning)
        {
                if (GameObject.FindGameObjectsWithTag("Meteor").Length < maxMeteorCount)
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

        float warningPos = Camera.main.transform.position.y + Camera.main.orthographicSize - 1f;
        Vector2 warningPosition = new Vector2(randomX, warningPos);

        GameObject warning = Instantiate(warningPrefab, warningPosition, Quaternion.identity);
        StartCoroutine(SpawnMeteorAfterWarning(randomX, warning, warningPos));
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    private IEnumerator SpawnMeteorAfterWarning(float xPos, GameObject warning, float warningY)
    {
        yield return new WaitForSeconds(warningDuration);

        Vector2 meteorPosition = new Vector2(xPos, yMax);
        Instantiate(meteorPrefab, meteorPosition, Quaternion.identity);

        Destroy(warning);
    }

}
