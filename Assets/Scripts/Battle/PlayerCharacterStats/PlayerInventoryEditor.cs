using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PlayerInventoryEditor : EditorWindow
{
    private PlayerInventory inventory;
    private CharacterStats testCharacter;
    private ShopItem itemToAdd;

    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "Items (Battle)", "Equipment (After Battle)" };

    private bool showDebugSection = true;

    [MenuItem("Tools/Player Inventory Editor")]
    public static void ShowWindow()
    {
        PlayerInventoryEditor window = GetWindow<PlayerInventoryEditor>("Inventory");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    private void OnEnable()
    {
        FindInventory();
    }

    private void FindInventory()
    {
        inventory = Object.FindObjectOfType<PlayerInventory>();

        if (inventory == null && !Application.isPlaying)
        {
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                inventory = obj.GetComponentInChildren<PlayerInventory>();
                if (inventory != null) break;
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 18;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("PLAYER INVENTORY", titleStyle);

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Inventory Reference", EditorStyles.boldLabel);

        PlayerInventory newInventory = (PlayerInventory)EditorGUILayout.ObjectField("Player Inventory", inventory, typeof(PlayerInventory), true);

        if (newInventory != inventory)
        {
            inventory = newInventory;
        }

        if (inventory == null)
        {
            EditorGUILayout.HelpBox("No PlayerInventory found in scene. Create one or enter Play Mode.", MessageType.Warning);

            if (GUILayout.Button("Create Player Inventory GameObject", GUILayout.Height(30)))
            {
                CreateInventoryObject();
            }

            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        DrawGoldSection();
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        Color originalColor = GUI.backgroundColor;

        for (int i = 0; i < tabNames.Length; i++)
        {
            GUI.backgroundColor = selectedTab == i ? Color.cyan : Color.white;
            if (GUILayout.Button(tabNames[i], GUILayout.Height(40)))
            {
                selectedTab = i;
            }
        }

        GUI.backgroundColor = originalColor;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (selectedTab == 0)
        {
            DrawItemsTab();
        }
        else if (selectedTab == 1)
        {
            DrawEquipmentTab();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);

        DrawDebugSection();

        if (Application.isPlaying)
        {
            Repaint();
        }

        if (GUI.changed && inventory != null)
        {
            EditorUtility.SetDirty(inventory);
        }
    }

    private void DrawGoldSection()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        GUIStyle goldStyle = new GUIStyle(EditorStyles.boldLabel);
        goldStyle.fontSize = 20;
        goldStyle.normal.textColor = Color.yellow;

        EditorGUILayout.LabelField("Gold:", EditorStyles.boldLabel, GUILayout.Width(50));
        EditorGUILayout.LabelField(inventory.gold.ToString() + " G", goldStyle);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawItemsTab()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ITEMS (Usable in Battle)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Count: " + inventory.GetItemCount(), GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("These items can be used during battles.", MessageType.Info);

        EditorGUILayout.Space(5);

        if (inventory.items.Count == 0)
        {
            EditorGUILayout.HelpBox("No items in inventory.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < inventory.items.Count; i++)
            {
                InventorySlot slot = inventory.items[i];
                if (slot.item == null) continue;

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                if (slot.item.itemIcon != null)
                {
                    Rect iconRect = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(48), GUILayout.Height(48));
                    EditorGUI.DrawPreviewTexture(iconRect, slot.item.itemIcon.texture);
                }

                EditorGUILayout.BeginVertical();

                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.fontSize = 14;
                EditorGUILayout.LabelField(slot.item.itemName, nameStyle);
                EditorGUILayout.LabelField(slot.item.description, EditorStyles.wordWrappedLabel);

                if (slot.item.hpRestore > 0)
                    EditorGUILayout.LabelField("• HP +" + slot.item.hpRestore, EditorStyles.miniLabel);
                if (slot.item.bitPointsRestore > 0)
                    EditorGUILayout.LabelField("• Bit Points +" + slot.item.bitPointsRestore, EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Use", GUILayout.Height(25)))
                    {
                        if (testCharacter != null)
                        {
                            inventory.UseItem(slot.item, testCharacter);
                        }
                        else
                        {
                            Debug.LogWarning("Assign a test character in Debug section to use items!");
                        }
                    }
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(25)))
                {
                    inventory.RemoveItem(slot.item, false);
                }
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Select Asset", GUILayout.Width(100), GUILayout.Height(25)))
                {
                    Selection.activeObject = slot.item;
                    EditorGUIUtility.PingObject(slot.item);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawEquipmentTab()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("EQUIPMENT (Use After Battle)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Count: " + inventory.GetEquipmentCount(), GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Equipment can only be equipped/unequipped outside of battles.", MessageType.Info);

        EditorGUILayout.Space(5);

        if (inventory.equippedItem != null)
        {
            EditorGUILayout.BeginVertical("box");

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);

            EditorGUILayout.LabelField("CURRENTLY EQUIPPED", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (inventory.equippedItem.itemIcon != null)
            {
                Rect iconRect = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(48), GUILayout.Height(48));
                EditorGUI.DrawPreviewTexture(iconRect, inventory.equippedItem.itemIcon.texture);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(inventory.equippedItem.itemName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(inventory.equippedItem.description, EditorStyles.wordWrappedLabel);

            if (inventory.equippedItem.attackBoost != 0)
                EditorGUILayout.LabelField("• ATK " + (inventory.equippedItem.attackBoost > 0 ? "+" : "") + inventory.equippedItem.attackBoost);
            if (inventory.equippedItem.defenseBoost != 0)
                EditorGUILayout.LabelField("• DEF " + (inventory.equippedItem.defenseBoost > 0 ? "+" : "") + inventory.equippedItem.defenseBoost);
            if (inventory.equippedItem.hpBoost != 0)
                EditorGUILayout.LabelField("• HP " + (inventory.equippedItem.hpBoost > 0 ? "+" : "") + inventory.equippedItem.hpBoost);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Unequip", GUILayout.Height(25)))
            {
                inventory.UnequipItem();
            }

            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if (inventory.equipment.Count == 0)
        {
            EditorGUILayout.HelpBox("No equipment in inventory.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < inventory.equipment.Count; i++)
            {
                InventorySlot slot = inventory.equipment[i];
                if (slot.item == null) continue;

                Color originalColor = GUI.backgroundColor;
                if (slot.isEquipped)
                {
                    GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
                }

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                if (slot.item.itemIcon != null)
                {
                    Rect iconRect = GUILayoutUtility.GetRect(48, 48, GUILayout.Width(48), GUILayout.Height(48));
                    EditorGUI.DrawPreviewTexture(iconRect, slot.item.itemIcon.texture);
                }

                EditorGUILayout.BeginVertical();

                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.fontSize = 14;
                EditorGUILayout.LabelField(slot.item.itemName + (slot.isEquipped ? " [EQUIPPED]" : ""), nameStyle);
                EditorGUILayout.LabelField(slot.item.description, EditorStyles.wordWrappedLabel);

                if (slot.item.attackBoost != 0)
                    EditorGUILayout.LabelField("• ATK " + (slot.item.attackBoost > 0 ? "+" : "") + slot.item.attackBoost, EditorStyles.miniLabel);
                if (slot.item.defenseBoost != 0)
                    EditorGUILayout.LabelField("• DEF " + (slot.item.defenseBoost > 0 ? "+" : "") + slot.item.defenseBoost, EditorStyles.miniLabel);
                if (slot.item.hpBoost != 0)
                    EditorGUILayout.LabelField("• HP " + (slot.item.hpBoost > 0 ? "+" : "") + slot.item.hpBoost, EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (!slot.isEquipped)
                {
                    if (GUILayout.Button("Equip", GUILayout.Height(25)))
                    {
                        inventory.EquipItem(slot.item);
                    }
                }
                else
                {
                    if (GUILayout.Button("Unequip", GUILayout.Height(25)))
                    {
                        inventory.UnequipItem();
                    }
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove", GUILayout.Width(70), GUILayout.Height(25)))
                {
                    if (slot.isEquipped)
                    {
                        inventory.UnequipItem();
                    }
                    inventory.RemoveItem(slot.item, true);
                }
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Select Asset", GUILayout.Width(100), GUILayout.Height(25)))
                {
                    Selection.activeObject = slot.item;
                    EditorGUIUtility.PingObject(slot.item);
                }

                EditorGUILayout.EndHorizontal();

                GUI.backgroundColor = originalColor;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawDebugSection()
    {
        EditorGUILayout.BeginVertical("box");

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);

        showDebugSection = EditorGUILayout.Foldout(showDebugSection, "DEBUG TOOLS", true, EditorStyles.foldoutHeader);

        if (showDebugSection)
        {
            GUI.backgroundColor = originalColor;
            EditorGUILayout.Space(5);

            testCharacter = (CharacterStats)EditorGUILayout.ObjectField("Test Character (for using items)", testCharacter, typeof(CharacterStats), false);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Quick Add Item", EditorStyles.boldLabel);
            itemToAdd = (ShopItem)EditorGUILayout.ObjectField("Item", itemToAdd, typeof(ShopItem), false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Item", GUILayout.Height(25)))
            {
                if (itemToAdd != null)
                {
                    inventory.AddItem(itemToAdd);
                    Debug.Log("Added " + itemToAdd.itemName);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Gold Controls", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+100 G"))
            {
                inventory.AddGold(100);
            }
            if (GUILayout.Button("+1000 G"))
            {
                inventory.AddGold(1000);
            }
            if (GUILayout.Button("Reset Gold"))
            {
                inventory.gold = 0;
                EditorUtility.SetDirty(inventory);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(1f, 0.5f, 0f);
            if (GUILayout.Button("Clear All Inventory", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Inventory",
                    "Are you sure you want to clear all items and equipment?",
                    "Clear", "Cancel"))
                {
                    inventory.ClearInventory();
                }
            }
        }

        GUI.backgroundColor = originalColor;
        EditorGUILayout.EndVertical();
    }

    private void CreateInventoryObject()
    {
        GameObject inventoryObj = new GameObject("PlayerInventory");
        inventoryObj.AddComponent<PlayerInventory>();
        inventory = inventoryObj.GetComponent<PlayerInventory>();

        Selection.activeGameObject = inventoryObj;

        Debug.Log("Created PlayerInventory GameObject in scene");
    }
}