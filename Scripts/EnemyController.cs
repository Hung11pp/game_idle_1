using IdleDefense.Core.Enemy;
using IdleDefense.GameServices.Presentation;
using UnityEngine;

/// <summary>
/// Presentation: orchestrates enemy components; combat numbers live in EnemyState + CombatService.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyCombat))]
[RequireComponent(typeof(LootOnDeath))]
public class EnemyController : MonoBehaviour
{
    [Header("Prefab tuning")]
    public float baseMaxHealth = 20f;
    public float baseDamage = 4f;
    public float baseMoveSpeed = 2.5f;
    public float contactRange = 1.2f;
    public float damageTickRate = 1f;

    [Header("Boss visuals (local override if no SO)")]
    public float bossHealthMultiplier = 4f;
    public float bossDamageMultiplier = 2f;
    public float bossSpeedMultiplier = 0.9f;
    public Vector3 bossScale = new Vector3(2f, 2f, 2f);

    [Header("Runtime (debug)")]
    public bool isBoss;
    public int waveNumber;

    public readonly EnemyState State = new EnemyState();

    private PlayerController targetPlayer;
    private EnemyPool sourcePool;
    private Renderer cachedRenderer;
    private GameServices services;

    private Health health;
    private EnemyMovement movement;
    private EnemyCombat combat;
    private LootOnDeath lootOnDeath;

    private Vector3 normalScale;

    private void Awake()
    {
        normalScale = transform.localScale;
        cachedRenderer = GetComponentInChildren<Renderer>();
        gameObject.tag = "Enemy";

        health = GetComponent<Health>();
        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
        lootOnDeath = GetComponent<LootOnDeath>();
    }

    private void OnEnable()
    {
        if (services == null)
        {
            services = FindObjectOfType<GameServices>();
        }

        if (sourcePool != null)
        {
            sourcePool.RegisterActiveEnemy(this);
        }
    }

    public void Setup(PlayerController targetPlayer, EnemyPool sourcePool, bool isBoss, int waveNumber)
    {
        this.targetPlayer = targetPlayer;
        this.sourcePool = sourcePool;
        this.isBoss = isBoss;
        this.waveNumber = waveNumber;

        if (services == null)
        {
            services = FindObjectOfType<GameServices>();
        }

        float gmHealth = 1f;
        float gmDamage = 1f;

        if (GameManager.Instance != null)
        {
            gmHealth = GameManager.Instance.enemyHealthMultiplier;
            gmDamage = GameManager.Instance.enemyDamageMultiplier;
        }

        float bh = bossHealthMultiplier;
        float bd = bossDamageMultiplier;
        float bs = bossSpeedMultiplier;

        if (services != null && services.ScalingDefinition != null)
        {
            bh = services.ScalingDefinition.BossHealthMultiplier;
            bd = services.ScalingDefinition.BossDamageMultiplier;
            bs = services.ScalingDefinition.BossSpeedMultiplier;
        }

        EnemyCombatMath.ComputeFinalStats(
            baseMaxHealth,
            baseDamage,
            baseMoveSpeed,
            gmHealth,
            gmDamage,
            isBoss,
            bh,
            bd,
            bs,
            out float maxHp,
            out float contactDamage,
            out float moveSpeed);

        State.ResetFromSpawn(maxHp, contactDamage, moveSpeed, contactRange, damageTickRate, isBoss, waveNumber);

        health.Configure(State, services, HandleDeath);
        health.ResetForSpawn();

        lootOnDeath.Configure(services, isBoss, waveNumber);

        movement.Configure(State, health, targetPlayer);
        combat.Configure(State, health, targetPlayer, services);
        combat.ResetForSpawn();

        if (this.sourcePool != null)
        {
            this.sourcePool.RegisterActiveEnemy(this);
        }

        if (isBoss)
        {
            transform.localScale = bossScale;
        }
        else
        {
            transform.localScale = normalScale;
        }

        ApplyVisuals();
    }

    public bool IsAlive()
    {
        return health != null && health.IsAlive();
    }

    public void TakeDamage(float damageAmount)
    {
        if (health != null)
        {
            health.TakeDamage(damageAmount);
        }
    }

    private void HandleDeath()
    {
        lootOnDeath.PublishDeathEvent();

        if (sourcePool != null)
        {
            sourcePool.ReturnEnemy(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void ApplyVisuals()
    {
        if (cachedRenderer == null)
        {
            return;
        }

        if (isBoss)
        {
            cachedRenderer.material.color = new Color(0.7f, 0.15f, 0.15f);
        }
        else
        {
            cachedRenderer.material.color = new Color(0.95f, 0.4f, 0.4f);
        }
    }
}
