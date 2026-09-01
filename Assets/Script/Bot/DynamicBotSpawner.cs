using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicBotSpawner : MonoBehaviour
{
    public static DynamicBotSpawner Instance;

    [Header("Bot")]
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private Transform player;

    [Header("Population")]
    [SerializeField] private int maximumBots = 6;
    [SerializeField] private float initialSpawnDelay = 0.35f;
    [SerializeField] private float respawnDelayMin = 1.5f;
    [SerializeField] private float respawnDelayMax = 3f;

    [Header("Spawn Area")]
    [SerializeField] private float minimumSpawnDistance = 15f;
    [SerializeField] private float maximumSpawnDistance = 28f;
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("Spawn Safety")]
    [SerializeField] private float spawnCheckRadius = 2f;
    [SerializeField] private LayerMask blockingLayers;
    [SerializeField] private int maximumSpawnAttempts = 15;

    private readonly List<GameObject> _activeBots = new List<GameObject>();
    private int _pendingSpawns;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(InitialSpawnRoutine());
    }

    private IEnumerator InitialSpawnRoutine()
    {
        while (_activeBots.Count + _pendingSpawns < maximumBots)
        {
            SpawnBot();
            yield return new WaitForSeconds(initialSpawnDelay);
        }
    }

    private void SpawnBot()
    {
        if (botPrefab == null || player == null) return;
        if (_activeBots.Count + _pendingSpawns >= maximumBots) return;

        if (!TryGetSpawnPosition(out Vector3 spawnPosition)) return;

        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject bot = Instantiate(botPrefab, spawnPosition, spawnRotation);

        _activeBots.Add(bot);
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        for (int i = 0; i < maximumSpawnAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minimumSpawnDistance, maximumSpawnDistance);

            Vector3 candidate = player.position + new Vector3(randomDirection.x, 0f, randomDirection.y) * randomDistance;
            candidate.y = spawnHeight;

            if (Physics.CheckSphere(candidate, spawnCheckRadius, blockingLayers)) continue;

            spawnPosition = candidate;
            return true;
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    public void ReportBotDefeated(GameObject bot)
    {
        if (bot == null) return;

        _activeBots.Remove(bot);

        if (_activeBots.Count + _pendingSpawns >= maximumBots) return;

        StartCoroutine(RespawnBotRoutine());
    }

    private IEnumerator RespawnBotRoutine()
    {
        _pendingSpawns++;

        float delay = Random.Range(respawnDelayMin, respawnDelayMax);

        yield return new WaitForSeconds(delay);

        _pendingSpawns--;

        if (_activeBots.Count >= maximumBots) yield break;

        SpawnBot();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}