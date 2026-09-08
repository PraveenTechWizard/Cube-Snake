using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DynamicBotSpawner : MonoBehaviourPunCallbacks
{
    public static DynamicBotSpawner Instance;

    [Header("Bot Settings")]
    [Tooltip("MUST BE PLACED IN A 'Resources' FOLDER!")]
    [SerializeField] private GameObject botPrefab;

    [Header("Population")]
    [SerializeField] private int maximumBots = 2;
    [SerializeField] private float initialSpawnDelay = 0.35f;
    [SerializeField] private float respawnDelayMin = 1.5f;
    [SerializeField] private float respawnDelayMax = 3.0f;

    [Header("Spawn Area")]
    [SerializeField] private float minimumSpawnDistance = 15f;
    [SerializeField] private float maximumSpawnDistance = 28f;
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("Spawn Safety")]
    [SerializeField] private float spawnCheckRadius = 2f;
    [SerializeField] private LayerMask blockingLayers;
    [SerializeField] private int maximumSpawnAttempts = 15;

    // RUNTIME
    private readonly List<GameObject> _activeBots = new List<GameObject>();
    private int _pendingSpawns;
    private Coroutine _initialSpawnRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CheckAndStartSpawning();
    }

    // PUN CALLBACKS
    public override void OnJoinedRoom()
    {
        CheckAndStartSpawning();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"[BOT SPAWNER] New Master Client: {newMasterClient.ActorNumber}");

        if (!PhotonNetwork.IsMasterClient)
        {
            StopBotPopulation();
            return;
        }

        RebuildBotList();
        CheckAndStartSpawning();
    }

    public override void OnLeftRoom()
    {
        StopBotPopulation();
        _activeBots.Clear();
        _pendingSpawns = 0;
    }

    private void OnDestroy()
    {
        StopBotPopulation();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    // SPAWN MANAGEMENT
    private void CheckAndStartSpawning()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            return;

        StartBotPopulation();
    }

    private void StartBotPopulation()
    {
        if (_initialSpawnRoutine != null)
            return;

        Debug.Log("[BOT SPAWNER] Master starting bot population check.");
        _initialSpawnRoutine = StartCoroutine(InitialSpawnRoutine());
    }

    private IEnumerator InitialSpawnRoutine()
    {
        while (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            // Clean out null references from destroyed bots
            _activeBots.RemoveAll(bot => bot == null);

            // Keep spawning until we hit maximumBots threshold
            while (_activeBots.Count + _pendingSpawns < maximumBots)
            {
                SpawnBot();
                yield return new WaitForSeconds(initialSpawnDelay);
            }

            yield return new WaitForSeconds(1f); // Check periodically
        }

        _initialSpawnRoutine = null;
    }

    private void SpawnBot()
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
            return;

        if (botPrefab == null)
        {
            Debug.LogError("[BOT SPAWNER] Bot Prefab is missing! Assign it in the Inspector.");
            return;
        }

        _activeBots.RemoveAll(bot => bot == null);

        if (_activeBots.Count + _pendingSpawns >= maximumBots)
            return;

        if (!TryGetSpawnPosition(out Vector3 spawnPosition))
        {
            Debug.LogWarning("[BOT SPAWNER] Could not find a valid spawn position.");
            return;
        }

        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // Note: Prefab MUST be located inside Assets/Resources/
        GameObject bot = PhotonNetwork.InstantiateRoomObject(botPrefab.name, spawnPosition, spawnRotation);

        if (bot == null)
        {
            Debug.LogError($"[BOT SPAWNER] Failed to instantiate room object '{botPrefab.name}'. Make sure it is inside a 'Resources' folder!");
            return;
        }

        _activeBots.Add(bot);

        PhotonView botView = bot.GetComponent<PhotonView>();
        Debug.Log($"[BOT SPAWNER] Bot spawned. ViewID = {(botView != null ? botView.ViewID.ToString() : "NO PHOTON VIEW")}");
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        Vector3 centerPosition = Vector3.zero;

        for (int i = 0; i < maximumSpawnAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            if (randomDirection == Vector2.zero)
                randomDirection = Vector2.up;

            float randomDistance = Random.Range(minimumSpawnDistance, maximumSpawnDistance);
            Vector3 candidate = centerPosition + new Vector3(randomDirection.x, 0f, randomDirection.y) * randomDistance;
            candidate.y = spawnHeight;

            if (!Physics.CheckSphere(candidate, spawnCheckRadius, blockingLayers))
            {
                spawnPosition = candidate;
                return true;
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    // PUBLIC API
    public void ReportBotDefeated(GameObject bot)
    {
        if (!PhotonNetwork.IsMasterClient || bot == null)
            return;

        _activeBots.Remove(bot);
        Debug.Log($"[BOT SPAWNER] Bot defeated. Active count = {_activeBots.Count}");

        StartCoroutine(RespawnBotRoutine());
    }

    private IEnumerator RespawnBotRoutine()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        _pendingSpawns++;

        float delay = Random.Range(respawnDelayMin, respawnDelayMax);
        yield return new WaitForSeconds(delay);

        _pendingSpawns--;

        _activeBots.RemoveAll(b => b == null);

        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && _activeBots.Count < maximumBots)
        {
            SpawnBot();
        }
    }

    private void RebuildBotList()
    {
        _activeBots.Clear();

        BotMovement[] bots = FindObjectsByType<BotMovement>(FindObjectsSortMode.None);
        foreach (BotMovement bot in bots)
        {
            if (bot != null && bot.GetComponent<PhotonView>() != null)
            {
                _activeBots.Add(bot.gameObject);
            }
        }

        Debug.Log($"[BOT SPAWNER] Bot list rebuilt. Count = {_activeBots.Count}");
    }

    private void StopBotPopulation()
    {
        if (_initialSpawnRoutine != null)
        {
            StopCoroutine(_initialSpawnRoutine);
            _initialSpawnRoutine = null;
        }
    }
}