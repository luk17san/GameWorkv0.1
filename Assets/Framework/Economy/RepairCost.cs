namespace Framework.Economy
{
    public static class RepairCost
    {
        public static bool TryCalculate(int currentHealth, int maxHealth, int goldPerHealth,
            out int missingHealth, out int cost)
        {
            missingHealth = cost = 0;
            if (maxHealth < 1 || currentHealth < 0 || currentHealth > maxHealth || goldPerHealth < 1) return false;
            long missing = (long)maxHealth - currentHealth;
            long total = missing * goldPerHealth;
            if (total > int.MaxValue) return false;
            missingHealth = (int)missing; cost = (int)total;
            return true;
        }
    }
}
