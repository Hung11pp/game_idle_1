using IdleDefense.GameServices.Config.Wave;
using IdleDefense.GameServices.Presentation;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public EnemyPool enemyPool;

    [Header("Wave Settings (fallback when Wave Progression SO not on GameServices)")]
    public float baseTimeBetweenSpawns = 1f;
    public float timeBetweenWaves = 3f;
    public int baseEnemiesPerWave = 6;
    public int extraEnemiesPerWave = 2;
    public float spawnDistanceFromCenter = 16f;
    public bool spawnBossEveryTenWaves = true;

    [Header("Boss (fallback)")]
    [Tooltip("Used only when no WaveScalingConfigSO; GameManager.IsBossWave uses GameServices config.")]
    public int bossWaveIntervalFallback = 10;

    private int enemiesToSpawn;
    private int enemiesSpawnedThisWave;
    private bool spawningWave;
    private float spawnTimer;
    private float nextWaveTimer;

    private float ActiveBaseTimeBetweenSpawns =>
        GameServices.Instance != null &&
        GameServices.Instance.WaveProgression != null &&
        GameServices.Instance.WaveProgression.BaseTimeBetweenSpawns > 0f
            ? GameServices.Instance.WaveProgression.BaseTimeBetweenSpawns
            : baseTimeBetweenSpawns;

    private float ActiveTimeBetweenWaves =>
        GameServices.Instance != null &&
        GameServices.Instance.WaveProgression != null &&
        GameServices.Instance.WaveProgression.TimeBetweenWaves > 0f
            ? GameServices.Instance.WaveProgression.TimeBetweenWaves
            : timeBetweenWaves;

    private void Start()
    {
        nextWaveTimer = ActiveTimeBetweenWaves;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (!GameManager.Instance.gameRunning)
        {
            return;
        }

        if (player == null)
        {
            player = GameManager.Instance.player;
        }

        if (enemyPool == null || player == null || player.IsDead())
        {
            return;
        }

        if (!spawningWave)
        {
            if (enemyPool.GetActiveEnemyCount() > 0)
            {
                return;
            }

            nextWaveTimer -= Time.deltaTime;

            if (nextWaveTimer <= 0f)
            {
                BeginWave();
            }

            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && enemiesSpawnedThisWave < enemiesToSpawn)
        {
            SpawnEnemy(false);
            enemiesSpawnedThisWave++;
            spawnTimer = GetCurrentSpawnDelay();
        }

        if (enemiesSpawnedThisWave >= enemiesToSpawn && enemyPool.GetActiveEnemyCount() <= 0)
        {
            FinishWave();
        }
    }

    private void BeginWave()
    {
        int waveNumber = Mathf.Max(1, GameManager.Instance.currentWave);

        WaveProgressionDefinition def = ResolveWaveProgression();

        enemiesToSpawn = def.GetEnemyCountForWave(waveNumber);
        enemiesSpawnedThisWave = 0;
        spawnTimer = 0f;
        spawningWave = true;

        bool spawnBoss = def.SpawnBossOnBossWave;

        if (spawnBoss && GameManager.Instance.IsBossWave())
        {
            SpawnEnemy(true);
        }
    }

    private WaveProgressionDefinition ResolveWaveProgression()
    {
        if (GameServices.Instance != null && GameServices.Instance.WaveProgression != null)
        {
            return GameServices.Instance.WaveProgressionDefinition;
        }

        return new WaveProgressionDefinition
        {
            BaseEnemiesPerWave = baseEnemiesPerWave,
            ExtraEnemiesPerWave = extraEnemiesPerWave,
            SpawnBossOnBossWave = spawnBossEveryTenWaves,
            BossWaveInterval = bossWaveIntervalFallback
        };
    }

    private void FinishWave()
    {
        spawningWave = false;
        nextWaveTimer = ActiveTimeBetweenWaves;
        GameManager.Instance.AdvanceWave();
    }

    private void SpawnEnemy(bool isBoss)
    {
        EnemyController enemy = enemyPool.GetEnemy();

        if (enemy == null)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        enemy.transform.position = spawnPosition;
        enemy.transform.rotation = Quaternion.identity;
        enemy.Setup(player, enemyPool, isBoss, GameManager.Instance.currentWave);
    }

    private Vector3 GetSpawnPosition()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        if (randomDirection == Vector2.zero)
        {
            randomDirection = Vector2.right;
        }

        Vector3 center = player.transform.position;
        Vector3 spawnOffset = new Vector3(randomDirection.x, 0f, randomDirection.y) * spawnDistanceFromCenter;
        return center + spawnOffset;
    }

    private float GetCurrentSpawnDelay()
    {
        float multiplier = 1f;

        if (GameManager.Instance != null)
        {
            multiplier = Mathf.Max(0.2f, GameManager.Instance.spawnRateMultiplier);
        }

        return ActiveBaseTimeBetweenSpawns / multiplier;
    }
}
