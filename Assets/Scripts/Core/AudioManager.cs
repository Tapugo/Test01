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

        [Header("Music")]
        [SerializeField] private AudioClip musicClip;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip rollSound;
        [SerializeField] private AudioClip jackpotSound;
        [SerializeField] private AudioClip purchaseSound;
        [SerializeField] private AudioClip skillUnlockSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip prestigeSound;
        [SerializeField] private AudioClip errorSound;

        [Header("Master Volume Settings")]
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private bool sfxEnabled = true;
        [SerializeField] private bool musicEnabled = true;

        [Header("Individual Sound Volumes")]
        [SerializeField, Range(0f, 1f)] private float rollSoundVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float jackpotSoundVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float purchaseSoundVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float skillUnlockSoundVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float buttonClickSoundVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float prestigeSoundVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float errorSoundVolume = 0.8f;

        [Header("Pitch Variation")]
        [SerializeField, Range(0f, 0.3f)] private float rollPitchVariation = 0.15f;

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

            // Assign and play music clip if available
            if (musicClip != null && musicSource.clip == null)
            {
                musicSource.clip = musicClip;
                if (musicEnabled)
                {
                    musicSource.Play();
                }
            }

            // Load sound effects from Resources if not assigned in Inspector
            LoadSoundEffectsFromResources();

            // Try to play music again after loading from resources (in case it wasn't assigned initially)
            if (musicClip != null && musicSource != null && musicSource.clip == null)
            {
                musicSource.clip = musicClip;
                if (musicEnabled)
                {
                    musicSource.Play();
                }
            }
        }

        /// <summary>
        /// Loads sound effects from Resources folder if not assigned in Inspector.
        /// </summary>
        private void LoadSoundEffectsFromResources()
        {
            if (buttonClickSound == null)
            {
                buttonClickSound = Resources.Load<AudioClip>("AudioFiles/click");
                if (buttonClickSound != null)
                    Debug.Log("[AudioManager] Loaded button click sound from Resources");
            }

            if (rollSound == null)
            {
                rollSound = Resources.Load<AudioClip>("AudioFiles/roll");
            }

            if (jackpotSound == null)
            {
                jackpotSound = Resources.Load<AudioClip>("AudioFiles/jackpot");
            }

            if (purchaseSound == null)
            {
                purchaseSound = Resources.Load<AudioClip>("AudioFiles/purchase");
            }

            if (skillUnlockSound == null)
            {
                skillUnlockSound = Resources.Load<AudioClip>("AudioFiles/skill_unlock");
            }

            if (prestigeSound == null)
            {
                prestigeSound = Resources.Load<AudioClip>("AudioFiles/prestige");
            }

            if (errorSound == null)
            {
                errorSound = Resources.Load<AudioClip>("AudioFiles/error");
            }

            if (musicClip == null)
            {
                musicClip = Resources.Load<AudioClip>("AudioFiles/music_loop");
            }
        }

        /// <summary>
        /// Plays the dice roll sound effect with pitch variation.
        /// </summary>
        public void PlayRollSound()
        {
            PlaySfxWithVolume(rollSound, rollSoundVolume, rollPitchVariation);
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
            PlaySfxWithVolume(jackpotSound, jackpotSoundVolume);
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
            PlaySfxWithVolume(purchaseSound, purchaseSoundVolume);
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
            PlaySfxWithVolume(skillUnlockSound, skillUnlockSoundVolume);
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
            PlaySfxWithVolume(buttonClickSound, buttonClickSoundVolume);
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
            PlaySfxWithVolume(prestigeSound, prestigeSoundVolume);
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
            PlaySfxWithVolume(errorSound, errorSoundVolume);
            // Failure haptic for errors
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.FailureHaptic();
            }
        }

        /// <summary>
        /// Plays a sound effect with individual volume and optional pitch variation.
        /// </summary>
        private void PlaySfxWithVolume(AudioClip clip, float individualVolume, float pitchVariation = 0f)
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

            // Combine master SFX volume with individual sound volume
            float finalVolume = sfxVolume * individualVolume;
            sfxSource.PlayOneShot(clip, finalVolume);
        }

        /// <summary>
        /// Plays a sound effect with optional pitch variation (legacy method for compatibility).
        /// </summary>
        public void PlaySfx(AudioClip clip, float pitchVariation = 0f)
        {
            PlaySfxWithVolume(clip, 1f, pitchVariation);
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
