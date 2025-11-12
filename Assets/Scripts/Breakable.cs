using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Breakable : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap breakableTilemap;   // Breakable platform (visual to change)
    [SerializeField] private Tilemap sampleTilemap;      // Normal platform (where player stands)

    [Header("Mapping")]
    [Tooltip("If Breakable is shifted relative to Platform, add a cell-space offset here.")]
    [SerializeField] private Vector3Int breakableCellOffset = Vector3Int.zero;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector2 feetOffset = new Vector2(0f, -0.8f); // from player.position

    [Header("Grouping")]
    [SerializeField] private int groupWidth = 3;

    [Header("Timing")]
    public float crackTime = 1f;
    public float breakTime = 2f;

    [Header("Tiles")]
    public TileBase normalLeft, normalMid, normalRight;
    public TileBase crackedLeft, crackedMid, crackedRight;
    public TileBase brokenLeft,  brokenMid,  brokenRight;

    [Header("Debug")]
    public bool drawGizmos = true;

    private Vector3Int currentGroup;
    private float standTime;
    private bool onGroup;

    void Reset()
    {
        breakableTilemap = GetComponent<Tilemap>();
    }

    void Awake()
    {
        if (!breakableTilemap) breakableTilemap = GetComponent<Tilemap>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!sampleTilemap) sampleTilemap = breakableTilemap; // fallback
    }

    void Update()
    {
        if (!player || !sampleTilemap || !breakableTilemap) return;

        // 1) Feet world position
        Vector3 feetWorld = (Vector2)player.position + feetOffset;

        // 2) Cell under feet on the SAMPLE map
        Vector3Int sampleCell = sampleTilemap.WorldToCell(feetWorld);
        if (!sampleTilemap.HasTile(sampleCell))
        {
            onGroup = false;
            standTime = 0f;
            return;
        }

        // 3) Map to BREAKABLE cell
        Vector3Int bCell = sampleCell + breakableCellOffset;
        if (!breakableTilemap.HasTile(bCell))
        {
            onGroup = false;
            standTime = 0f;
            return;
        }

        // 4) Find the LEFTMOST contiguous cell for this chunk (robust against seams)
        Vector3Int left = bCell;
        int stepsLeft = 0;
        while (stepsLeft < groupWidth - 1 &&
               breakableTilemap.HasTile(left + Vector3Int.left))
        {
            left += Vector3Int.left;
            stepsLeft++;
        }
        Vector3Int groupOrigin = new Vector3Int(left.x, bCell.y, 0);

        if (!onGroup || groupOrigin != currentGroup)
        {
            currentGroup = groupOrigin;
            standTime = 0f;
            onGroup = true;
        }

        standTime += Time.deltaTime;

        if (standTime >= crackTime && standTime < breakTime)
        {
            SetGroupTiles(currentGroup, crackedLeft, crackedMid, crackedRight);
        }
        else if (standTime >= breakTime)
        {
            SetGroupTiles(currentGroup, brokenLeft, brokenMid, brokenRight);
            Vector3Int captured = currentGroup;
            StartCoroutine(DestroyGroupAfterDelay(captured, 0.25f));
            standTime = -9999f; // prevent requeue while still on it
        }
    }

    private void SetGroupTiles(Vector3Int origin, TileBase left, TileBase mid, TileBase right)
    {
        for (int i = 0; i < groupWidth; i++)
        {
            var pos = new Vector3Int(origin.x + i, origin.y, 0);
            var tile = (i == 0) ? left : (i == groupWidth - 1) ? right : mid;

            if (breakableTilemap.HasTile(pos))
                breakableTilemap.SetTile(pos, tile);
        }
    }

    private IEnumerator DestroyGroupAfterDelay(Vector3Int origin, float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < groupWidth; i++)
        {
            var pos = new Vector3Int(origin.x + i, origin.y, 0);
            if (breakableTilemap.HasTile(pos))
                breakableTilemap.SetTile(pos, null);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !player) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)player.position + feetOffset, 0.05f);
    }
}
