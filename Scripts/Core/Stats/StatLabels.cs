namespace IdleDefense.Core.Stats
{
    /// <summary>Short UI labels for stats (extend when adding new <see cref="StatId"/> values).</summary>
    public static class StatLabels
    {
        public static string ToShortLabel(StatId id)
        {
            switch (id)
            {
                case StatId.Attack:
                    return "ATK";
                case StatId.MaxHp:
                    return "HP";
                case StatId.AttackSpeed:
                    return "ASPD";
                case StatId.AttackRange:
                    return "RANGE";
                case StatId.CritChance:
                    return "CRIT%";
                case StatId.CritMultiplier:
                    return "CRIT×";
                case StatId.LifeSteal:
                    return "LS%";
                default:
                    return id.ToString();
            }
        }
    }
}
