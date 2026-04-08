using IdleDefense.GameServices.Config.Scaling;
using IdleDefense.GameServices.Presentation;
using TMPro;
using UnityEngine;

/// <summary>
/// Presentation: wave + game flow; difficulty uses EnemyScalingDefinition (exponential) when SO is assigned on GameServices.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool gameRunning = true;
    public int currentWave = 1;

    [Header("Difficulty Scaling (fallback if no EnemyScaling SO on GameServices)")]
    public float enemyHealthMultiplier = 1f;
    public float enemyDamageMultiplier = 1f;
    public float spawnRateMultiplier = 1f;

    [Tooltip("Compound per-wave factors when not using EnemyScalingConfigSO.")]
    public float healthGrowthPerWave = 1.12f;

    public float damageGrowthPerWave = 1.06f;
    public float spawnRateGrowthPerWave = 1.042f;

    [Header("References")]
    public PlayerController player;
    public WaveSpawner waveSpawner;

    [Header("Boss (fallback when no WaveScalingConfigSO on GameServices)")]
    [Tooltip("Used when waveSpawner is null; otherwise WaveSpawner.bossWaveIntervalFallback is used.")]
    public int bossWaveIntervalFallback = 10;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text stateText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        gameRunning = true;
        currentWave = 1;
        RecalculateDifficulty();
        UpdateUI();

        if (stateText != null)
        {
            stateText.text = "";
        }
    }

    public void AdvanceWave()
    {
        currentWave++;
        RecalculateDifficulty();
        UpdateUI();
    }

    private EnemyScalingDefinition ResolveScalingDefinition()
    {
        if (GameServices.Instance != null && GameServices.Instance.EnemyScaling != null)
        {
            return GameServices.Instance.ScalingDefinition;
        }

        return new EnemyScalingDefinition
        {
            HealthGrowthPerWave = healthGrowthPerWave,
            DamageGrowthPerWave = damageGrowthPerWave,
            SpawnRateGrowthPerWave = spawnRateGrowthPerWave
        };
    }

    private void RecalculateDifficulty()
    {
        EnemyScalingDefinition def = ResolveScalingDefinition();
        def.GetDifficultyMultipliers(currentWave, out enemyHealthMultiplier, out enemyDamageMultiplier, out spawnRateMultiplier);
    }

    public bool IsBossWave()
    {
        if (GameServices.Instance != null && GameServices.Instance.WaveProgression != null)
        {
            return GameServices.Instance.WaveProgressionDefinition.IsBossWave(currentWave);
        }

        int interval = bossWaveIntervalFallback;

        if (waveSpawner != null)
        {
            interval = waveSpawner.bossWaveIntervalFallback;
        }

        if (interval <= 0)
        {
            return false;
        }

        return currentWave > 0 && currentWave % interval == 0;
    }

    public void GameOver()
    {
        if (!gameRunning)
        {
            return;
        }

        gameRunning = false;

        if (stateText != null)
        {
            stateText.text = "Game Over";
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWave;
        }
    }
}
