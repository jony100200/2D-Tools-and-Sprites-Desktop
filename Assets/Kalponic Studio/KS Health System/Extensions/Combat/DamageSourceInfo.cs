using System;
using UnityEngine;
using KalponicStudio.Health;

namespace KalponicStudio.Health.Extensions.Combat
{
    [Serializable]
    public struct DamageSourceInfo
    {
        public GameObject Source;
        public int TeamId;
        public string FactionId;
        public bool AllowFriendlyFire;
        public string SourceTag;

        public bool IsValid => Source != null;

        public void ApplyTo(ref DamageInfo damageInfo)
        {
            damageInfo.Source = Source;
            if (!string.IsNullOrWhiteSpace(SourceTag))
            {
                damageInfo.SourceTag = SourceTag;
            }
        }
    }
}
