using System;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Utility to rotate through a combo sequence using <see cref="ComboChain"/> timing rules.
    /// </summary>
    public sealed class ComboStateSelector
    {
        private readonly ComboChain combo;
        private readonly AnimationId[] sequence;

        public ComboStateSelector(AnimationId[] sequence, int maxCombo = 0, float resetTimeSeconds = 0.8f)
        {
            this.sequence = sequence ?? Array.Empty<AnimationId>();
            combo = new ComboChain(Mathf.Max(sequence?.Length ?? 1, maxCombo), resetTimeSeconds);
        }

        public AnimationId Next()
        {
            if (sequence.Length == 0)
            {
                return default;
            }

            int index = combo.Advance();
            return sequence[index % sequence.Length];
        }

        public void Reset() => combo.Reset();
    }

    /// <summary>
    /// Helper for buffered inputs when driving state machines (e.g., buffered jumps/attacks).
    /// </summary>
    public sealed class BufferedStateSelector
    {
        private readonly InputBuffer buffer;
        private readonly float moveDeadZoneSqr;

        public BufferedStateSelector(float bufferSeconds = 0.12f, float moveDeadZone = 0.01f)
        {
            buffer = new InputBuffer(bufferSeconds);
            moveDeadZoneSqr = moveDeadZone * moveDeadZone;
        }

        public void Record() => buffer.Record();

        public bool TryConsume(AnimationId id, out AnimationId selected)
        {
            if (buffer.Consume())
            {
                selected = id;
                return true;
            }

            selected = default;
            return false;
        }

        public bool HasBufferedInput => buffer.IsBuffered();

        public bool HasMovement(Vector2 moveInput) => moveInput.sqrMagnitude > moveDeadZoneSqr;
    }

    /// <summary>
    /// Priority-based selector that picks the first satisfied condition by <see cref="AnimationStatePriority"/>.
    /// </summary>
    public sealed class AnimationStateSelector
    {
        private readonly List<Rule> rules = new List<Rule>();

        public void AddRule(AnimationId id, Func<bool> condition, AnimationStatePriority priority = AnimationStatePriority.Medium)
        {
            rules.Add(new Rule
            {
                Id = id,
                Condition = condition ?? (() => false),
                Priority = priority
            });
        }

        public bool TrySelect(out AnimationId id)
        {
            AnimationStatePriority bestPriority = AnimationStatePriority.Low;
            id = default;
            bool found = false;

            foreach (var rule in rules)
            {
                if (!rule.Condition())
                {
                    continue;
                }

                if (!found || rule.Priority > bestPriority)
                {
                    found = true;
                    bestPriority = rule.Priority;
                    id = rule.Id;
                }
            }

            return found;
        }

        private struct Rule
        {
            public AnimationId Id;
            public Func<bool> Condition;
            public AnimationStatePriority Priority;
        }
    }
}
