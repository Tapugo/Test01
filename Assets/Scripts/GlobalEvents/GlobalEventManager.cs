using System;
using System.Collections.Generic;
using UnityEngine;
using Incredicer.Core;
using Incredicer.Dice;
using Incredicer.Missions;
using Incredicer.Overclock;

namespace Incredicer.GlobalEvents
{
    /// <summary>
    /// Manages global community events with progress bars and tier rewards.
    /// Since this is a single-player game, community progress is simulated.
    /// </summary>
    public class GlobalEventManager : MonoBehaviour
    {
        public static GlobalEventManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private float communityProgressMultiplier = 50f;  // Simulates other players
        [SerializeField] private float progressUpdateInterval = 5f;        // Seconds between community updates

        [Header("Events")]
        [SerializeField] private List<GlobalEventDefinition> availableEvents = new List<GlobalEventDefinition>();

        // Runtime state
        private GlobalEventProgress currentEventProgress;
        private GlobalEventDefinition currentEvent;
        private float timeSinceLastUpdate = 0f;

        // Events
        public event Action<GlobalEventDefinition, GlobalEventProgress> OnEventStarted;
        public event Action<GlobalEventDefinition, GlobalEventProgress> OnEventEnded;
        public event Action<double, double> OnProgressUpdated;  // playerContribution, communityProgress
        public event Action<int, GlobalEventTierReward> OnTierReached;
        public event Action<int, GlobalEventTierReward> OnTierClaimed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadEventDefinitions();
        }

        private void Start()
        {
            // Subscribe to game events
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned += OnDiceSpawned;
                foreach (var dice in DiceManager.Instance.GetAllDice())
                {
                    dice.OnRolled += OnDiceRolled;
                }
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged += OnMoneyChanged;
                CurrencyManager.Instance.OnDarkMatterChanged += OnDarkMatterChanged;
            }

            if (OverclockManager.Instance != null)
            {
                OverclockManager.Instance.OnDiceDestroyed += OnDiceDestroyed;
            }

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionCompleted += OnMissionCompleted;
            }
        }

        private void OnDestroy()
        {
            if (DiceManager.Instance != null)
            {
                DiceManager.Instance.OnDiceSpawned -= OnDiceSpawned;
                foreach (var dice in DiceManager.Instance.GetAllDice())
                {
                    dice.OnRolled -= OnDiceRolled;
                }
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnMoneyChanged -= OnMoneyChanged;
                CurrencyManager.Instance.OnDarkMatterChanged -= OnDarkMatterChanged;
            }

            if (OverclockManager.Instance != null)
            {
                OverclockManager.Instance.OnDiceDestroyed -= OnDiceDestroyed;
            }

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionCompleted -= OnMissionCompleted;
            }
        }

        private void Update()
        {
            if (currentEvent == null || currentEventProgress == null || !currentEventProgress.isActive)
                return;

            // Check if event has expired
            DateTime endTime = currentEventProgress.GetStartTime().AddHours(currentEvent.durationHours);
            if (DateTime.UtcNow >= endTime)
            {
                EndCurrentEvent();
                return;
            }

            // Simulate community progress over time
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= progressUpdateInterval)
            {
                timeSinceLastUpdate = 0f;
                SimulateCommunityProgress();
            }
        }

        private void LoadEventDefinitions()
        {
            var loaded = Resources.LoadAll<GlobalEventDefinition>("GlobalEvents");
            availableEvents = new List<GlobalEventDefinition>(loaded);
            Debug.Log($"[GlobalEventManager] Loaded {availableEvents.Count} event definitions");
        }

        #region Event Lifecycle

        /// <summary>
        /// Starts a new global event.
        /// </summary>
        public void StartEvent(GlobalEventDefinition eventDef)
        {
            if (eventDef == null) return;

            // End current event if one is active
            if (currentEvent != null && currentEventProgress != null && currentEventProgress.isActive)
            {
                EndCurrentEvent();
            }

            currentEvent = eventDef;
            currentEventProgress = new GlobalEventProgress(eventDef.eventId);

            Debug.Log($"[GlobalEventManager] Started event: {eventDef.eventName}");
            OnEventStarted?.Invoke(currentEvent, currentEventProgress);
        }

        /// <summary>
        /// Starts a random event from available events.
        /// </summary>
        public void StartRandomEvent()
        {
            if (availableEvents.Count == 0)
            {
                Debug.LogWarning("[GlobalEventManager] No events available to start");
                return;
            }

            int index = UnityEngine.Random.Range(0, availableEvents.Count);
            StartEvent(availableEvents[index]);
        }

        private void EndCurrentEvent()
        {
            if (currentEvent == null || currentEventProgress == null) return;

            currentEventProgress.isActive = false;
            Debug.Log($"[GlobalEventManager] Event ended: {currentEvent.eventName}");
            OnEventEnded?.Invoke(currentEvent, currentEventProgress);
        }

        #endregion

        #region Contribution Tracking

        private void OnDiceSpawned(Dice.Dice dice)
        {
            dice.OnRolled += OnDiceRolled;
        }

        private void OnDiceRolled(Dice.Dice dice, double moneyEarned, bool isJackpot)
        {
            if (currentEvent == null || !currentEventProgress.isActive) return;

            if (currentEvent.contributionType == GlobalEventContributionType.DiceRolls)
            {
                AddContribution(1);
            }
            else if (currentEvent.contributionType == GlobalEventContributionType.JackpotsHit && isJackpot)
            {
                AddContribution(1);
            }
        }

        private double lastMoney = 0;
        private void OnMoneyChanged(double newMoney)
        {
            if (currentEvent == null || !currentEventProgress.isActive) return;

            if (currentEvent.contributionType == GlobalEventContributionType.MoneyEarned)
            {
                double earned = newMoney - lastMoney;
                if (earned > 0)
                {
                    AddContribution(earned);
                }
            }
            lastMoney = newMoney;
        }

        private double lastDarkMatter = 0;
        private void OnDarkMatterChanged(double newDM)
        {
            if (currentEvent == null || !currentEventProgress.isActive) return;

            if (currentEvent.contributionType == GlobalEventContributionType.DarkMatterEarned)
            {
                double earned = newDM - lastDarkMatter;
                if (earned > 0)
                {
                    AddContribution(earned);
                }
            }
            lastDarkMatter = newDM;
        }

        private void OnDiceDestroyed(Dice.Dice dice, double dmEarned)
        {
            if (currentEvent == null || !currentEventProgress.isActive) return;

            if (currentEvent.contributionType == GlobalEventContributionType.DiceDestroyed)
            {
                AddContribution(1);
            }
        }

        private void OnMissionCompleted(MissionInstance mission)
        {
            if (currentEvent == null || !currentEventProgress.isActive) return;

            if (currentEvent.contributionType == GlobalEventContributionType.MissionsCompleted)
            {
                AddContribution(1);
            }
        }

        private void AddContribution(double amount)
        {
            if (currentEventProgress == null || !currentEventProgress.isActive) return;

            currentEventProgress.playerContribution += amount;

            // Simulate community contribution (other players)
            currentEventProgress.communityProgress += amount * communityProgressMultiplier;

            // Check for tier unlocks
            CheckTierProgress();

            OnProgressUpdated?.Invoke(currentEventProgress.playerContribution, currentEventProgress.communityProgress);
        }

        private void SimulateCommunityProgress()
        {
            if (currentEventProgress == null || !currentEventProgress.isActive) return;

            // Add some random community progress
            double randomProgress = UnityEngine.Random.Range(10f, 100f) * communityProgressMultiplier;
            currentEventProgress.communityProgress += randomProgress;

            CheckTierProgress();
            OnProgressUpdated?.Invoke(currentEventProgress.playerContribution, currentEventProgress.communityProgress);
        }

        #endregion

        #region Tier Management

        private void CheckTierProgress()
        {
            if (currentEvent == null || currentEventProgress == null) return;

            float progress = GetCommunityProgressNormalized();

            for (int i = 0; i < currentEvent.tierRewards.Length; i++)
            {
                var tier = currentEvent.tierRewards[i];

                // Check if tier just became available (not yet claimed)
                if (progress >= tier.progressThreshold && i > currentEventProgress.highestTierClaimed)
                {
                    OnTierReached?.Invoke(i, tier);
                }
            }
        }

        /// <summary>
        /// Claims rewards for a specific tier.
        /// </summary>
        public bool ClaimTierReward(int tierIndex)
        {
            if (currentEvent == null || currentEventProgress == null) return false;
            if (tierIndex < 0 || tierIndex >= currentEvent.tierRewards.Length) return false;

            var tier = currentEvent.tierRewards[tierIndex];

            // Check if tier is unlocked
            float progress = GetCommunityProgressNormalized();
            if (progress < tier.progressThreshold) return false;

            // Check if already claimed
            if (tierIndex <= currentEventProgress.highestTierClaimed) return false;

            // Grant rewards
            if (tier.timeShardsReward > 0)
            {
                CurrencyManager.Instance?.AddTimeShards(tier.timeShardsReward);
            }
            if (tier.darkMatterReward > 0)
            {
                CurrencyManager.Instance?.AddDarkMatter(tier.darkMatterReward);
            }

            // Mark as claimed
            currentEventProgress.highestTierClaimed = tierIndex;

            Debug.Log($"[GlobalEventManager] Claimed tier {tierIndex} reward: {tier.tierName}");
            OnTierClaimed?.Invoke(tierIndex, tier);

            // Play effects
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySkillUnlockSound();
            }
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.SpawnSparkleEffect(Vector3.zero);
            }

            return true;
        }

        /// <summary>
        /// Gets the next tier that can be claimed.
        /// </summary>
        public int GetNextClaimableTier()
        {
            if (currentEvent == null || currentEventProgress == null) return -1;

            float progress = GetCommunityProgressNormalized();
            int nextTier = currentEventProgress.highestTierClaimed + 1;

            if (nextTier < currentEvent.tierRewards.Length &&
                progress >= currentEvent.tierRewards[nextTier].progressThreshold)
            {
                return nextTier;
            }

            return -1;
        }

        #endregion

        #region Queries

        public bool HasActiveEvent()
        {
            return currentEvent != null && currentEventProgress != null && currentEventProgress.isActive;
        }

        public GlobalEventDefinition GetCurrentEvent()
        {
            return currentEvent;
        }

        public GlobalEventProgress GetCurrentProgress()
        {
            return currentEventProgress;
        }

        public float GetCommunityProgressNormalized()
        {
            if (currentEvent == null || currentEventProgress == null) return 0f;
            return Mathf.Clamp01((float)(currentEventProgress.communityProgress / currentEvent.communityGoal));
        }

        public float GetPlayerContributionPercent()
        {
            if (currentEventProgress == null || currentEventProgress.communityProgress <= 0) return 0f;
            return Mathf.Clamp01((float)(currentEventProgress.playerContribution / currentEventProgress.communityProgress));
        }

        public TimeSpan GetTimeRemaining()
        {
            if (currentEvent == null || currentEventProgress == null) return TimeSpan.Zero;

            DateTime endTime = currentEventProgress.GetStartTime().AddHours(currentEvent.durationHours);
            TimeSpan remaining = endTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public List<GlobalEventDefinition> GetAvailableEvents()
        {
            return new List<GlobalEventDefinition>(availableEvents);
        }

        #endregion

        #region Save/Load

        public GlobalEventSaveData GetSaveData()
        {
            return new GlobalEventSaveData
            {
                currentEventId = currentEvent?.eventId ?? "",
                eventProgress = currentEventProgress,
                lastMoney = lastMoney,
                lastDarkMatter = lastDarkMatter
            };
        }

        public void LoadSaveData(GlobalEventSaveData data)
        {
            if (data == null) return;

            lastMoney = data.lastMoney;
            lastDarkMatter = data.lastDarkMatter;

            if (!string.IsNullOrEmpty(data.currentEventId) && data.eventProgress != null)
            {
                // Find the event definition
                currentEvent = availableEvents.Find(e => e.eventId == data.currentEventId);
                if (currentEvent != null)
                {
                    currentEventProgress = data.eventProgress;

                    // Check if event has expired
                    DateTime endTime = currentEventProgress.GetStartTime().AddHours(currentEvent.durationHours);
                    if (DateTime.UtcNow >= endTime)
                    {
                        currentEventProgress.isActive = false;
                    }
                }
            }

            Debug.Log($"[GlobalEventManager] Loaded event data. Active: {HasActiveEvent()}");
        }

        #endregion
    }

    /// <summary>
    /// Save data for global events.
    /// </summary>
    [Serializable]
    public class GlobalEventSaveData
    {
        public string currentEventId;
        public GlobalEventProgress eventProgress;
        public double lastMoney;
        public double lastDarkMatter;
    }
}
