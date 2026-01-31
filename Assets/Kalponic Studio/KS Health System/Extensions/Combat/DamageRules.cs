using UnityEngine;

namespace KalponicStudio.Health.Extensions.Combat
{
    public static class DamageRules
    {
        public static bool IsFriendly(DamageSourceInfo attacker, DamageSourceInfo target)
        {
            if (!attacker.IsValid || !target.IsValid) return false;

            if (attacker.TeamId != 0 && target.TeamId != 0 && attacker.TeamId == target.TeamId)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(attacker.FactionId) &&
                !string.IsNullOrWhiteSpace(target.FactionId) &&
                attacker.FactionId == target.FactionId)
            {
                return true;
            }

            return false;
        }

        public static bool CanDamage(DamageSourceInfo attacker, DamageSourceInfo target)
        {
            if (!attacker.IsValid || !target.IsValid) return true;

            if (IsFriendly(attacker, target))
            {
                return attacker.AllowFriendlyFire || target.AllowFriendlyFire;
            }

            return true;
        }

        public static DamageSourceInfo FromGameObject(GameObject obj)
        {
            if (obj == null) return default;

            TeamComponent team = obj.GetComponent<TeamComponent>();
            if (team == null) return default;

            return team.ToDamageSource();
        }
    }
}
