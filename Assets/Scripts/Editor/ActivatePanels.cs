using UnityEngine;
using UnityEditor;

namespace Incredicer.Editor
{
    public static class ActivatePanels
    {
        [MenuItem("Incredicer/Activate SkillTree Panel")]
        public static void ActivateSkillTreePanel()
        {
            var skillTreeUI = Object.FindObjectOfType<UI.SkillTreeUI>(true);
            if (skillTreeUI != null)
            {
                skillTreeUI.gameObject.SetActive(true);
                EditorUtility.SetDirty(skillTreeUI.gameObject);
                Debug.Log("[ActivatePanels] SkillTreePanel activated");
            }
            else
            {
                Debug.LogError("[ActivatePanels] SkillTreeUI not found!");
            }
        }
    }
}
