using System;
using System.Collections.Generic;
using UnityEngine;
using Incredicer.Core;
using Incredicer.Dice;

namespace Incredicer.Skills
{
    /// <summary>
    /// Manages active skills that can be triggered by the player.
    /// </summary>
    public class ActiveSkillManager : MonoBehaviour
    {
        public static ActiveSkillManager Instance { get; private set; }

        [Header("Skill Settings")]
        [SerializeField] private float rollBurstCooldown = 30f;
        [SerializeField] private float hyperburstCooldown = 45f;
        [SerializeField] private float hyperburstDuration = 10f;

        // Cooldown tracking
        private Dictionary<ActiveSkillType, float> skillCooldowns = new Dictionary<ActiveSkillType, float>();
        private Dictionary<ActiveSkillType, float> lastUsedTime = new Dictionary<ActiveSkillType, float>();

        // Active skill state
        private bool isSkillActive = false;
        private ActiveSkillType currentActiveSkill = ActiveSkillType.None;

        // Events
        public event Action<ActiveSkillType> OnSkillActivated;
        public event Action<ActiveSkillType> OnSkillCooldownComplete;
        public event Action<ActiveSkillType, float> OnCooldownUpdated;

        // Properties
        public bool IsSkillActive => isSkillActive;
        public ActiveSkillType CurrentActiveSkill => currentActiveSkill;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize cooldowns
            skillCooldowns[ActiveSkillType.RollBurst] = rollBurstCooldown;
            skillCooldowns[ActiveSkillType.Hyperburst] = hyperburstCooldown;

            lastUsedTime[ActiveSkillType.RollBurst] = -rollBurstCooldown; // Ready immediately
            lastUsedTime[ActiveSkillType.Hyperburst] = -hyperburstCooldown;
        }

        private void Update()
        {
            // Check for cooldown completions
            foreach (var skill in skillCooldowns.Keys)
            {
                if (SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsActiveSkillUnlocked(skill))
                {
                    float cooldown = GetEffectiveCooldown(skill);
                    float timeSinceUse = Time.time - lastUsedTime[skill];
                    float remaining = Mathf.Max(0, cooldown - timeSinceUse);

                    OnCooldownUpdated?.Invoke(skill, remaining);
                }
            }
        }

        /// <summary>
        /// Gets the effective cooldown for a skill after applying modifiers.
        /// </summary>
        public float GetEffectiveCooldown(ActiveSkillType skill)
        {
            float baseCooldown = skillCooldowns.ContainsKey(skill) ? skillCooldowns[skill] : 30f;
            float multiplier = GameStats.Instance != null ? (float)GameStats.Instance.SkillCooldownMultiplier : 1f;
            return baseCooldown * multiplier;
        }

        /// <summary>
        /// Checks if a skill is ready to use.
        /// </summary>
        public bool IsSkillReady(ActiveSkillType skill)
        {
            if (skill == ActiveSkillType.None) return false;
            if (SkillTreeManager.Instance == null || !SkillTreeManager.Instance.IsActiveSkillUnlocked(skill)) return false;

            float cooldown = GetEffectiveCooldown(skill);
            float timeSinceUse = Time.time - lastUsedTime[skill];
            return timeSinceUse >= cooldown;
        }

        /// <summary>
        /// Gets the remaining cooldown for a skill.
        /// </summary>
        public float GetRemainingCooldown(ActiveSkillType skill)
        {
            if (!lastUsedTime.ContainsKey(skill)) return 0;

            float cooldown = GetEffectiveCooldown(skill);
            float timeSinceUse = Time.time - lastUsedTime[skill];
            return Mathf.Max(0, cooldown - timeSinceUse);
        }

        /// <summary>
        /// Attempts to activate a skill.
        /// </summary>
        public bool TryActivateSkill(ActiveSkillType skill)
        {
            if (!IsSkillReady(skill))
            {
                Debug.Log($"[ActiveSkillManager] Skill {skill} not ready. Cooldown: {GetRemainingCooldown(skill):F1}s");
                return false;
            }

            switch (skill)
            {
                case ActiveSkillType.RollBurst:
                    ActivateRollBurst();
                    break;
                case ActiveSkillType.Hyperburst:
                    ActivateHyperburst();
                    break;
                default:
                    return false;
            }

            lastUsedTime[skill] = Time.time;
            OnSkillActivated?.Invoke(skill);
            return true;
        }

        /// <summary>
        /// Roll Burst - Rolls all dice at once.
        /// </summary>
        private void ActivateRollBurst()
        {
            Debug.Log("[ActiveSkillManager] Activating Roll Burst!");

            isSkillActive = true;
            currentActiveSkill = ActiveSkillType.RollBurst;

            if (DiceManager.Instance != null)
            {
                var allDice = DiceManager.Instance.GetAllDice();
                int rollCount = 0;

                foreach (var dice in allDice)
                {
                    if (dice != null && dice.CanRoll())
                    {
                        dice.Roll(true, false);
                        rollCount++;
                    }
                }

                Debug.Log($"[ActiveSkillManager] Roll Burst rolled {rollCount} dice!");
            }

            isSkillActive = false;
            currentActiveSkill = ActiveSkillType.None;
        }

        /// <summary>
        /// Hyperburst - Doubles all dice rolls for 10 seconds.
        /// </summary>
        private void ActivateHyperburst()
        {
            Debug.Log("[ActiveSkillManager] Activating Hyperburst! Doubling all rolls for 10 seconds!");

            isSkillActive = true;
            currentActiveSkill = ActiveSkillType.Hyperburst;

            // Enable the hyperburst multiplier in GameStats
            if (GameStats.Instance != null)
            {
                GameStats.Instance.HyperburstActive = true;
            }

            StartCoroutine(HyperburstCoroutine());
        }

        private System.Collections.IEnumerator HyperburstCoroutine()
        {
            // Wait for the hyperburst duration
            yield return new WaitForSeconds(hyperburstDuration);

            // Disable the hyperburst multiplier
            if (GameStats.Instance != null)
            {
                GameStats.Instance.HyperburstActive = false;
            }

            Debug.Log("[ActiveSkillManager] Hyperburst ended!");

            isSkillActive = false;
            currentActiveSkill = ActiveSkillType.None;
        }

        /// <summary>
        /// Gets the best available active skill (prefers Hyperburst if unlocked).
        /// </summary>
        public ActiveSkillType GetBestAvailableSkill()
        {
            if (SkillTreeManager.Instance == null) return ActiveSkillType.None;

            // Prefer Hyperburst if unlocked and ready
            if (SkillTreeManager.Instance.IsActiveSkillUnlocked(ActiveSkillType.Hyperburst) && IsSkillReady(ActiveSkillType.Hyperburst))
            {
                return ActiveSkillType.Hyperburst;
            }

            // Fall back to RollBurst
            if (SkillTreeManager.Instance.IsActiveSkillUnlocked(ActiveSkillType.RollBurst) && IsSkillReady(ActiveSkillType.RollBurst))
            {
                return ActiveSkillType.RollBurst;
            }

            return ActiveSkillType.None;
        }
    }
}
