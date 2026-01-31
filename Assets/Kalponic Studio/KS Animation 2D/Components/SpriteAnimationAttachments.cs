using UnityEngine;

namespace KalponicStudio
{
    /// <summary>
    /// Component that attaches child game objects to corresponding Animation Nodes
    /// The index in the list of attachments corresponds to the Animation node it'll be attached to
    /// </summary>
    [RequireComponent(typeof(SpriteAnimationNodes))]
    public class SpriteAnimationAttachments : MonoBehaviour
    {
        [Tooltip("Add the name of child game object to attach to each SpriteAnimation Node")]
        public Transform[] m_attachments = null;

        // A reference to the SpriteAnimationNodes component on this object
        private SpriteAnimationNodes m_nodes;

        // We use LateUpdate to ensure the animation has updated before updating attachment positions
        void LateUpdate()
        {
            // Cache the SpriteAnimationNodes component for efficiency
            if (m_nodes == null)
                m_nodes = GetComponent<SpriteAnimationNodes>();

            // Loop through the list of attachment names, and attach to the corresponding node's index
            for (int i = 0; i < m_attachments.Length; ++i)
            {
                if (m_attachments[i] != null)
                {
                    m_nodes.SetTransformFromNode(m_attachments[i], i);
                }
            }
        }

        /// <summary>
        /// Get the attachment transform for a specific node
        /// </summary>
        public Transform GetAttachment(int nodeId)
        {
            if (nodeId >= 0 && nodeId < m_attachments.Length)
                return m_attachments[nodeId];
            return null;
        }

        /// <summary>
        /// Set the attachment transform for a specific node
        /// </summary>
        public void SetAttachment(int nodeId, Transform attachment)
        {
            if (nodeId >= 0 && nodeId < m_attachments.Length)
            {
                m_attachments[nodeId] = attachment;
            }
            else
            {
                KSAnimLog.Warn($"SpriteAnimationAttachments: Node ID {nodeId} is out of range. Max nodes: {m_attachments.Length}", "Playback", this);
            }
        }

        /// <summary>
        /// Add a new attachment slot
        /// </summary>
        public void AddAttachment(Transform attachment)
        {
            var newArray = new Transform[m_attachments.Length + 1];
            m_attachments.CopyTo(newArray, 0);
            newArray[m_attachments.Length] = attachment;
            m_attachments = newArray;
        }

        /// <summary>
        /// Remove an attachment by transform reference
        /// </summary>
        public void RemoveAttachment(Transform attachment)
        {
            var list = new System.Collections.Generic.List<Transform>(m_attachments);
            list.Remove(attachment);
            m_attachments = list.ToArray();
        }

        /// <summary>
        /// Clear all attachments
        /// </summary>
        public void ClearAttachments()
        {
            m_attachments = new Transform[0];
        }
    }
}
