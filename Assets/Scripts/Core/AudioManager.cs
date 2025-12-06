using UnityEngine;

namespace Incredicer.Core
{
    /// <summary>
    /// Manages game audio including sound effects and music.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip rollSound;
        [SerializeField] private AudioClip jackpotSound;
        [SerializeField] private AudioClip purchaseSound;
        [SerializeField] private AudioClip skillUnlockSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip prestigeSound;
        [SerializeField] private AudioClip errorSound;

        [Header("Volume Settings")]
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private bool sfxEnabled = true;
        [SerializeField] private bool musicEnabled = true;

        // Properties
        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                if (sfxSource != null) sfxSource.volume = sfxVolume;
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                if (musicSource != null) musicSource.volume = musicVolume;
            }
        }

        public bool SfxEnabled
        {
            get => sfxEnabled;
            set => sfxEnabled = value;
        }

        public bool MusicEnabled
        {
            get => musicEnabled;
            set
            {
                musicEnabled = value;
                if (musicSource != null)
                {
                    if (musicEnabled && !musicSource.isPlaying)
                        musicSource.Play();
                    else if (!musicEnabled && musicSource.isPlaying)
                        musicSource.Stop();
                }
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
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            // Create SFX source if not assigned
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFX Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }

            // Create music source if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("Music Source");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
            }
        }

        /// <summary>
        /// Plays the dice roll sound effect.
        /// </summary>
        public void PlayRollSound()
        {
            PlaySfx(rollSound);
            // Medium haptic for dice rolls
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.MediumHaptic();
            }
        }

        /// <summary>
        /// Plays the jackpot sound effect.
        /// </summary>
        public void PlayJackpotSound()
        {
            PlaySfx(jackpotSound);
            // Heavy haptic for jackpots
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.HeavyHaptic();
            }
        }

        /// <summary>
        /// Plays the purchase sound effect.
        /// </summary>
        public void PlayPurchaseSound()
        {
            PlaySfx(purchaseSound);
            // Success haptic for purchases
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.SuccessHaptic();
            }
        }

        /// <summary>
        /// Plays the skill unlock sound effect.
        /// </summary>
        public void PlaySkillUnlockSound()
        {
            PlaySfx(skillUnlockSound);
            // Medium haptic for skill unlocks
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.MediumHaptic();
            }
        }

        /// <summary>
        /// Plays the button click sound effect.
        /// </summary>
        public void PlayButtonClickSound()
        {
            PlaySfx(buttonClickSound);
            // Light haptic for button clicks
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.LightHaptic();
            }
        }

        /// <summary>
        /// Plays the prestige/ascension sound effect.
        /// </summary>
        public void PlayPrestigeSound()
        {
            PlaySfx(prestigeSound);
            // Heavy haptic for prestige (big moment)
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.HeavyHaptic();
            }
        }

        /// <summary>
        /// Plays an error/denied sound effect.
        /// </summary>
        public void PlayErrorSound()
        {
            PlaySfx(errorSound);
            // Failure haptic for errors
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.FailureHaptic();
            }
        }

        /// <summary>
        /// Plays a sound effect with optional pitch variation.
        /// </summary>
        public void PlaySfx(AudioClip clip, float pitchVariation = 0f)
        {
            if (!sfxEnabled || clip == null || sfxSource == null) return;

            if (pitchVariation > 0)
            {
                sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            }
            else
            {
                sfxSource.pitch = 1f;
            }

            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// Plays a sound effect at a specific world position (3D sound).
        /// </summary>
        public void PlaySfxAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (!sfxEnabled || clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume);
        }
    }
}
