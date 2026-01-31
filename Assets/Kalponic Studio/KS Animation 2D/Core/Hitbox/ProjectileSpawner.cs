using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Simple projectile spawner driven by HitboxContact or animation events, using projectileOrigin from hitbox metadata.
    /// </summary>
    public sealed class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float lifetime = 4f;

        public void SpawnFromContact(HitboxContact contact)
        {
            if (projectilePrefab == null) return;
            Vector3 pos = contact.Point;
            if (contact.ProjectileOrigin != Vector2.zero && contact.Source != null)
            {
                // Transform local origin from source into world space
                pos = contact.Source.transform.TransformPoint(contact.ProjectileOrigin);
            }
            Spawn(pos);
        }

        public void Spawn(Vector3 position)
        {
            var go = Instantiate(projectilePrefab, position, Quaternion.identity);
            if (lifetime > 0f)
            {
                Destroy(go, lifetime);
            }
        }
    }
}
