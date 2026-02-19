using UnityEngine;

public class ManagersBootstrapper : MonoBehaviour
{
    void Awake()
    {
        if (DatabaseManager.Instance == null)
        {
            GameObject managersPrefab = Resources.Load<GameObject>("Managers");
            if (managersPrefab != null)
                Instantiate(managersPrefab);
            else
            {
                // Create manually if prefab not found
                GameObject managers = new GameObject("Managers");
                managers.AddComponent<DatabaseManager>();
                managers.AddComponent<SaveLoadManager>();
                managers.AddComponent<SceneController>();
            }
        }
    }
}