using UnityEngine;
using System.Collections;   

public class MeteorSpawner : MonoBehaviour
{
    public GameObject meteorPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float xMin = -7f;
    public float xMax = 7f;
    public float yMax = 60f;
    public int maxMeteorCount = 10;

    private bool spawning = true;

    void Start()
    {
        StartCoroutine(SpawnLoop());
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
        Vector2 spawnPosition = new Vector2(randomX, yMax);

        Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
    }

    public void StopSpawning()
    {
        spawning = false;
    }
}
