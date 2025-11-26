using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Breakable : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip crumbleSfx;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap breakableTilemap;
    [SerializeField] private Tilemap sampleTilemap;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector2 feetOffset = new Vector2(0f, -1.0f);

    [Tooltip("Offset tambahan ke kiri & kanan (world space) untuk meng-handle kasus berdiri di pojok.")]
    [SerializeField] private float extraFeetCheckOffsetX = 0.65f;

    [Header("Timing")]
    [Tooltip("Waktu dari normal -> retak (player berdiri di atas).")]
    public float timeToCrack = 0.25f;          // cepat ke retak
    [Tooltip("Waktu dari retak -> hancur. SESUAIKAN dengan durasi animasi FallingStones.")]
    public float timeFromCrackToBreak = 0.75f; // agak lama
    [Tooltip("Waktu setelah hancur sebelum tile BOLEH muncul lagi.")]
    public float respawnTime = 3f;

    [Header("Tiles")]
    public TileBase normalTile;
    public TileBase crackedTile;

    [Header("Mapping Offset (optional)")]
    public Vector3Int breakableCellOffset = Vector3Int.zero;

    [Header("Falling Stones (Debris)")]
    [SerializeField] private GameObject debrisPrefab;
    [SerializeField] private int debrisCount = 1;
    [SerializeField] private Vector2 debrisOffsetRange = new Vector2(0.4f, 0.2f);

    [Header("Debug")]
    public bool drawGizmos = true;

    // ===== Per-tile state =====
    private enum BreakState { Normal, Cracked, Breaking }

    private class TileState
    {
        public BreakState state = BreakState.Normal;
        public float contactTime;     // waktu diinjak saat Normal
        public float crackedElapsed;  // waktu berlalu saat Cracked
        public bool debrisSpawned;
    }

    private readonly Dictionary<Vector3Int, TileState> tileStates =
        new Dictionary<Vector3Int, TileState>();

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

        float dt = Time.deltaTime;

        // ===== 1. Kumpulkan beberapa titik cek di bawah kaki (tengah, kiri sedikit, kanan sedikit) =====
        Vector3 feetCenter = (Vector2)player.position + feetOffset;

        Vector3[] probePoints =
        {
            feetCenter,                                              // tengah
            feetCenter + new Vector3(-extraFeetCheckOffsetX, 0f, 0f),// sedikit ke kiri
            feetCenter + new Vector3( extraFeetCheckOffsetX, 0f, 0f) // sedikit ke kanan
        };

        bool foundPlatform = false;
        Vector3Int usedSampleCell = Vector3Int.zero;

        // Untuk setiap titik probe:
        foreach (Vector3 probe in probePoints)
        {
            // Cell utama di bawah titik probe
            Vector3Int baseCell = sampleTilemap.WorldToCell(probe);

            // Cek cell ini
            if (sampleTilemap.HasTile(baseCell))
            {
                usedSampleCell = baseCell;
                foundPlatform = true;
                break;
            }

            // Kalau tidak ada tile, cek tetangga kiri/kanan cell ini (dx = -1, +1)
            for (int dx = -1; dx <= 1 && !foundPlatform; dx++)
            {
                if (dx == 0) continue;
                Vector3Int neighbor = baseCell + new Vector3Int(dx, 0, 0);
                if (sampleTilemap.HasTile(neighbor))
                {
                    usedSampleCell = neighbor;
                    foundPlatform = true;
                    break;
                }
            }

            if (foundPlatform)
                break;
        }

        // Tidak menginjak platform apa pun, semua tile “pause”
        if (!foundPlatform)
            return;

        // ===== 2. Map ke BREAKABLE tilemap =====
        Vector3Int targetCell = usedSampleCell + breakableCellOffset;
        if (!breakableTilemap.HasTile(targetCell))
            return;

        // Ambil / buat state untuk cell ini
        if (!tileStates.TryGetValue(targetCell, out TileState ts))
        {
            ts = new TileState();
            tileStates[targetCell] = ts;
            breakableTilemap.SetTile(targetCell, normalTile);
        }

        // ===== 3. Update state hanya untuk tile yang sedang diinjak =====
        switch (ts.state)
        {
            case BreakState.Normal:
                HandleNormalState(targetCell, ts, dt);
                break;

            case BreakState.Cracked:
                HandleCrackedState(targetCell, ts, dt);
                break;

            case BreakState.Breaking:
                break;
        }
    }

    // ----- State: Normal -----
    private void HandleNormalState(Vector3Int cell, TileState ts, float dt)
    {
        ts.contactTime += dt;

        if (ts.contactTime >= timeToCrack)
        {
            ts.state = BreakState.Cracked;
            ts.crackedElapsed = 0f;

            breakableTilemap.SetTile(cell, crackedTile);

            if (!ts.debrisSpawned)
            {
                SpawnDebris(cell);
                ts.debrisSpawned = true;

                if (sfxSource != null && crumbleSfx != null)
                    sfxSource.PlayOneShot(crumbleSfx);
            }
        }
    }

    // ----- State: Cracked -----
    private void HandleCrackedState(Vector3Int cell, TileState ts, float dt)
    {
        ts.crackedElapsed += dt;

        if (ts.crackedElapsed >= timeFromCrackToBreak)
        {
            ts.state = BreakState.Breaking;
            StartCoroutine(BreakAndRespawn(cell));
        }
    }

    // ----- Break & Respawn -----
    private IEnumerator BreakAndRespawn(Vector3Int cell)
    {
        // Platform hilang
        breakableTilemap.SetTile(cell, null);

        // Tunggu jeda respawn
        yield return new WaitForSeconds(respawnTime);

        // Kalau player MASIH berada di area tile (menurut probe yang sama), TUNDA respawn
        while (PlayerOverlapsCell(cell))
            yield return null;

        // Baru respawn kalau area sudah kosong
        breakableTilemap.SetTile(cell, normalTile);

        if (tileStates.TryGetValue(cell, out TileState ts))
        {
            ts.state = BreakState.Normal;
            ts.contactTime = 0f;
            ts.crackedElapsed = 0f;
            ts.debrisSpawned = false;
        }
    }

    // Cek apakah "kaki + probe kiri/kanan" masih berada di atas cell breakable ini
    private bool PlayerOverlapsCell(Vector3Int cell)
    {
        if (!player || !sampleTilemap || !breakableTilemap)
            return false;

        Vector3 feetCenter = (Vector2)player.position + feetOffset;

        Vector3[] probePoints =
        {
            feetCenter,
            feetCenter + new Vector3(-extraFeetCheckOffsetX, 0f, 0f),
            feetCenter + new Vector3( extraFeetCheckOffsetX, 0f, 0f)
        };

        foreach (Vector3 probe in probePoints)
        {
            Vector3Int baseCell = sampleTilemap.WorldToCell(probe);

            // cek cell utama
            if (IsSampleCellMappedToBreakableCell(baseCell, cell))
                return true;

            // cek tetangga kiri/kanan
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0) continue;
                Vector3Int neighbor = baseCell + new Vector3Int(dx, 0, 0);
                if (IsSampleCellMappedToBreakableCell(neighbor, cell))
                    return true;
            }
        }

        return false;
    }

    // Helper: apakah sampleCell (di sampleTilemap) kalau di-offset akan jadi cell breakable yang sama?
    private bool IsSampleCellMappedToBreakableCell(Vector3Int sampleCell, Vector3Int targetBreakableCell)
    {
        if (!sampleTilemap.HasTile(sampleCell))
            return false;

        Vector3Int mapped = sampleCell + breakableCellOffset;
        return mapped == targetBreakableCell;
    }

    // ----- Debris -----
    private void SpawnDebris(Vector3Int cell)
    {
        if (debrisPrefab == null || debrisCount <= 0) return;

        Vector3 center = breakableTilemap.GetCellCenterWorld(cell);

        for (int i = 0; i < debrisCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-debrisOffsetRange.x, debrisOffsetRange.x),
                Random.Range(0f, debrisOffsetRange.y),
                0f
            );

            GameObject d = Instantiate(debrisPrefab, center + offset, Quaternion.identity);
            Destroy(d, timeFromCrackToBreak);
        }
    }

    // ----- Gizmos -----
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !player) return;

        Gizmos.color = Color.yellow;
        Vector3 feetCenter = (Vector2)player.position + feetOffset;
        Gizmos.DrawWireSphere(feetCenter, 0.05f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(feetCenter + new Vector3(-extraFeetCheckOffsetX, 0f, 0f), 0.04f);
        Gizmos.DrawWireSphere(feetCenter + new Vector3( extraFeetCheckOffsetX, 0f, 0f), 0.04f);
    }
}
