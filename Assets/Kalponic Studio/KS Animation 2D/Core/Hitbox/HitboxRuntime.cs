using System;
using System.Collections.Generic;
using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Contact payload produced when two hitboxes overlap.
    /// </summary>
    public struct HitboxContact
    {
        public HitboxManager Source;
        public HitboxManager Target;
        public HitboxType SourceType;
        public HitboxType TargetType;
        public Vector2 Point;
        public float Damage;
        public Vector2 Knockback;
        public float Hitstun;
        public string FxId;
        public Vector2 ProjectileOrigin;

        public HitboxContact(HitboxManager source, HitboxManager target, HitboxType sourceType, HitboxType targetType, Vector2 point, float damage, Vector2 knockback, float hitstun, string fxId, Vector2 projectileOrigin)
        {
            Source = source;
            Target = target;
            SourceType = sourceType;
            TargetType = targetType;
            Point = point;
            Damage = damage;
            Knockback = knockback;
            Hitstun = hitstun;
            FxId = fxId;
            ProjectileOrigin = projectileOrigin;
        }
    }

    /// <summary>
    /// Runtime manager that spawns hitbox colliders based on the current animation frame.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayableAnimatorComponent))]
    public sealed class HitboxManager : MonoBehaviour
    {
        [Tooltip("Optional override; if empty the component searches locally.")]
        [SerializeField] private PlayableAnimatorComponent animatorComponent;
        [Tooltip("Physics layer for generated hitboxes.")]
        [SerializeField] private int hitboxLayer = 0;
        [Tooltip("Optional collision matrix to filter hitbox interactions.")]
        [SerializeField] private HitboxCollisionMatrix collisionMatrix;
        [Tooltip("Reduce updates for performance. 0 = every frame; >0 = seconds between syncs.")]
        [SerializeField] private float updateInterval = 0f;
        [Tooltip("Disable hitboxes when beyond this distance from the main camera. 0 = always on.")]
        [SerializeField] private float cullDistance = 0f;

        private readonly List<HitboxProxy> proxies = new List<HitboxProxy>();
        private Transform proxyRoot;
        private float nextUpdateTime;

        public event Action<HitboxContact> Contact;

        private void Awake()
        {
            if (animatorComponent == null)
            {
                animatorComponent = GetComponent<PlayableAnimatorComponent>();
            }
            proxyRoot = new GameObject("Hitboxes").transform;
            proxyRoot.SetParent(transform, false);
        }

        private void Update()
        {
            if (updateInterval > 0f && Time.time < nextUpdateTime) return;
            if (cullDistance > 0f)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    float dist = Vector3.Distance(transform.position, cam.transform.position);
                    if (dist > cullDistance)
                    {
                        SetProxyCount(0);
                        nextUpdateTime = Time.time + updateInterval;
                        return;
                    }
                }
            }

            SyncHitboxes();
            if (updateInterval > 0f) nextUpdateTime = Time.time + updateInterval;
        }

        private void SyncHitboxes()
        {
            if (animatorComponent == null || animatorComponent.CharacterType == null)
            {
                SetProxyCount(0);
                return;
            }

            var state = animatorComponent.CharacterType.GetState(animatorComponent.CurrentState);
            if (state == null || state.frameHitboxes == null || state.sprites == null || state.sprites.Length == 0)
            {
                SetProxyCount(0);
                return;
            }

            int frameIndex = GetFrameIndex(state, animatorComponent.NormalizedTime);
            if (frameIndex < 0 || frameIndex >= state.frameHitboxes.Count)
            {
                SetProxyCount(0);
                return;
            }

            var frameSet = state.frameHitboxes[frameIndex];
            int count = frameSet?.hitboxes?.Count ?? 0;
            SetProxyCount(count);

            for (int i = 0; i < count; i++)
            {
                var hb = frameSet.hitboxes[i];
                var proxy = proxies[i];
                proxy.Type = hb.type;
                proxy.Manager = this;
                proxy.Collider.size = hb.size;
                proxy.Collider.offset = hb.offset;
                proxy.MetadataDamage = hb.damage;
                proxy.MetadataKnockback = hb.knockback;
                proxy.MetadataHitstun = hb.hitstun;
                proxy.gameObject.SetActive(true);
            }
        }

        private int GetFrameIndex(AnimationStateSO state, float normalizedTime)
        {
            if (state == null || state.sprites == null || state.sprites.Length == 0) return 0;
            normalizedTime = Mathf.Repeat(normalizedTime, 1f);

            if (state.frameDurations != null && state.frameDurations.Count == state.sprites.Length)
            {
                float total = 0f;
                foreach (var d in state.frameDurations) total += Mathf.Max(0.0001f, d);
                float t = normalizedTime * total;
                float accum = 0f;
                for (int i = 0; i < state.frameDurations.Count; i++)
                {
                    accum += Mathf.Max(0.0001f, state.frameDurations[i]);
                    if (t <= accum) return i;
                }
                return state.frameDurations.Count - 1;
            }

            // Fallback to FPS
            float frameDuration = state.frameRate > 0f ? 1f / state.frameRate : 0.1f;
            int index = Mathf.FloorToInt(normalizedTime * state.sprites.Length);
            return Mathf.Clamp(index, 0, state.sprites.Length - 1);
        }

        private void SetProxyCount(int count)
        {
            // Disable extra
            for (int i = count; i < proxies.Count; i++)
            {
                proxies[i].gameObject.SetActive(false);
            }

            // Create missing
            while (proxies.Count < count)
            {
                var go = new GameObject($"Hitbox_{proxies.Count}");
                go.layer = hitboxLayer;
                go.transform.SetParent(proxyRoot, false);
                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                var proxy = go.AddComponent<HitboxProxy>();
                proxy.Collider = col;
                proxies.Add(proxy);
            }
        }

        internal void RaiseContact(HitboxProxy a, HitboxProxy b, Vector2 point)
        {
            if (a == null || b == null) return;
            if (a.Manager == b.Manager) return;
            if (collisionMatrix != null && !collisionMatrix.Allows(a.Type, b.Type)) return;
            Contact?.Invoke(new HitboxContact(this, b.Manager, a.Type, b.Type, point, a.MetadataDamage, a.MetadataKnockback, a.MetadataHitstun, a.MetadataFxId, a.MetadataProjectileOrigin));
        }
    }

    /// <summary>
    /// Attached to generated hitbox colliders to relay contacts.
    /// </summary>
    public sealed class HitboxProxy : MonoBehaviour
    {
        public HitboxManager Manager;
        public HitboxType Type;
        public BoxCollider2D Collider;
        public float MetadataDamage;
        public Vector2 MetadataKnockback;
        public float MetadataHitstun;
        public string MetadataFxId;
        public Vector2 MetadataProjectileOrigin;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var otherProxy = other.GetComponent<HitboxProxy>();
            if (otherProxy != null && Manager != null)
            {
                Manager.RaiseContact(this, otherProxy, other.ClosestPoint(transform.position));
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            OnTriggerEnter2D(other);
        }
    }
}
