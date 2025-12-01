using UnityEngine;
using UnityEditor;

namespace Incredicer.Editor
{
    public static class DeactivateSkillTreePanel
    {
        [MenuItem("Incredicer/Hide Skill Tree Panel")]
        public static void Execute()
        {
            GameObject panel = GameObject.Find("SkillTreePanel");
            if (panel == null)
            {
                // Try finding it even if inactive
                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name == "SkillTreePanel" && obj.scene.IsValid())
                    {
                        panel = obj;
                        break;
                    }
                }
            }

            if (panel != null)
            {
                panel.SetActive(false);
                EditorUtility.SetDirty(panel);
                Debug.Log("[Setup] SkillTreePanel hidden");
            }
            else
            {
                Debug.LogWarning("[Setup] SkillTreePanel not found");
            }
        }
    }
}
