using UnityEngine;
#if MOREMOUNTAINS_NICEVIBRATIONS
using Lofelt.NiceVibrations;
#endif

namespace Incredicer.Core
{
    /// <summary>
    /// Centralized haptic feedback manager for mobile devices.
    /// Provides light, medium, and heavy haptic feedback for different game events.
    /// </summary>
    public class HapticManager : MonoBehaviour
    {
        public static HapticManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool hapticsEnabled = true;

        /// <summary>
        /// Whether haptics are enabled.
        /// </summary>
        public bool HapticsEnabled
        {
            get => hapticsEnabled;
            set
            {
                hapticsEnabled = value;
                PlayerPrefs.SetInt("HapticsEnabled", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load haptics preference
            hapticsEnabled = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;

#if MOREMOUNTAINS_NICEVIBRATIONS && !UNITY_EDITOR
            // Initialize haptic controller
            HapticController.Init();
#endif
        }

        /// <summary>
        /// Light haptic feedback for UI interactions like button clicks.
        /// </summary>
        public void LightHaptic()
        {
            if (!hapticsEnabled) return;

#if MOREMOUNTAINS_NICEVIBRATIONS && !UNITY_EDITOR
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
#endif
        }

        /// <summary>
        /// Medium haptic feedback for actions like dice rolls.
        /// </summary>
        public void MediumHaptic()
        {
            if (!hapticsEnabled) return;

#if MOREMOUNTAINS_NICEVIBRATIONS && !UNITY_EDITOR
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
#endif
        }

        /// <summary>
        /// Heavy haptic feedback for major events like jackpots.
        /// </summary>
        public void HeavyHaptic()
        {
            if (!hapticsEnabled) return;

#if MOREMOUNTAINS_NICEVIBRATIONS && !UNITY_EDITOR
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
#endif
        }

        /// <summary>
        /// Success haptic feedback for positive outcomes.
        /// </summary>
        public void SuccessHaptic()
        {
            if (!hapticsEnabled) return;

#if MOREMOUNTAINS_NICEVIBRATIONS && !UNITY_EDITOR
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
#endif
        }

        /// <summary>
        /// Warning haptic feedback for alerts.
        /// </summary>
        public void WarningHaptic()
        {
            if (!hapticsEnabled) return;

#if MOREMOUNTAINS_NICEVIBRATIONS && !UNITY_EDITOR
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
#endif
        }

        /// <summary>
        /// Failure haptic feedback for errors.
        /// </summary>
        public void FailureHaptic()
        {
            if (!hapticsEnabled) return;

#if MOREMOUNTAINS_NICEVIBRATIONS && !UNITY_EDITOR
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
#endif
        }

        /// <summary>
        /// Selection haptic feedback for selections/toggles.
        /// </summary>
        public void SelectionHaptic()
        {
            if (!hapticsEnabled) return;

#if MOREMOUNTAINS_NICEVIBRATIONS && !UNITY_EDITOR
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
#endif
        }
    }
}
