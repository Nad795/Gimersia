using UnityEngine;
using UnityEngine.Tilemaps;

public class Breakable : MonoBehaviour
{
    [Header("Normal Tiles")]
    public TileBase normalLeft;
    public TileBase normalMid;
    public TileBase normalRight;

    [Header("Cracked Tiles")]
    public TileBase crackedLeft;
    public TileBase crackedMid;
    public TileBase crackedRight;

    [Header("Broken Tiles")]
    public TileBase brokenLeft;
    public TileBase brokenMid;
    public TileBase brokenRight;

    [Header("Timing")]
    public float crackTime = 1f;
    public float breakTime = 2f;

    private Tilemap tilemap;
    private Transform player;
    private Vector3Int currentPlatformOrigin;
    private float standTime;
    private bool playerOnPlatform;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
            Debug.LogWarning("Player with tag 'Player' not found!");
    }

    void Update()
    {
        if (player == null) return;

        Vector3 playerPos = player.position + Vector3.down * 0.1f;
        Vector3Int tilePos = tilemap.WorldToCell(playerPos);

        if (tilemap.HasTile(tilePos))
        {
            Vector3Int platformOrigin = new Vector3Int(tilePos.x - (tilePos.x % 3), tilePos.y, 0);

            if (!playerOnPlatform || platformOrigin != currentPlatformOrigin)
            {
                currentPlatformOrigin = platformOrigin;
                standTime = 0f;
                playerOnPlatform = true;
            }

            standTime += Time.deltaTime;

            if (standTime >= crackTime && standTime < breakTime)
            {
                SetPlatformTiles(currentPlatformOrigin, crackedLeft, crackedMid, crackedRight);
            }
            else if (standTime >= breakTime)
            {
                SetPlatformTiles(currentPlatformOrigin, brokenLeft, brokenMid, brokenRight);
                Invoke(nameof(DestroyPlatform), 0.3f);
            }
        }
        else
        {
            playerOnPlatform = false;
            standTime = 0f;
        }
    }

    void SetPlatformTiles(Vector3Int origin, TileBase left, TileBase mid, TileBase right)
    {
        Vector3Int leftPos = origin;
        Vector3Int midPos = origin + Vector3Int.right;
        Vector3Int rightPos = origin + Vector3Int.right * 2;

        if (tilemap.HasTile(leftPos)) tilemap.SetTile(leftPos, left);
        if (tilemap.HasTile(midPos))  tilemap.SetTile(midPos, mid);
        if (tilemap.HasTile(rightPos)) tilemap.SetTile(rightPos, right);
    }

    void DestroyPlatform()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3Int pos = new Vector3Int(currentPlatformOrigin.x + i, currentPlatformOrigin.y, 0);
            if (tilemap.HasTile(pos))
                tilemap.SetTile(pos, null);
        }
    }
}
