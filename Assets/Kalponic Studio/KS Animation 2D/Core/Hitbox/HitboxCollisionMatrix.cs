using System;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    [CreateAssetMenu(menuName = "KS Animation 2D/Hitbox Collision Matrix", fileName = "HitboxCollisionMatrix")]
    public sealed class HitboxCollisionMatrix : ScriptableObject
    {
        [Serializable]
        public struct Rule
        {
            public HitboxType source;
            public HitboxType target;
            public bool allow;
        }

        [SerializeField] private List<Rule> rules = new List<Rule>();

        public bool Allows(HitboxType source, HitboxType target)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i].source == source && rules[i].target == target)
                {
                    return rules[i].allow;
                }
            }
            // Default: allow
            return true;
        }
    }
}
