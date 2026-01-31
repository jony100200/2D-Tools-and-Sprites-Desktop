using UnityEngine;

namespace KalponicStudio.Health.Extensions.Combat
{
    public class TeamComponent : MonoBehaviour
    {
        [Header("Team")]
        [SerializeField] private int teamId = 0;
        [SerializeField] private string factionId = "";
        [SerializeField] private bool allowFriendlyFire = false;

        public int TeamId => teamId;
        public string FactionId => factionId;
        public bool AllowFriendlyFire => allowFriendlyFire;

        public DamageSourceInfo ToDamageSource(string sourceTag = null)
        {
            return new DamageSourceInfo
            {
                Source = gameObject,
                TeamId = teamId,
                FactionId = factionId,
                AllowFriendlyFire = allowFriendlyFire,
                SourceTag = string.IsNullOrWhiteSpace(sourceTag) ? gameObject.tag : sourceTag
            };
        }
    }
}
