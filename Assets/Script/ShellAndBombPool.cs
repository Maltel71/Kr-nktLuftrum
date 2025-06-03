using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellAndBombPool : MonoBehaviour
{
    private static ShellAndBombPool instance;
    public static ShellAndBombPool Instance => instance;

    [Header("Shell Settings")]
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private int shellPoolSize = 20;

    [Header("Bomb Settings")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private int bombPoolSize = 10;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = false;

    private Queue<GameObject> shellPool;
    private Queue<GameObject> bombPool;
    private Transform shellContainer;
    private Transform bombContainer;

    // Statistik för debugging
    private int totalShellsCreated = 0;
    private int totalBombsCreated = 0;
    private int totalShellsReused = 0;
    private int totalBombsReused = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            ValidateSetup();
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ValidateSetup()
    {
        if (shellPrefab == null)
            Debug.LogWarning("Shell prefab not assigned!");
        if (bombPrefab == null)
            Debug.LogWarning("Bomb prefab not assigned!");
    }

    private void InitializePools()
    {
        DebugLog("Initializing shell and bomb pools...");

        shellPool = new Queue<GameObject>();
        bombPool = new Queue<GameObject>();

        shellContainer = new GameObject("ShellPool").transform;
        bombContainer = new GameObject("BombPool").transform;
        shellContainer.parent = transform;
        bombContainer.parent = transform;

        for (int i = 0; i < shellPoolSize; i++)
        {
            CreateShell();
        }

        for (int i = 0; i < bombPoolSize; i++)
        {
            CreateBomb();
        }

        DebugLog($"Pool initialization complete. Created {shellPoolSize} shells and {bombPoolSize} bombs.");
    }

    private void CreateShell()
    {
        if (shellPrefab == null) return;
        GameObject shell = Instantiate(shellPrefab, shellContainer);
        shell.SetActive(false);
        shellPool.Enqueue(shell);
        totalShellsCreated++;
    }

    private void CreateBomb()
    {
        if (bombPrefab == null) return;
        GameObject bomb = Instantiate(bombPrefab, bombContainer);
        bomb.SetActive(false);
        bombPool.Enqueue(bomb);
        totalBombsCreated++;
    }

    public GameObject GetShell()
    {
        if (shellPool.Count == 0)
        {
            DebugLog("Shell pool empty, creating new shell");
            CreateShell();
        }
        GameObject shell = shellPool.Dequeue();
        shell.SetActive(true);
        totalShellsReused++;
        DebugLog($"Shell retrieved from pool. Total reused: {totalShellsReused}");
        return shell;
    }

    public GameObject GetBomb()
    {
        if (bombPool.Count == 0)
        {
            DebugLog("Bomb pool empty, creating new bomb");
            CreateBomb();
        }
        GameObject bomb = bombPool.Dequeue();
        bomb.SetActive(true);
        totalBombsReused++;
        DebugLog($"Bomb retrieved from pool. Total reused: {totalBombsReused}");
        return bomb;
    }

    public void ReturnShell(GameObject shell)
    {
        shell.SetActive(false);
        shell.transform.position = shellContainer.position;
        ResetRigidbody(shell);
        shellPool.Enqueue(shell);
        DebugLog("Shell returned to pool");
    }

    public void ReturnBomb(GameObject bomb)
    {
        bomb.SetActive(false);
        bomb.transform.position = bombContainer.position;
        ResetRigidbody(bomb);
        bombPool.Enqueue(bomb);
        DebugLog("Bomb returned to pool");
    }

    private void ResetRigidbody(GameObject obj)
    {
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ReturnToPool(GameObject obj, float delay)
    {
        StartCoroutine(ReturnWithDelay(obj, delay));
    }

    private IEnumerator ReturnWithDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj.CompareTag("Shell"))
            ReturnShell(obj);
        else if (obj.CompareTag("Bomb"))
            ReturnBomb(obj);
    }

    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            //Debug.Log($"[ShellAndBombPool] {message}");
        }
    }

    public void PrintPoolStatistics()
    {
        Debug.Log($"=== ShellAndBombPool Statistics ===\n" +
                  $"Total Shells Created: {totalShellsCreated}\n" +
                  $"Total Bombs Created: {totalBombsCreated}\n" +
                  $"Total Shells Reused: {totalShellsReused}\n" +
                  $"Total Bombs Reused: {totalBombsReused}\n" +
                  $"Current Shell Pool Size: {shellPool?.Count ?? 0}\n" +
                  $"Current Bomb Pool Size: {bombPool?.Count ?? 0}");
    }
}