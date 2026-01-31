using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Stores a series of positions and angles that can be animated and retrieved here.
    /// </summary>
    public class SpriteAnimationNodes : MonoBehaviour
    {
        public static readonly int NUM_NODES = 10;

        // These can't be in an array or they can't be animated :/
        [SerializeField, HideInInspector] Vector2 m_node0 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node1 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node2 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node3 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node4 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node5 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node6 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node7 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node8 = Vector2.zero;
        [SerializeField, HideInInspector] Vector2 m_node9 = Vector2.zero;

        [SerializeField, HideInInspector] float m_ang0 = 0;
        [SerializeField, HideInInspector] float m_ang1 = 0;
        [SerializeField, HideInInspector] float m_ang2 = 0;
        [SerializeField, HideInInspector] float m_ang3 = 0;
        [SerializeField, HideInInspector] float m_ang4 = 0;
        [SerializeField, HideInInspector] float m_ang5 = 0;
        [SerializeField, HideInInspector] float m_ang6 = 0;
        [SerializeField, HideInInspector] float m_ang7 = 0;
        [SerializeField, HideInInspector] float m_ang8 = 0;
        [SerializeField, HideInInspector] float m_ang9 = 0;

        private SpriteRenderer m_spriteRenderer;

        void Awake()
        {
            m_spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Returns the position in world space of the specified node. 
        /// Set ignorePivot if you have "Ignore Pivot" set in animation settings.
        /// </summary>
        public Vector3 GetPosition(int nodeId, bool ignorePivot = false)
        {
            if (m_spriteRenderer == null)
                m_spriteRenderer = GetComponent<SpriteRenderer>();
            if (m_spriteRenderer == null || m_spriteRenderer.sprite == null)
                return Vector2.zero;

            Vector3 result = GetPositionRaw(nodeId);

            // If x or y is a tiny value, set it to zero. Nodes are set to tiny values instead of 0 so the key doesn't get stripped in the editor.
            if (Mathf.Abs(result.x) <= 0.00011f)
                result.x = 0;
            if (Mathf.Abs(result.y) <= 0.00011f)
                result.y = 0;

            // Invert the Y of the node position, account for pivot, and scale by the pixelPerUnit of the sprite
            result.y = -result.y;
            if (ignorePivot)
                result += (Vector3)(m_spriteRenderer.sprite.rect.size * 0.5f - m_spriteRenderer.sprite.pivot);
            result *= (1.0f / m_spriteRenderer.sprite.pixelsPerUnit);

            // Flip the result if necessary
            if (m_spriteRenderer.flipX)
                result.x = -result.x;
            if (m_spriteRenderer.flipY)
                result.y = -result.y;

            // Transform the result into game object's space
            result.Scale(transform.lossyScale);
            result = transform.rotation * result;
            result += transform.position;
            return result;
        }

        /// <summary>
        /// Returns the rotation angle in world space of the specified node
        /// </summary>
        public float GetAngle(int nodeId)
        {
            if (m_spriteRenderer == null)
                m_spriteRenderer = GetComponent<SpriteRenderer>();
            if (m_spriteRenderer == null || m_spriteRenderer.sprite == null)
                return 0;

            float angle = GetAngleRaw(nodeId);

            // Now flip/rotate to desired direction
            // If sprite being flipped doesn't match transform being flipped, then we flip the angle
            if (m_spriteRenderer.flipX != m_spriteRenderer.transform.lossyScale.x < 0)
                angle = 180.0f - angle;
            if (m_spriteRenderer.flipY != m_spriteRenderer.transform.lossyScale.y < 0)
                angle = (180.0f - (angle + 90)) - 90.0f;

            // Apply transform rotation
            angle += transform.eulerAngles.z;
            return angle;
        }

        /// <summary>
        /// Attaches a transform to a specified node. Call from LateUpdate(). 
        /// Handles sprite flipping and scaling automatically.
        /// </summary>
        public void SetTransformFromNode(Transform attachment, int nodeId)
        {
            if (m_spriteRenderer == null)
                m_spriteRenderer = GetComponent<SpriteRenderer>();
            if (m_spriteRenderer == null || m_spriteRenderer.sprite == null)
                return;

            if (attachment == null)
                return;

            attachment.position = GetPosition(nodeId);

            float angle = GetAngleRaw(nodeId);

            // Depending on whether it's a child, and the actual renderer is flipped, we need to set the scale, and flip
            bool flipX = attachment.IsChildOf(transform) ? m_spriteRenderer.flipX : (m_spriteRenderer.flipX != m_spriteRenderer.transform.lossyScale.x < 0);
            bool flipY = attachment.IsChildOf(transform) ? m_spriteRenderer.flipY : (m_spriteRenderer.flipY != m_spriteRenderer.transform.lossyScale.y < 0);

            if (attachment.IsChildOf(transform))
            {
                flipX = m_spriteRenderer.flipX;
                flipY = m_spriteRenderer.flipY;
            }

            if (flipX != attachment.localScale.x < 0)
                attachment.localScale = new Vector3(-attachment.localScale.x, attachment.localScale.y, attachment.localScale.z);
            if (flipY != attachment.localScale.y < 0)
                attachment.localScale = new Vector3(attachment.localScale.x, -attachment.localScale.y, attachment.localScale.z);

            // Apply transform rotation
            angle += transform.eulerAngles.z;

            // Apply the new angle
            attachment.eulerAngles = new Vector3(0, 0, angle);
        }

        /// <summary>
        /// Returns the raw position of a particular node (the position specified in the animation). 
        /// Raw values are unscaled offset from sprite centerpoint.
        /// </summary>
        public Vector2 GetPositionRaw(int nodeId)
        {
            switch (nodeId)
            {
                case 0: return m_node0;
                case 1: return m_node1;
                case 2: return m_node2;
                case 3: return m_node3;
                case 4: return m_node4;
                case 5: return m_node5;
                case 6: return m_node6;
                case 7: return m_node7;
                case 8: return m_node8;
                case 9: return m_node9;
            }
            return Vector2.zero;
        }

        /// <summary>
        /// Returns the raw angle value of a particular node (the angle specified in the animation)
        /// </summary>
        public float GetAngleRaw(int nodeId)
        {
            switch (nodeId)
            {
                case 0: return m_ang0;
                case 1: return m_ang1;
                case 2: return m_ang2;
                case 3: return m_ang3;
                case 4: return m_ang4;
                case 5: return m_ang5;
                case 6: return m_ang6;
                case 7: return m_ang7;
                case 8: return m_ang8;
                case 9: return m_ang9;
            }
            return 0;
        }

        /// <summary>
        /// Called before changing animation to reset nodes to zero
        /// </summary>
        public void Reset()
        {
            m_node0 = Vector2.zero;
            m_node1 = Vector2.zero;
            m_node2 = Vector2.zero;
            m_node3 = Vector2.zero;
            m_node4 = Vector2.zero;
            m_node5 = Vector2.zero;
            m_node6 = Vector2.zero;
            m_node7 = Vector2.zero;
            m_node8 = Vector2.zero;
            m_node9 = Vector2.zero;
            m_ang0 = 0;
            m_ang1 = 0;
            m_ang2 = 0;
            m_ang3 = 0;
            m_ang4 = 0;
            m_ang5 = 0;
            m_ang6 = 0;
            m_ang7 = 0;
            m_ang8 = 0;
            m_ang9 = 0;
        }

        /// <summary>
        /// Sets the raw position of a particular node
        /// </summary>
        public void SetPositionRaw(int nodeId, Vector2 position)
        {
            switch (nodeId)
            {
                case 0: m_node0 = position; break;
                case 1: m_node1 = position; break;
                case 2: m_node2 = position; break;
                case 3: m_node3 = position; break;
                case 4: m_node4 = position; break;
                case 5: m_node5 = position; break;
                case 6: m_node6 = position; break;
                case 7: m_node7 = position; break;
                case 8: m_node8 = position; break;
                case 9: m_node9 = position; break;
            }
        }

        /// <summary>
        /// Sets the raw angle of a particular node
        /// </summary>
        public void SetAngleRaw(int nodeId, float angle)
        {
            switch (nodeId)
            {
                case 0: m_ang0 = angle; break;
                case 1: m_ang1 = angle; break;
                case 2: m_ang2 = angle; break;
                case 3: m_ang3 = angle; break;
                case 4: m_ang4 = angle; break;
                case 5: m_ang5 = angle; break;
                case 6: m_ang6 = angle; break;
                case 7: m_ang7 = angle; break;
                case 8: m_ang8 = angle; break;
                case 9: m_ang9 = angle; break;
            }
        }

        /// <summary>
        /// Resets a specific node to zero position and angle
        /// </summary>
        public void ResetNode(int nodeId)
        {
            SetPositionRaw(nodeId, Vector2.zero);
            SetAngleRaw(nodeId, 0f);
        }

        /// <summary>
        /// Gets the world position of a node (alias for GetPosition)
        /// </summary>
        public Vector3 GetNodeWorldPosition(int nodeId)
        {
            return GetPosition(nodeId);
        }

        /// <summary>
        /// Gets the raw position of a node (alias for GetPositionRaw)
        /// </summary>
        public Vector2 GetNodePosition(int nodeId)
        {
            return GetPositionRaw(nodeId);
        }

        /// <summary>
        /// Sets the raw position of a node (alias for SetPositionRaw)
        /// </summary>
        public void SetNodePosition(int nodeId, Vector2 position)
        {
            SetPositionRaw(nodeId, position);
        }

        /// <summary>
        /// Gets the raw angle of a node (alias for GetAngleRaw)
        /// </summary>
        public float GetNodeAngle(int nodeId)
        {
            return GetAngleRaw(nodeId);
        }

        /// <summary>
        /// Sets the raw angle of a node (alias for SetAngleRaw)
        /// </summary>
        public void SetNodeAngle(int nodeId, float angle)
        {
            SetAngleRaw(nodeId, angle);
        }

        /// <summary>
        /// Resets all nodes (alias for Reset)
        /// </summary>
        public void ResetAllNodes()
        {
            Reset();
        }
    }
}
