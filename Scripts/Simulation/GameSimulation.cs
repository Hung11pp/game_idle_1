using System;
using IdleDefense.Core.Math;
using IdleDefense.Core.Random;
using IdleDefense.GameServices.CoreServices.Inventory;
using IdleDefense.GameServices.Systems.EventBus;
using IdleDefense.GameServices.Systems.Loot;

namespace IdleDefense.Simulation
{
    /// <summary>
    /// Headless gameplay loop: waves → enemy deaths → <see cref="EnemyDiedEvent"/> → loot (same path as <c>LootSystem</c>).
    /// Call from tests, Editor menu, or a tiny console driver — no scene required.
    /// </summary>
    public sealed class GameSimulation
    {
        public void Run()
        {
            var services = new GameServicesFake();

            for (int wave = 1; wave <= 20; wave++)
            {
                Console.WriteLine($"Wave {wave}");

                for (int i = 0; i < 10 + wave; i++)
                {
                    SimulateEnemyDeath(services, wave);
                }
            }

            services.Dispose();
        }

        private static void SimulateEnemyDeath(GameServicesFake services, int wave)
        {
            var pos = new Position3(0f, 0f, 0f);
            services.Events.EnemyDied.Publish(new EnemyDiedEvent(pos, wave % 10 == 0, wave));
        }
    }

    /// <summary>
    /// Non-Unity stand-in for <c>GameServices</c>: event bus + seeded RNG + loot + inventory.
    /// </summary>
    public sealed class GameServicesFake : IDisposable
    {
        private readonly LootTableDefinition _lootTableDef;

        public GameEventBus Events { get; } = new GameEventBus();
        public LootDropService Loot { get; }
        public InventoryState Inventory { get; }

        public GameServicesFake(int randomSeed = 42)
        {
            _lootTableDef = new LootTableDefinition();
            var rng = new SeededRandomSource(randomSeed);
            Loot = new LootDropService(rng, _lootTableDef);
            Inventory = new InventoryState(_lootTableDef.ToEquipScoringWeights());

            Events.EnemyDied.OnEvent += OnEnemyDied;
            Events.LootDropped.OnEvent += OnLootDropped;
        }

        private void OnEnemyDied(EnemyDiedEvent e)
        {
            Loot.TryDropLoot(e.IsBoss, e.WaveNumber, Inventory, Events);
        }

        private void OnLootDropped(LootDroppedEvent e)
        {
            if (e.Item == null)
            {
                return;
            }

            string equipNote = e.WasEquipped ? "" : " (not equipped)";
            Console.WriteLine("  Loot: " + e.Item.GetSummary() + equipNote);
        }

        public void Dispose()
        {
            Events.EnemyDied.OnEvent -= OnEnemyDied;
            Events.LootDropped.OnEvent -= OnLootDropped;
            Events.ClearAllSubscriptions();
        }

        private sealed class SeededRandomSource : IRandomSource
        {
            private readonly Random _rng;

            public SeededRandomSource(int seed)
            {
                _rng = new Random(seed);
            }

            public float NextFloat()
            {
                return (float)_rng.NextDouble();
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                return _rng.Next(minInclusive, maxExclusive);
            }
        }
    }
}
