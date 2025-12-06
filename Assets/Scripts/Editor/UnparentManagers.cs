using UnityEngine;
using UnityEditor;

namespace Incredicer.Editor
{
    public static class UnparentManagers
    {
        [MenuItem("Incredicer/Fix Manager Hierarchy")]
        public static void Execute()
        {
            // Find the Managers parent
            GameObject managersParent = GameObject.Find("Managers");
            if (managersParent == null)
            {
                Debug.Log("[FixHierarchy] No Managers parent found");
                return;
            }

            // Get all children and unparent them
            int count = 0;
            while (managersParent.transform.childCount > 0)
            {
                Transform child = managersParent.transform.GetChild(0);
                child.SetParent(null);
                count++;
                Debug.Log($"[FixHierarchy] Moved {child.name} to root");
            }

            // Delete the empty Managers parent
            Object.DestroyImmediate(managersParent);

            Debug.Log($"[FixHierarchy] Moved {count} managers to root and deleted Managers parent");
        }
    }
}
