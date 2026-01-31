using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// ScriptableObject defining a character type (Chris, SkeletonArcher, etc.)
    /// Contains animation mappings and character-specific properties
    /// </summary>
    [CreateAssetMenu(menuName = "KS Animation 2D/Character Type", fileName = "NewCharacterType")]
    public class CharacterTypeSO : ScriptableObject
    {
        [Header("Character Identity")]
        public string characterName = "Hero";
        public string description = "Character type description";

        [Header("Animation Mappings")]
        public AnimationStateSO idleState;
        public AnimationStateSO walkState;
        public AnimationStateSO runState;
        public AnimationStateSO jumpState;
        public AnimationStateSO fallState;
        public AnimationStateSO attackState;
        public AnimationStateSO hurtState;
        public AnimationStateSO deathState;

        [Header("Additional States")]
        public List<AnimationStateSO> customStates = new List<AnimationStateSO>();

        [Header("Character Properties")]
        public float moveSpeed = 5f;
        public float jumpForce = 10f;
        public int maxHealth = 100;

        // Quick access to all states
        public List<AnimationStateSO> AllStates
        {
            get
            {
                var states = new List<AnimationStateSO>();
                if (idleState) states.Add(idleState);
                if (walkState) states.Add(walkState);
                if (runState) states.Add(runState);
                if (jumpState) states.Add(jumpState);
                if (fallState) states.Add(fallState);
                if (attackState) states.Add(attackState);
                if (hurtState) states.Add(hurtState);
                if (deathState) states.Add(deathState);
                states.AddRange(customStates);
                return states;
            }
        }

        // Get state by name
        public AnimationStateSO GetState(string stateName)
        {
            return AllStates.Find(s => s.stateName == stateName);
        }

        // Validation
        public bool IsValid
        {
            get
            {
                // At minimum, need idle and walk states
                return idleState != null && walkState != null &&
                       idleState.IsValid && walkState.IsValid;
            }
        }
    }
}
