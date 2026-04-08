using IdleDefense.GameServices.Config.Scaling;
using IdleDefense.GameServices.Config.Wave;
using IdleDefense.GameServices.CoreServices.Combat;
using IdleDefense.GameServices.CoreServices.Inventory;
using IdleDefense.GameServices.CoreServices.Player;
using IdleDefense.GameServices.Systems.EventBus;
using IdleDefense.GameServices.Systems.Loot;
using IdleDefense.Core.Random;
using IdleDefense.Presentation;
using IdleDefense.Core.Stats;
using UnityEngine;

namespace IdleDefense.GameServices.Presentation
{
    /// <summary>
    /// Scene bootstrap: wires CoreServices + Systems + Config without DI framework.
    /// Assign ScriptableObjects here to replace hardcoded tuning values.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class GameServices : MonoBehaviour
    {
        public static GameServices Instance { get; private set; }

        [Header("Optional ScriptableObject configs")]
        public PlayerBaseStatsSO PlayerBaseStats;
        public LootTableSO LootTable;
        public EnemyScalingConfigSO EnemyScaling;
        public WaveScalingConfigSO WaveProgression;

        [Header("Runtime (inspector debug)")]
        public GameEventBus Events = new GameEventBus();
        public CombatService Combat = new CombatService();
        public InventoryState Inventory;
        public PlayerState Player;
        public LootDropService Loot;
        public EnemyScalingDefinition ScalingDefinition;
        public WaveProgressionDefinition WaveProgressionDefinition;

        private IRandomSource _random;

        private void Awake()
        {
            Instance = this;

            _random = new UnityRandomSource();

            ScalingDefinition = EnemyScaling != null ? EnemyScaling.ToDefinition() : new EnemyScalingDefinition();
            WaveProgressionDefinition = WaveProgression != null ? WaveProgression.ToDefinition() : new WaveProgressionDefinition();

            LootTableDefinition lootDef = LootTable != null ? LootTable.ToDefinition() : new LootTableDefinition();
            Loot = new LootDropService(_random, lootDef);

            EquipScoringWeights equipWeights = lootDef.ToEquipScoringWeights();
            Inventory = new InventoryState(equipWeights);

            StatBlock baseStats = PlayerBaseStats != null ? PlayerBaseStats.ToStatBlock() : CreateDefaultPlayerBaseStats();
            Player = new PlayerState(Combat, Inventory, baseStats);
        }

        private static StatBlock CreateDefaultPlayerBaseStats()
        {
            StatBlock block = new StatBlock();
            block.Set(StatId.MaxHp, 100f);
            block.Set(StatId.Attack, 10f);
            block.Set(StatId.AttackSpeed, 1f);
            block.Set(StatId.AttackRange, 4f);
            return block;
        }

        private void OnDestroy()
        {
            Events?.ClearAllSubscriptions();
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
