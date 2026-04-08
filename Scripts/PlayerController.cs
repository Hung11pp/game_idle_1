using UnityEngine;

/// <summary>
/// Scene bridge: wires GameServices to player presentation (combat adapter + UI), movement hooks, damage intake.
/// </summary>
[RequireComponent(typeof(PlayerCombatAdapter))]
[RequireComponent(typeof(PlayerUI))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public GameServices Services;
    public EnemyPool enemyPool;

    private PlayerCombatAdapter _combat;
    private PlayerUI _ui;

    private void Awake()
    {
        if (Services == null)
        {
            Services = FindObjectOfType<GameServices>();
        }

        _combat = GetComponent<PlayerCombatAdapter>();
        _ui = GetComponent<PlayerUI>();
    }

    private void Start()
    {
        if (Services == null || Services.Player == null)
        {
            Debug.LogError("PlayerController requires GameServices with PlayerState.");
            return;
        }

        Services.Player.InitializeAtFullHealth();
        _combat.Initialize(Services, enemyPool, transform);
        _ui.Initialize(Services);
    }

    private void Update()
    {
        if (Services == null || Services.Player == null)
        {
            return;
        }

        if (Services.Player.IsDead)
        {
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.gameRunning)
        {
            return;
        }

        _ui.Refresh();
    }

    public void TakeDamage(float damage)
    {
        if (Services == null || Services.Player == null)
        {
            return;
        }

        if (Services.Player.IsDead)
        {
            return;
        }

        Services.Player.TakeDamage(damage);

        if (Services.Player.IsDead && GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        _ui.Refresh();
    }

    public bool IsDead()
    {
        return Services != null && Services.Player != null && Services.Player.IsDead;
    }
}
