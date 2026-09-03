using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DynamicBotSpawner : MonoBehaviourPunCallbacks
{
    public static DynamicBotSpawner Instance;

    [Header("Bot")]
    [SerializeField]
    private GameObject botPrefab;


    [Header("Population")]
    [SerializeField]
    private int maximumBots = 6;

    [SerializeField]
    private float initialSpawnDelay = 0.35f;

    [SerializeField]
    private float respawnDelayMin = 1.5f;

    [SerializeField]
    private float respawnDelayMax = 3f;


    [Header("Spawn Area")]
    [SerializeField]
    private float minimumSpawnDistance = 15f;

    [SerializeField]
    private float maximumSpawnDistance = 28f;

    [SerializeField]
    private float spawnHeight = 0.5f;


    [Header("Spawn Safety")]
    [SerializeField]
    private float spawnCheckRadius = 2f;

    [SerializeField]
    private LayerMask blockingLayers;

    [SerializeField]
    private int maximumSpawnAttempts = 15;


    private readonly List<GameObject> _activeBots =
        new List<GameObject>();


    private int _pendingSpawns;

    
    //
    
    private void Awake()
    {
        Instance = this;
    }
    
    // 
    
    private void Start()
    {
        // Only Master creates and controls bots.
        if (!PhotonNetwork.IsMasterClient)
            return;


        StartCoroutine(InitialSpawnRoutine());
    }
    
    // INITIAL SPAWN
    private IEnumerator InitialSpawnRoutine()
    {
        while (_activeBots.Count + _pendingSpawns < maximumBots)
        {
            SpawnBot();

            yield return new WaitForSeconds(initialSpawnDelay);
        }
    }

    
    // SPAWN BOT
    private void SpawnBot()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;


        if (botPrefab == null)
        {
            Debug.LogError("Bot Prefab is not assigned!");
            return;
        }


        if (_activeBots.Count + _pendingSpawns >= maximumBots)
        {
            return;
        }


        if (!TryGetSpawnPosition(out Vector3 spawnPosition))
        {
            Debug.LogWarning("Could not find bot spawn position.");

            return;
        }


        Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);


        // IMPORTANT:
        // Network instantiate.
        // This bot will appear for all players.
        GameObject bot = PhotonNetwork.Instantiate(
                botPrefab.name,
                spawnPosition,
                spawnRotation);


        _activeBots.Add(bot);
        
        Debug.Log("Bot spawned: " + bot.GetComponent<PhotonView>().ViewID);
    }
    
   
    // SPAWN POSITION

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        // Use a room center for consistent spawning.
        Vector3 centerPosition = Vector3.zero;
        
        for (int i = 0; i < maximumSpawnAttempts; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;


            float randomDistance =
                Random.Range(minimumSpawnDistance, maximumSpawnDistance);
            
            
            Vector3 candidate = centerPosition + new Vector3(randomDirection.x, 0f, randomDirection.y) * randomDistance;

            candidate.y = spawnHeight;
            
            bool blocked = Physics.CheckSphere(candidate, spawnCheckRadius, blockingLayers);
            
            if (blocked)
                continue;


            spawnPosition =
                candidate;


            return true;
        }


        spawnPosition =
            Vector3.zero;


        return false;
    }

    
    // BOT DEFEATED

    public void ReportBotDefeated(GameObject bot)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;


        if (bot == null)
            return;


        if (!_activeBots.Contains(bot))
            return;


        _activeBots.Remove(bot);
        
        StartCoroutine(RespawnBotRoutine());
    }
    
    
    // RESPAWN
    private IEnumerator RespawnBotRoutine()
    {
        _pendingSpawns++;

        float delay = Random.Range(respawnDelayMin, respawnDelayMax);
        
        yield return new WaitForSeconds(delay);
        
        _pendingSpawns--;
        
        if (!PhotonNetwork.IsMasterClient)
            yield break;
        
        if (_activeBots.Count >= maximumBots)
        {
            yield break;
        }
        
        SpawnBot();
    }

    
    // MASTER CLIENT CHANGED

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        // If this client becomes Master,
        // it should take responsibility for bots.
        if (PhotonNetwork.IsMasterClient)
        {
            RebuildBotList();
        }
    }

    
    // REBUILD BOT LIST
    private void RebuildBotList()
    {
        _activeBots.Clear();
        
        BotMovement[] bots = FindObjectsByType<BotMovement>(FindObjectsSortMode.None);
        
        foreach (BotMovement bot in bots)
        {
            if (bot != null)
            {
                _activeBots.Add(bot.gameObject);
            }
        }
        
        Debug.Log("Bot list rebuilt. Count: " + _activeBots.Count);
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}