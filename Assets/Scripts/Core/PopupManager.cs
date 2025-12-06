using UnityEngine;
using System.Collections.Generic;

namespace Incredicer.Core
{
    /// <summary>
    /// Manages popup state across the game.
    /// Used to prevent interactions (like dice tapping) when popups are open.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }

        // Track which popups are currently open
        private HashSet<string> openPopups = new HashSet<string>();

        /// <summary>
        /// Returns true if any popup is currently open.
        /// </summary>
        public bool IsAnyPopupOpen => openPopups.Count > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Clear any stale popup registrations from previous session
            openPopups.Clear();
        }

        /// <summary>
        /// Registers a popup as open. Call this when showing a popup.
        /// </summary>
        /// <param name="popupId">Unique identifier for the popup (usually class name).</param>
        public void RegisterPopupOpen(string popupId)
        {
            openPopups.Add(popupId);
        }

        /// <summary>
        /// Registers a popup as closed. Call this when hiding a popup.
        /// </summary>
        /// <param name="popupId">Unique identifier for the popup (usually class name).</param>
        public void RegisterPopupClosed(string popupId)
        {
            openPopups.Remove(popupId);
        }

        /// <summary>
        /// Checks if a specific popup is open.
        /// </summary>
        public bool IsPopupOpen(string popupId)
        {
            return openPopups.Contains(popupId);
        }

        /// <summary>
        /// Clears all popup registrations (useful for scene transitions).
        /// </summary>
        public void ClearAll()
        {
            openPopups.Clear();
        }
    }
}
