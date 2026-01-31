using System;
using System.Collections.Generic;
using KalponicStudio.Health;

namespace KalponicStudio.Health.Extensions.Persistence
{
    [Serializable]
    public struct HealthSnapshot
    {
        public int MaxHealth;
        public int CurrentHealth;
        public bool IsDowned;
        public bool IsDead;
        public float DownedTimeRemaining;

        public int MaxShield;
        public int CurrentShield;

        public List<StatusEffect> StatusEffects;
    }
}
