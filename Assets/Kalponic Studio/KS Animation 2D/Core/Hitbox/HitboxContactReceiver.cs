using UnityEngine;
using UnityEngine.Events;

namespace KalponicStudio
{
    /// <summary>
    /// Listens to a HitboxManager and triggers UnityEvents / optional audio/VFX on contacts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitboxContactReceiver : MonoBehaviour
    {
        [Tooltip("HitboxManager to listen to. If empty, searches on this GameObject.")]
        [SerializeField] private HitboxManager manager;
        [Tooltip("Optional source filter. Leave empty to accept any.")]
        [SerializeField] private HitboxType? sourceTypeFilter = null;
        [Tooltip("Optional target filter. Leave empty to accept any.")]
        [SerializeField] private HitboxType? targetTypeFilter = null;
        [Tooltip("Optional collision matrix; if assigned, contacts not allowed are ignored here.")]
        [SerializeField] private HitboxCollisionMatrix collisionMatrix;

        [Header("Events")]
        public UnityEvent<HitboxContact> OnContact = new UnityEvent<HitboxContact>();

        [Header("Audio/VFX")]
        [SerializeField] private AudioClip contactClip;
        [SerializeField] private GameObject contactVfxPrefab;
        [SerializeField] private float vfxLifetime = 2f;

        private AudioSource audioSource;

        private void Awake()
        {
            if (manager == null)
            {
                manager = GetComponent<HitboxManager>();
            }
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && contactClip != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void OnEnable()
        {
            if (manager != null)
            {
                manager.Contact += HandleContact;
            }
        }

        private void OnDisable()
        {
            if (manager != null)
            {
                manager.Contact -= HandleContact;
            }
        }

        private void HandleContact(HitboxContact contact)
        {
            if (sourceTypeFilter.HasValue && contact.SourceType != sourceTypeFilter.Value) return;
            if (targetTypeFilter.HasValue && contact.TargetType != targetTypeFilter.Value) return;
            if (collisionMatrix != null && !collisionMatrix.Allows(contact.SourceType, contact.TargetType)) return;

            OnContact?.Invoke(contact);

            if (contactClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(contactClip);
            }

            if (contactVfxPrefab != null)
            {
                var vfx = Instantiate(contactVfxPrefab, contact.Point, Quaternion.identity);
                if (vfxLifetime > 0f) Destroy(vfx, vfxLifetime);
            }
        }
    }
}
