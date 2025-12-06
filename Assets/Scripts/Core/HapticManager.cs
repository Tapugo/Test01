using UnityEngine;

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
        }

        /// <summary>
        /// Light haptic feedback for UI interactions like button clicks.
        /// </summary>
        public void LightHaptic()
        {
            if (!hapticsEnabled) return;

#if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(
                    MoreMountains.NiceVibrations.HapticTypes.LightImpact
                );
            }
#endif
        }

        /// <summary>
        /// Medium haptic feedback for actions like dice rolls.
        /// </summary>
        public void MediumHaptic()
        {
            if (!hapticsEnabled) return;

#if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(
                    MoreMountains.NiceVibrations.HapticTypes.MediumImpact
                );
            }
#endif
        }

        /// <summary>
        /// Heavy haptic feedback for major events like jackpots.
        /// </summary>
        public void HeavyHaptic()
        {
            if (!hapticsEnabled) return;

#if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(
                    MoreMountains.NiceVibrations.HapticTypes.HeavyImpact
                );
            }
#endif
        }

        /// <summary>
        /// Success haptic feedback for positive outcomes.
        /// </summary>
        public void SuccessHaptic()
        {
            if (!hapticsEnabled) return;

#if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(
                    MoreMountains.NiceVibrations.HapticTypes.Success
                );
            }
#endif
        }

        /// <summary>
        /// Warning haptic feedback for alerts.
        /// </summary>
        public void WarningHaptic()
        {
            if (!hapticsEnabled) return;

#if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(
                    MoreMountains.NiceVibrations.HapticTypes.Warning
                );
            }
#endif
        }

        /// <summary>
        /// Failure haptic feedback for errors.
        /// </summary>
        public void FailureHaptic()
        {
            if (!hapticsEnabled) return;

#if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(
                    MoreMountains.NiceVibrations.HapticTypes.Failure
                );
            }
#endif
        }

        /// <summary>
        /// Selection haptic feedback for selections/toggles.
        /// </summary>
        public void SelectionHaptic()
        {
            if (!hapticsEnabled) return;

#if UNITY_IOS || UNITY_ANDROID
            if (MoreMountains.NiceVibrations.MMVibrationManager.HapticsSupported())
            {
                MoreMountains.NiceVibrations.MMVibrationManager.Haptic(
                    MoreMountains.NiceVibrations.HapticTypes.Selection
                );
            }
#endif
        }
    }
}
