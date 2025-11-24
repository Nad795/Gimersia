using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Breakable : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap breakableTilemap;
    [SerializeField] private Tilemap sampleTilemap;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector2 feetOffset = new Vector2(0f, -0.8f);

    [Header("Timing")]
    public float crackTime = 1f;
    public float disappearTime = 2f;
    public float respawnTime = 3f;

    [Header("Tiles")]
    public TileBase normalTile;
    public TileBase crackedTile;

    [Header("Mapping Offset (optional)")]
    public Vector3Int breakableCellOffset = Vector3Int.zero;

    private Vector3Int currentCell;
    private float standTime;
    private bool onTile;
    private bool isBreaking = false;

    void Awake()
    {
        if (!breakableTilemap) breakableTilemap = GetComponent<Tilemap>();
        if (!sampleTilemap) sampleTilemap = breakableTilemap;

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (!player || !sampleTilemap || !breakableTilemap) return;

        Vector3 feetWorld = (Vector2)player.position + feetOffset;

        Vector3Int sampleCell = sampleTilemap.WorldToCell(feetWorld);

        if (!sampleTilemap.HasTile(sampleCell))
        {
            ResetState();
            return;
        }

        Vector3Int targetCell = sampleCell + breakableCellOffset;

        if (!breakableTilemap.HasTile(targetCell))
        {
            ResetState();
            return;
        }

        if (!onTile || targetCell != currentCell)
        {
            currentCell = targetCell;
            standTime = 0f;
            onTile = true;
            isBreaking = false;
        }

        standTime += Time.deltaTime;

        if (standTime >= crackTime && standTime < disappearTime)
        {
            breakableTilemap.SetTile(currentCell, crackedTile);
        }
        else if (standTime >= disappearTime && !isBreaking)
        {
            StartCoroutine(BreakAndRespawn(currentCell));
            isBreaking = true;
        }
    }

    private IEnumerator BreakAndRespawn(Vector3Int cell)
    {
        breakableTilemap.SetTile(cell, null);

        yield return new WaitForSeconds(respawnTime);

        breakableTilemap.SetTile(cell, normalTile);
    }

    void ResetState()
    {
        onTile = false;
        standTime = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (player) Gizmos.DrawWireSphere((Vector2)player.position + feetOffset, 0.05f);
    }
}
