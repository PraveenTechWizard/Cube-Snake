using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class SnakeController : MonoBehaviourPunCallbacks
{
    [Header("Starting Settings")]
    [SerializeField] private int startingValue = 2;

    [Header("Body Settings")]
    [SerializeField] private float bodySpacing = 1.2f;

    [Header("Merge Settings")]
    [SerializeField] private float mergeDelay = 0.3f;

    [Header("Head")]
    [SerializeField] private TextMeshPro headText;

    [Header("Snake Follow Settings")]
    [SerializeField] private float pathPointDistance = 0.08f;
    [SerializeField] private float bodyFollowSpeed = 18f;
    [SerializeField] private float bodyRotationSpeed = 14f;
    [SerializeField] private int pathBufferPoints = 20;

    private readonly List<int> _values = new List<int>();
    private readonly List<GameObject> _bodyCubes = new List<GameObject>();
    private readonly List<Vector3> _movementPath = new List<Vector3>(256);

    private bool _isMerging;
    private bool _isDefeated;

    private Vector3 _lastRecordedPosition;

    private int _pathTrimThreshold = 64;


    
    // PUBLIC
    public int HeadValue
    {
        get
        {
            if (_values.Count == 0)
                return 0;

            return _values[0];
        }
    }


    public bool IsDefeated => _isDefeated;


    // UNITY
    private void Awake()
    {
        _values.Add(startingValue);

        _bodyCubes.Add(gameObject);

        _movementPath.Add(transform.position);

        _lastRecordedPosition = transform.position;
    }


    private void Start()
    {
        UpdateAllVisuals();

        UpdateBodyPositions();

        PrintSnake();
    }


    private void Update()
    {
        if (_isDefeated)
            return;

        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }


    private void LateUpdate()
    {
        if (_isDefeated)
            return;

        RecordHeadPath();

        FollowHeadPath();
    }

    
    // COLLECT REQUEST
    public bool CollectValue(int value, GameObject target)
    {
        if (!photonView.IsMine)
            return false;

        if (_isDefeated)
            return false;

        if (target == null)
            return false;

        PhotonView targetView = target.GetComponent<PhotonView>();

        if (targetView == null)
        {
            Debug.LogWarning($"[{name}] Target {target.name} has no PhotonView.");

            return false;
        }

        // Player/Bot value must be smaller.
        if (value > HeadValue)
            return false;

        SnakeController targetSnake = target.GetComponent<SnakeController>();
        
        // SNAKE HEAD
        if (targetSnake != null && targetSnake != this)
        {
            photonView.RPC(nameof(RPC_RequestCollectSnake), RpcTarget.MasterClient, targetView.ViewID, value);
            return true;
        }
        
        // NORMAL CUBE / BODY
        photonView.RPC(nameof(RPC_RequestCollectCube), RpcTarget.MasterClient, targetView.ViewID, value);

        return true;
    }
    //
    [PunRPC]
    private void RPC_RequestCollectSnake(int targetSnakeViewID, int requestedValue)
    {
        // Only Master decides the result.
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonView targetView = PhotonView.Find(targetSnakeViewID);

        if (targetView == null)
            return;

        SnakeController targetSnake = targetView.GetComponent<SnakeController>();

        if (targetSnake == null)
            return;

        // Don't collect yourself.
        if (targetSnake == this)
            return;

       
        // GET REAL VALUES
        int collectorValue =
            HeadValue;

        int targetValue = targetSnake.HeadValue;

        Debug.Log($"[MASTER SNAKE COLLISION] " + $"{name} = {collectorValue} | " + $"{targetSnake.name} = {targetValue}");
        
        // COLLECTOR MUST BE STRONGER
        if (collectorValue <= targetValue)
        {
            Debug.Log($"[MASTER] Collection rejected: " + $"{collectorValue} <= {targetValue}");
            return;
        }
        
        // SEND TO EVERY CLIENT
        photonView.RPC(nameof(RPC_CollectSnake), RpcTarget.All, targetSnakeViewID, targetValue);
    }
    
    //
    [PunRPC]
    private void RPC_CollectSnake(int defeatedSnakeViewID, int value)
    {
        PhotonView defeatedView = PhotonView.Find(defeatedSnakeViewID);

        if (defeatedView == null)
        {
            Debug.LogWarning($"Defeated snake ViewID {defeatedSnakeViewID} not found.");
            return;
        }

        SnakeController defeatedSnake = defeatedView.GetComponent<SnakeController>();

        if (defeatedSnake == null)
            return;

        if (defeatedSnake == this)
            return;
        
        // SAFETY CHECK
        if (value > HeadValue)
        {
            Debug.LogWarning(
                $"[{name}] Cannot collect snake value {value}. " +
                $"My value = {HeadValue}");

            return;
    }

        Debug.Log($"[{name}] COLLECTING {defeatedSnake.name} " + $"VALUE = {value}");
    
    // DEFEAT TARGET
    defeatedSnake._isDefeated = true;

    // Release defeated snake body.
    int cubeLayer = LayerMask.NameToLayer("Cube");

    defeatedSnake.ReleaseBodyOnDefeat(cubeLayer);

    defeatedSnake.DisableSnakeSystems();

   
    // ADD DEFEATED HEAD TO WINNER
    GameObject defeatedHead = defeatedSnake.gameObject;

    // Remove from defeated snake's body list
    defeatedSnake._bodyCubes.Clear();
    defeatedSnake._values.Clear();

    // Winner gets the defeated value.
    _values.Add(value);

    _bodyCubes.Add(defeatedHead);

    // Parent defeated head to winner.
    defeatedHead.transform.SetParent(transform, true);

    // Set body layer.
    SetBodyLayer(defeatedHead);

    // Make defeated head behave as a body cube.
    Rigidbody rb = defeatedHead.GetComponent<Rigidbody>();

    if (rb != null)
    {
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;

            rb.angularVelocity = Vector3.zero;
        }

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    
    // SORT
    SortSnakeByValue();

    UpdateAllVisuals();

    UpdateBodyPositions();

    PrintSnake();

    // MERGE
    if (!_isMerging)
    {
        StartCoroutine(MergeRoutine());
    }
}
    
    [PunRPC]
    private void RPC_RequestCollectCube(int targetViewID, int requestedValue)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonView targetView = PhotonView.Find(targetViewID);

        if (targetView == null)
            return;

        GameObject target = targetView.gameObject;

        NumberCube cube = target.GetComponent<NumberCube>();

        if (cube == null)
            return;

        // Get the collector.
        SnakeController collector = GetComponent<SnakeController>();

        if (collector == null)
            return;

        int actualValue = cube.CubeValue;

        // Master validates value.
        if (actualValue > collector.HeadValue)
            return;

        collector.photonView.RPC(nameof(RPC_CollectCube), RpcTarget.All, targetViewID, actualValue);
    }
    // COLLECT ON ALL CLIENTS
    [PunRPC]
    private void RPC_CollectCube(int cubeViewID, int value)
    {
        PhotonView cubeView = PhotonView.Find(cubeViewID);

        if (cubeView == null)
            return;

        GameObject collectedCube = cubeView.gameObject;

        if (collectedCube == null)
            return;

        CollectValueInternal(value, collectedCube);
    }
    
    // INTERNAL COLLECTION
    private bool CollectValueInternal(int value, GameObject collectedCube)
    {
        if (_isDefeated)
        return false;

        if (collectedCube == null)
            return false;

        if (collectedCube == gameObject)
            return false;

        if (_bodyCubes.Contains(collectedCube))
            return false;

    
        // CHECK VALUE BEFORE COLLECTING
        if (value > HeadValue)
            return false;

        
        // FIND OLD OWNER
        SnakeController oldOwner =
            GetOwnerOfCube(collectedCube);


        if (oldOwner != null &&
            oldOwner != this)
        {
            oldOwner.RemoveBodyCubeInternal(
                collectedCube
            );
        }

    
    // PHYSICS
    Rigidbody rb = collectedCube.GetComponent<Rigidbody>();

    if (rb != null)
    {
        // Velocity MUST be cleared BEFORE
        // making Rigidbody kinematic.

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;

            rb.angularVelocity = Vector3.zero;
        }

        rb.useGravity = false;
        rb.isKinematic = true;
    }
    
    // ADD TO SNAKE
    _values.Add(value);

    _bodyCubes.Add(collectedCube);
    
    collectedCube.transform.SetParent(transform, true);
    
    // SET BODY LAYER
    SetBodyLayer(collectedCube);
    
    // UPDATE SNAKE
    SortSnakeByValue();

    UpdateAllVisuals();

    UpdateBodyPositions();

    PrintSnake();
    
    // MERGE

    if (!_isMerging)
    {
        StartCoroutine(MergeRoutine());
    }

    return true; 
    }
    // FIND OWNER
    private SnakeController GetOwnerOfCube(GameObject cube)
    {
        Transform currentParent = cube.transform.parent;

        while (currentParent != null)
        {
            SnakeController snake = currentParent.GetComponent<SnakeController>();

            if (snake != null)
                return snake;
            
            currentParent = currentParent.parent;
        }


        return null;
    }
    
   
    // REMOVE BODY CUBE REQUEST
    public bool RemoveBodyCube(GameObject cube)
    {
        if (cube == null)
            return false;
        
        PhotonView cubeView = cube.GetComponent<PhotonView>();
        
        if (cubeView == null)
        {
            return RemoveBodyCubeInternal(cube);
        }

        photonView.RPC(nameof(RPC_RemoveBodyCube), RpcTarget.All, cubeView.ViewID);
        
        return true;
    }
    
    // REMOVE BODY CUBE ON ALL CLIENTS
    [PunRPC]
    private void RPC_RemoveBodyCube(int cubeViewID)
    {
        PhotonView cubeView = PhotonView.Find(cubeViewID);

        if (cubeView == null)
            return;

        RemoveBodyCubeInternal(cubeView.gameObject);
    }
    
    
    // INTERNAL REMOVE
    private bool RemoveBodyCubeInternal(GameObject cube)
    {
        int index = _bodyCubes.IndexOf(cube);

        if (index == -1)
            return false;

        // Never remove snake head
        if (index == 0)
            return false;
        
        _bodyCubes.RemoveAt(index);

        _values.RemoveAt(index);
        
        cube.transform.SetParent(null, true);
        
        SortSnakeByValue();

        UpdateAllVisuals();

        PrintSnake();

        return true;
    }
    
    // DEFEAT SNAKE
    [PunRPC]
    private void RPC_DefeatSnake(int defeatedSnakeViewID, int collectorViewID)
    {
        PhotonView defeatedView = PhotonView.Find(defeatedSnakeViewID);
        
        PhotonView collectorView = PhotonView.Find(collectorViewID);

        if (defeatedView == null)
            return;

        if (collectorView == null)
            return;
        
        SnakeController defeatedSnake = defeatedView.GetComponent<SnakeController>();

        if (defeatedSnake == null)
            return;
        
        if (defeatedSnake._isDefeated)
            return;
        
        defeatedSnake._isDefeated =
            true;
        
        // Release all body cubes
        int cubeLayer = LayerMask.NameToLayer("Cube");
        
        defeatedSnake.ReleaseBodyOnDefeat(cubeLayer);

        // Disable movement and gameplay
        defeatedSnake.DisableSnakeSystems();

        Debug.Log($"Snake defeated: {defeatedSnake.gameObject.name}");
    }

    
    // RELEASE BODY ON DEFEAT
    public void ReleaseBodyOnDefeat(int worldCubeLayer)
    {
        for (int i = 1;
             i < _bodyCubes.Count;
             i++)
        {
            GameObject cube =
                _bodyCubes[i];


            if (cube == null)
                continue;


            cube.transform.SetParent(null, true);
            
            cube.layer = worldCubeLayer;

            Rigidbody rb = cube.GetComponent<Rigidbody>();
            
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;

                rb.isKinematic = true;

                rb.useGravity = false;
            }
        }

        if (_bodyCubes.Count > 1)
        {
            _bodyCubes.RemoveRange(1, _bodyCubes.Count - 1);
        }

        if (_values.Count > 1)
        {
            _values.RemoveRange(1, _values.Count - 1);
        }
        
        UpdateAllVisuals();
    }

    
    // DISABLE SNAKE SYSTEMS
    public void DisableSnakeSystems()
    {
        PlayerMovementControl playerMovement = GetComponent<PlayerMovementControl>();
        
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        BotMovement botMovement = GetComponent<BotMovement>();
        
        if (botMovement != null)
        {
            botMovement.enabled = false;
        }

        BotTargetDetector detector = GetComponent<BotTargetDetector>();

        if (detector != null)
        {
            detector.enabled = false;
        }
        
        BotCollisionAction botCollision = GetComponent<BotCollisionAction>();
        
        if (botCollision != null)
        {
            botCollision.enabled = false;
        }
        
        CollisionAction playerCollision = GetComponent<CollisionAction>();

        if (playerCollision != null)
        {
            playerCollision.enabled = false;
        }
    }
    
    // SORT SNAKE
    private void SortSnakeByValue()
    {
        for (int i = 1; i < _values.Count - 1; i++)
        {
            for (int j = i + 1; j < _values.Count; j++)
            {
                if (_values[j] > _values[i])
                {
                    int tempValue = _values[i];

                    _values[i] = _values[j];

                    _values[j] = tempValue;


                    GameObject tempCube = _bodyCubes[i];

                    _bodyCubes[i] = _bodyCubes[j];

                    _bodyCubes[j] = tempCube;
                }
            }
        }
    }
    
    // BODY POSITION UPDATE
    private void UpdateBodyPositions()
    {
        if (_bodyCubes.Count <= 1)
            return;

        if (_movementPath.Count == 0)
            return;


        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            GameObject cube = _bodyCubes[i];
            
            if (cube == null)
                continue;
            
            GetFollowPoint(i, out Vector3 targetPosition, out Vector3 targetDirection);
            
            targetPosition.y = 0.5f;
            
            cube.transform.position = targetPosition;

            targetDirection.y = 0f;

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                cube.transform.rotation = Quaternion.LookRotation(targetDirection);
            }
        }
    }
    
    // RECORD HEAD PATH
    private void RecordHeadPath()
    {
        if (pathPointDistance <= 0.001f)
        {
            pathPointDistance = 0.001f;
        }
        
        Vector3 currentPosition = transform.position;
        
        Vector3 movement = currentPosition - _lastRecordedPosition;
        
        movement.y = 0f;
        
        float distance = movement.magnitude;

        if (distance < pathPointDistance)
        {
            return;
        }
        
        Vector3 direction = movement / distance;
        
        while (distance >= pathPointDistance)
        {
            _lastRecordedPosition += direction * pathPointDistance;

            _lastRecordedPosition.y = currentPosition.y;
            
            _movementPath.Add(_lastRecordedPosition);
            
            distance -= pathPointDistance;
        }
        
        TrimMovementPath();
    }
    
    
    // TRIM PATH
    private void TrimMovementPath()
    {
        float requiredDistance = (_bodyCubes.Count + pathBufferPoints) * bodySpacing;
        
        int requiredPoints = Mathf.CeilToInt(requiredDistance / pathPointDistance) + 2;

        int excessPoints = _movementPath.Count - requiredPoints;

        if (excessPoints < _pathTrimThreshold)
        {
            return;
        }
        
        _movementPath.RemoveRange(0, excessPoints);
    }
    
    
    // FOLLOW HEAD PATH
    private void FollowHeadPath()
    {
        if (_bodyCubes.Count <= 1)
            return;

        if (_movementPath.Count < 2)
            return;
        
        float positionLerp = Mathf.Clamp01(bodyFollowSpeed * Time.deltaTime);

        float rotationLerp = Mathf.Clamp01(bodyRotationSpeed * Time.deltaTime);
        
        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            GameObject cube = _bodyCubes[i];

            if (cube == null)
                continue;
            
            GetFollowPoint(i, out Vector3 targetPosition, out Vector3 targetDirection);

            targetPosition.y = 0.5f;
            
            cube.transform.position = Vector3.Lerp(cube.transform.position, targetPosition, positionLerp);

            targetDirection.y = 0f;
            
            if (targetDirection.sqrMagnitude <= 0.001f)
            {
                continue;
            }
            
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            cube.transform.rotation = Quaternion.Slerp(cube.transform.rotation, targetRotation, rotationLerp);
        }
    }


    
    // GET FOLLOW POINT
    private void GetFollowPoint(int bodyIndex, out Vector3 targetPosition, out Vector3 targetDirection)
    {
        float distanceBehind = bodySpacing * bodyIndex;
        
        float pointOffset =
            distanceBehind /
            pathPointDistance;
        
        int newestIndex = _movementPath.Count - 1;


        float targetIndex = newestIndex - pointOffset;

        if (targetIndex <= 0f)
        {
            targetPosition = _movementPath[0];

            if (_movementPath.Count > 1)
            {
                targetDirection = _movementPath[1] - _movementPath[0];
            }
            else
            {
                targetDirection = transform.forward;
            }

            return;
        }
        
        int lowerIndex = Mathf.FloorToInt(targetIndex);

        int upperIndex = lowerIndex + 1;
        
        if (upperIndex > newestIndex)
        {
            upperIndex = newestIndex;
        }
        
        float interpolation = targetIndex - lowerIndex;

        Vector3 lowerPoint = _movementPath[lowerIndex];

        Vector3 upperPoint = _movementPath[upperIndex];

        targetPosition = Vector3.Lerp(lowerPoint, upperPoint, interpolation);

        targetDirection = upperPoint - lowerPoint;
        
        if (targetDirection.sqrMagnitude <= 0.001f)
        {
            targetDirection = transform.forward;
        }
    }
    
    // MERGE ROUTINE
    private IEnumerator MergeRoutine()
    {
        _isMerging = true;

        while (true)
        {
            bool foundMerge = false;
            
            for (int i = _values.Count - 1; i > 0; i--)
            {
                if (_values[i] != _values[i - 1])
                {
                    continue;
                }

                foundMerge = true;
                
                GameObject firstCube = _bodyCubes[i - 1];
                
                GameObject secondCube = _bodyCubes[i];
                
                yield return new WaitForSeconds(mergeDelay);
                
                int firstIndex = _bodyCubes.IndexOf(firstCube);
                
                int secondIndex = _bodyCubes.IndexOf(secondCube);
                
                if (firstIndex == -1 || secondIndex == -1)
                {
                    break;
                }


                if (secondIndex != firstIndex + 1)
                {
                    break;
                }


                if (_values[firstIndex] != _values[secondIndex])
                {
                    break;
                }


                // Only Master decides network merge
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonView firstView = firstCube.GetComponent<PhotonView>();
                    
                    PhotonView secondView = secondCube.GetComponent<PhotonView>();
                    
                    if (firstView != null && secondView != null)
                    {
                        photonView.RPC(nameof(RPC_MergeCubes), RpcTarget.All, firstView.ViewID, secondView.ViewID);
                    }
                    else
                    {
                        MergeAtIndex(firstIndex);
                    }
                }
                
                break;
            }


            if (!foundMerge)
                break;

            yield return null;
        }
        
        _isMerging = false;
    }
   
    // MERGE ON ALL CLIENTS
    [PunRPC]
    private void RPC_MergeCubes(int firstCubeViewID, int secondCubeViewID)
    {
        PhotonView firstView = PhotonView.Find(firstCubeViewID);

        PhotonView secondView = PhotonView.Find(secondCubeViewID);
        
        if (firstView == null || secondView == null)
        {
            return;
        }

        GameObject firstCube = firstView.gameObject;

        GameObject secondCube = secondView.gameObject;
        
        int firstIndex = _bodyCubes.IndexOf(firstCube);

        int secondIndex = _bodyCubes.IndexOf(secondCube);

        if (firstIndex == -1 || secondIndex == -1)
        {
            return;
        }

        if (secondIndex != firstIndex + 1)
        {
            return;
        }
        
        if (_values[firstIndex] != _values[secondIndex])
        {
            return;
        }
        
        // UPDATE VALUE
        _values[firstIndex] *= 2;
        
       
        // REMOVE SECOND CUBE FROM LIST
        _values.RemoveAt(secondIndex);

        _bodyCubes.RemoveAt(secondIndex);


        
        // DESTROY NETWORK OBJECT
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(
                secondCube
            );
        }


       
        // SORT AGAIN
        SortSnakeByValue();

        UpdateAllVisuals();

        UpdateBodyPositions();

        PrintSnake();
    }

   
    // INTERNAL MERGE
    private void MergeAtIndex(int firstIndex)
    {
        int secondIndex = firstIndex + 1;


        if (firstIndex < 0)
            return;

        if (secondIndex >= _values.Count)
            return;


        _values[firstIndex] *= 2;


        GameObject cubeToDestroy = _bodyCubes[secondIndex];
        
        _values.RemoveAt(secondIndex);
        
        _bodyCubes.RemoveAt(secondIndex);

        
        // DESTROY NETWORK CUBE
        if (cubeToDestroy != null && cubeToDestroy != gameObject)
        {
            PhotonView cubeView = cubeToDestroy.GetComponent<PhotonView>();
            
            if (cubeView != null)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(cubeToDestroy);
                }
            }
            else
            {
                Destroy(cubeToDestroy);
            }
        }
        
        SortSnakeByValue();

        UpdateAllVisuals();

        UpdateBodyPositions();

        PrintSnake();
    }


   
    // SET BODY LAYER
    private void SetBodyLayer(GameObject cube)
    {
        if (cube == null)
            return;


        int playerLayer = LayerMask.NameToLayer("Player");
        
        int botLayer = LayerMask.NameToLayer("Bot");

        int bodyLayer;


        if (gameObject.layer == playerLayer)
        {
            bodyLayer = LayerMask.NameToLayer("PlayerBody");
        }
        else if (gameObject.layer == botLayer)
        {
            bodyLayer = LayerMask.NameToLayer("BotBody");
        }
        else
        {
            bodyLayer = LayerMask.NameToLayer("Cube");
        }
        
        cube.layer = bodyLayer;
    }
    
    
    // VISUALS
    private void UpdateAllVisuals()
    {
        UpdateHeadVisual();

        for (int i = 1; i < _bodyCubes.Count; i++)
        {
            UpdateSegmentVisual(i);
        }
    }
    
    private void UpdateSegmentVisual(int index)
    {
        if (index < 0 || index >= _bodyCubes.Count)
        {
            return;
        }
        
        GameObject cube = _bodyCubes[index];
        
        if (cube == null)
            return;

        int value = _values[index];


        NumberCube numberCube =
            cube.GetComponent<
                NumberCube>();
        
        if (numberCube != null)
        {
            numberCube.SetValue(value);
        }


        TextMeshPro text = cube.GetComponentInChildren<TextMeshPro>();

        if (text != null)
        {
            text.text = value.ToString();
        }
    }


    private void UpdateHeadVisual()
    {
        if (_values.Count == 0)
            return;


        int value = _values[0];


        if (headText != null)
        {
            headText.text = value.ToString();
        }
        
        NumberCube numberCube = GetComponent<NumberCube>();


        if (numberCube != null)
        {
            numberCube.SetValue(value);
        }
    }


   
    // GET BODY CUBES
    public List<GameObject> GetBodyCubes()
    {
        return _bodyCubes;
    }
   
    // DEBUG
    private void PrintSnake()
    {
        string result = $"{gameObject.name} Snake: ";
        
        foreach (int value in _values)
        {
            result += $"[{value}]";
        }

        Debug.Log(result);
    }
}

