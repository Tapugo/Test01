using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Incredicer.Core;

namespace Incredicer.Skills
{
    /// <summary>
    /// Manages the skill tree: unlocking nodes, tracking progress, applying effects.
    /// </summary>
    public class SkillTreeManager : MonoBehaviour
    {
        public static SkillTreeManager Instance { get; private set; }

        [Header("Skill Data")]
        [SerializeField] private List<SkillNodeData> allSkillNodes = new List<SkillNodeData>();

        [Header("State")]
        [SerializeField] private HashSet<SkillNodeId> unlockedNodes = new HashSet<SkillNodeId>();
        [SerializeField] private HashSet<ActiveSkillType> unlockedActiveSkills = new HashSet<ActiveSkillType>();

        // Dictionary for quick lookup
        private Dictionary<SkillNodeId, SkillNodeData> nodeDataLookup;

        // Events
        public event Action<SkillNodeId> OnSkillUnlocked;
        public event Action<ActiveSkillType> OnActiveSkillUnlocked;
        public event Action OnSkillTreeReset;

        // Properties
        public IReadOnlyCollection<SkillNodeId> UnlockedNodes => unlockedNodes;
        public IReadOnlyCollection<ActiveSkillType> UnlockedActiveSkills => unlockedActiveSkills;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildLookupDictionary();
        }

        private void BuildLookupDictionary()
        {
            nodeDataLookup = new Dictionary<SkillNodeId, SkillNodeData>();
            foreach (var node in allSkillNodes)
            {
                if (node != null && !nodeDataLookup.ContainsKey(node.nodeId))
                {
                    nodeDataLookup[node.nodeId] = node;
                }
            }
        }

        /// <summary>
        /// Gets the skill node data for a specific node ID.
        /// </summary>
        public SkillNodeData GetNodeData(SkillNodeId nodeId)
        {
            if (nodeDataLookup == null) BuildLookupDictionary();
            return nodeDataLookup.TryGetValue(nodeId, out var data) ? data : null;
        }

        /// <summary>
        /// Checks if a skill node is unlocked.
        /// </summary>
        public bool IsNodeUnlocked(SkillNodeId nodeId)
        {
            return unlockedNodes.Contains(nodeId);
        }

        /// <summary>
        /// Checks if all prerequisites for a node are met.
        /// </summary>
        public bool ArePrerequisitesMet(SkillNodeId nodeId)
        {
            var data = GetNodeData(nodeId);
            if (data == null) return false;

            // Core node has no prerequisites
            if (data.prerequisites.Count == 0) return true;

            return data.prerequisites.All(prereq => IsNodeUnlocked(prereq));
        }

        /// <summary>
        /// Checks if a node can be purchased (prerequisites met, not already owned, can afford).
        /// </summary>
        public bool CanPurchaseNode(SkillNodeId nodeId)
        {
            if (IsNodeUnlocked(nodeId)) return false;
            if (!ArePrerequisitesMet(nodeId)) return false;

            var data = GetNodeData(nodeId);
            if (data == null) return false;

            if (CurrencyManager.Instance == null) return false;
            return CurrencyManager.Instance.DarkMatter >= data.darkMatterCost;
        }

        /// <summary>
        /// Attempts to purchase and unlock a skill node.
        /// </summary>
        public bool TryPurchaseNode(SkillNodeId nodeId)
        {
            if (!CanPurchaseNode(nodeId)) return false;

            var data = GetNodeData(nodeId);
            if (data == null) return false;

            // Spend dark matter
            if (!CurrencyManager.Instance.SpendDarkMatter(data.darkMatterCost))
            {
                return false;
            }

            // Unlock the node
            UnlockNode(nodeId);
            return true;
        }

        /// <summary>
        /// Unlocks a skill node and applies its effects.
        /// </summary>
        public void UnlockNode(SkillNodeId nodeId)
        {
            if (unlockedNodes.Contains(nodeId)) return;

            unlockedNodes.Add(nodeId);

            var data = GetNodeData(nodeId);
            if (data != null)
            {
                data.ApplyEffects();
                Debug.Log($"[SkillTree] Unlocked: {data.displayName}");
            }

            OnSkillUnlocked?.Invoke(nodeId);
        }

        /// <summary>
        /// Unlocks an active skill for use.
        /// </summary>
        public void UnlockActiveSkill(ActiveSkillType skill)
        {
            if (skill == ActiveSkillType.None) return;
            if (unlockedActiveSkills.Contains(skill)) return;

            unlockedActiveSkills.Add(skill);
            Debug.Log($"[SkillTree] Unlocked active skill: {skill}");
            OnActiveSkillUnlocked?.Invoke(skill);
        }

        /// <summary>
        /// Checks if an active skill is unlocked.
        /// </summary>
        public bool IsActiveSkillUnlocked(ActiveSkillType skill)
        {
            return unlockedActiveSkills.Contains(skill);
        }

        /// <summary>
        /// Gets all skill nodes in a specific branch.
        /// </summary>
        public List<SkillNodeData> GetNodesInBranch(SkillBranch branch)
        {
            return allSkillNodes.Where(n => n != null && n.branch == branch).ToList();
        }

        /// <summary>
        /// Gets all unlocked skill nodes.
        /// </summary>
        public List<SkillNodeData> GetUnlockedNodeData()
        {
            return unlockedNodes
                .Select(id => GetNodeData(id))
                .Where(data => data != null)
                .ToList();
        }

        /// <summary>
        /// Gets the total number of skill points (unlocked nodes).
        /// </summary>
        public int GetTotalSkillPoints()
        {
            return unlockedNodes.Count;
        }

        /// <summary>
        /// Resets the skill tree (used during prestige/ascension).
        /// Refunds dark matter based on settings.
        /// </summary>
        public void ResetSkillTree(bool refundDarkMatter = false)
        {
            if (refundDarkMatter && CurrencyManager.Instance != null)
            {
                // Calculate total dark matter spent
                double totalSpent = 0;
                foreach (var nodeId in unlockedNodes)
                {
                    var data = GetNodeData(nodeId);
                    if (data != null)
                    {
                        totalSpent += data.darkMatterCost;
                    }
                }
                CurrencyManager.Instance.AddDarkMatter(totalSpent);
            }

            // Remove all effects
            foreach (var nodeId in unlockedNodes)
            {
                var data = GetNodeData(nodeId);
                if (data != null)
                {
                    data.RemoveEffects();
                }
            }

            // Clear state
            unlockedNodes.Clear();
            unlockedActiveSkills.Clear();

            // Reset game stats to defaults
            if (GameStats.Instance != null)
            {
                GameStats.Instance.ResetToDefaults();
            }

            OnSkillTreeReset?.Invoke();
            Debug.Log("[SkillTree] Reset complete");
        }

        /// <summary>
        /// Reapplies all effects from unlocked nodes (used after loading).
        /// </summary>
        public void ReapplyAllEffects()
        {
            // Reset stats first
            if (GameStats.Instance != null)
            {
                GameStats.Instance.ResetToDefaults();
            }

            // Apply all unlocked node effects
            foreach (var nodeId in unlockedNodes)
            {
                var data = GetNodeData(nodeId);
                if (data != null)
                {
                    data.ApplyEffects();
                }
            }

            Debug.Log($"[SkillTree] Reapplied effects from {unlockedNodes.Count} nodes");
        }

        /// <summary>
        /// Sets the unlocked nodes (used for save/load).
        /// </summary>
        public void SetUnlockedNodes(HashSet<SkillNodeId> nodes)
        {
            unlockedNodes = new HashSet<SkillNodeId>(nodes);
        }

        /// <summary>
        /// Sets the unlocked active skills (used for save/load).
        /// </summary>
        public void SetUnlockedActiveSkills(HashSet<ActiveSkillType> skills)
        {
            unlockedActiveSkills = new HashSet<ActiveSkillType>(skills);
        }

        /// <summary>
        /// Gets the unlocked nodes as a list for serialization.
        /// </summary>
        public List<SkillNodeId> GetUnlockedNodesList()
        {
            return new List<SkillNodeId>(unlockedNodes);
        }

        /// <summary>
        /// Gets the unlocked active skills as a list for serialization.
        /// </summary>
        public List<ActiveSkillType> GetUnlockedActiveSkillsList()
        {
            return new List<ActiveSkillType>(unlockedActiveSkills);
        }
    }
}
