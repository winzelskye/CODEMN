using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShopItemsEditor : EditorWindow
{
    private List<ShopItem> shopItems = new List<ShopItem>();
    private ShopItem selectedItem;
    private int selectedItemIndex = -1;

    private Vector2 scrollPosition;
    private Vector2 itemListScroll;
    private bool showBasicInfo = true;
    private bool showShopSettings = true;
    private bool showConsumableEffects = true;
    private bool showEquipmentStats = true;
    private bool showVisuals = true;

    private string searchQuery = "";
    private ItemCategory filterCategory = ItemCategory.Consumable;
    private bool useCategoryFilter = false;

    [MenuItem("Tools/Shop Items Editor")]
    public static void ShowWindow()
    {
        ShopItemsEditor window = GetWindow<ShopItemsEditor>("Shop Items");
        window.minSize = new Vector2(600, 600);
        window.Show();
    }

    private void OnEnable()
    {
        LoadAllItems();
    }

    private void LoadAllItems()
    {
        shopItems.Clear();
        string[] guids = AssetDatabase.FindAssets("t:ShopItem");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopItem item = AssetDatabase.LoadAssetAtPath<ShopItem>(path);
            if (item != null)
            {
                shopItems.Add(item);
            }
        }

        if (shopItems.Count > 0 && selectedItem == null)
        {
            selectedItem = shopItems[0];
            selectedItemIndex = 0;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        // Title
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 18;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("SHOP ITEMS EDITOR", titleStyle);

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        // Left Panel - Item List
        DrawItemList();

        // Right Panel - Item Editor
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width - 250));

        if (selectedItem != null)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawItemEditor();
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Select an item from the list or create a new one.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        // Save Changes
        if (GUI.changed && selectedItem != null)
        {
            EditorUtility.SetDirty(selectedItem);
            AssetDatabase.SaveAssets();
        }
    }

    private void DrawItemList()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(230));

        EditorGUILayout.LabelField("Shop Items", EditorStyles.boldLabel);

        // Create New Item Button
        if (GUILayout.Button("+ Create New Item", GUILayout.Height(30)))
        {
            CreateNewItem();
        }

        EditorGUILayout.Space(5);

        // Search Bar
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
        searchQuery = EditorGUILayout.TextField(searchQuery);
        EditorGUILayout.EndHorizontal();

        // Filter by Category
        EditorGUILayout.BeginHorizontal();
        useCategoryFilter = EditorGUILayout.Toggle("Filter:", useCategoryFilter, GUILayout.Width(55));
        GUI.enabled = useCategoryFilter;
        filterCategory = (ItemCategory)EditorGUILayout.EnumPopup(filterCategory);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Item List
        itemListScroll = EditorGUILayout.BeginScrollView(itemListScroll, GUILayout.Height(position.height - 220));

        for (int i = 0; i < shopItems.Count; i++)
        {
            if (shopItems[i] == null) continue;

            // Apply search filter
            if (!string.IsNullOrEmpty(searchQuery) &&
                !shopItems[i].itemName.ToLower().Contains(searchQuery.ToLower()))
            {
                continue;
            }

            // Apply category filter
            if (useCategoryFilter && shopItems[i].category != filterCategory)
            {
                continue;
            }

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = selectedItemIndex == i ? Color.cyan : Color.white;

            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.BeginVertical();

            if (GUILayout.Button(shopItems[i].itemName, GUILayout.Height(40)))
            {
                selectedItem = shopItems[i];
                selectedItemIndex = i;
            }

            // Show category and price
            GUIStyle smallStyle = new GUIStyle(EditorStyles.miniLabel);
            smallStyle.normal.textColor = Color.gray;
            string info = shopItems[i].category + " - " + shopItems[i].price + "G";
            if (shopItems[i].spawnChance < 100f)
            {
                info += " (" + shopItems[i].spawnChance + "%)";
            }
            EditorGUILayout.LabelField(info, smallStyle);

            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Delete Item",
                    "Are you sure you want to delete " + shopItems[i].itemName + "?",
                    "Delete", "Cancel"))
                {
                    DeleteItem(i);
                }
            }

            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // Stats
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Total Items: " + shopItems.Count, EditorStyles.miniLabel);

        // Refresh Button
        if (GUILayout.Button("Refresh List"))
        {
            LoadAllItems();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawItemEditor()
    {
        // Item Preview Header
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        // Icon preview
        if (selectedItem.itemIcon != null)
        {
            Rect iconRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));
            EditorGUI.DrawPreviewTexture(iconRect, selectedItem.itemIcon.texture);
        }
        else
        {
            Rect iconRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));
            EditorGUI.DrawRect(iconRect, Color.gray);
            GUIStyle centerStyle = new GUIStyle(EditorStyles.label);
            centerStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(iconRect, "No Icon", centerStyle);
        }

        EditorGUILayout.BeginVertical();

        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.fontSize = 16;
        EditorGUILayout.LabelField(selectedItem.itemName, nameStyle);

        EditorGUILayout.LabelField("Category: " + selectedItem.category);
        EditorGUILayout.LabelField("Price: " + selectedItem.price + " G");

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        // Basic Info
        DrawBasicInfoSection();
        EditorGUILayout.Space(5);

        // Shop Settings
        DrawShopSettingsSection();
        EditorGUILayout.Space(5);

        // Category-specific sections
        if (selectedItem.category == ItemCategory.Consumable)
        {
            DrawConsumableEffectsSection();
        }
        else if (selectedItem.category == ItemCategory.Equipment)
        {
            DrawEquipmentStatsSection();
        }

        EditorGUILayout.Space(5);

        // Visuals
        DrawVisualsSection();
    }

    private void DrawBasicInfoSection()
    {
        EditorGUILayout.BeginVertical("box");
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "BASIC INFO", true, EditorStyles.foldoutHeader);

        if (showBasicInfo)
        {
            EditorGUILayout.Space(5);

            selectedItem.itemName = EditorGUILayout.TextField("Item Name", selectedItem.itemName);

            EditorGUILayout.LabelField("Description");
            selectedItem.description = EditorGUILayout.TextArea(selectedItem.description, GUILayout.Height(60));

            selectedItem.itemIcon = (Sprite)EditorGUILayout.ObjectField("Icon", selectedItem.itemIcon, typeof(Sprite), false);

            EditorGUI.BeginChangeCheck();
            ItemCategory newCategory = (ItemCategory)EditorGUILayout.EnumPopup("Category", selectedItem.category);
            if (EditorGUI.EndChangeCheck())
            {
                selectedItem.category = newCategory;
                // Clear opposite category's fields when switching
                if (newCategory == ItemCategory.Consumable)
                {
                    selectedItem.attackBoost = 0;
                    selectedItem.defenseBoost = 0;
                    selectedItem.hpBoost = 0;
                    selectedItem.equipmentEffect = "";
                }
                else
                {
                    selectedItem.hpRestore = 0;
                    selectedItem.bitPointsRestore = 0;
                    selectedItem.consumableEffect = "";
                }
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawShopSettingsSection()
    {
        EditorGUILayout.BeginVertical("box");

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.8f, 0f);
        showShopSettings = EditorGUILayout.Foldout(showShopSettings, "SHOP SETTINGS", true, EditorStyles.foldoutHeader);
        GUI.backgroundColor = originalColor;

        if (showShopSettings)
        {
            EditorGUILayout.Space(5);

            selectedItem.price = EditorGUILayout.IntField("Price (G)", selectedItem.price);

            EditorGUILayout.Space(3);

            selectedItem.spawnChance = EditorGUILayout.Slider("Spawn Chance (%)", selectedItem.spawnChance, 0f, 100f);

            // Visual indicator
            Rect spawnBarRect = GUILayoutUtility.GetRect(18, 20, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(spawnBarRect, selectedItem.spawnChance / 100f, selectedItem.spawnChance.ToString("F1") + "%");

            if (selectedItem.spawnChance < 100f)
            {
                EditorGUILayout.HelpBox("This item has a " + selectedItem.spawnChance + "% chance to appear in the shop.", MessageType.Info);
            }

            if (selectedItem.spawnChance == 0f)
            {
                EditorGUILayout.HelpBox("This item will never appear in the shop!", MessageType.Warning);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawConsumableEffectsSection()
    {
        EditorGUILayout.BeginVertical("box");

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        showConsumableEffects = EditorGUILayout.Foldout(showConsumableEffects, "CONSUMABLE EFFECTS", true, EditorStyles.foldoutHeader);
        GUI.backgroundColor = originalColor;

        if (showConsumableEffects)
        {
            EditorGUILayout.Space(5);

            selectedItem.hpRestore = EditorGUILayout.IntField("HP Restore", selectedItem.hpRestore);
            selectedItem.bitPointsRestore = EditorGUILayout.IntField("Bit Points Restore", selectedItem.bitPointsRestore);

            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("Special Effect");
            selectedItem.consumableEffect = EditorGUILayout.TextArea(selectedItem.consumableEffect, GUILayout.Height(40));

            // Preview what it does
            if (selectedItem.hpRestore > 0 || selectedItem.bitPointsRestore > 0 || !string.IsNullOrEmpty(selectedItem.consumableEffect))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Effect Summary:", EditorStyles.boldLabel);

                if (selectedItem.hpRestore > 0)
                    EditorGUILayout.LabelField("• Restores " + selectedItem.hpRestore + " HP");

                if (selectedItem.bitPointsRestore > 0)
                    EditorGUILayout.LabelField("• Restores " + selectedItem.bitPointsRestore + " Bit Points");

                if (!string.IsNullOrEmpty(selectedItem.consumableEffect))
                    EditorGUILayout.LabelField("• " + selectedItem.consumableEffect);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawEquipmentStatsSection()
    {
        EditorGUILayout.BeginVertical("box");

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.7f, 0.5f, 1f);
        showEquipmentStats = EditorGUILayout.Foldout(showEquipmentStats, "EQUIPMENT STATS", true, EditorStyles.foldoutHeader);
        GUI.backgroundColor = originalColor;

        if (showEquipmentStats)
        {
            EditorGUILayout.Space(5);

            selectedItem.attackBoost = EditorGUILayout.IntField("Attack Boost", selectedItem.attackBoost);
            selectedItem.defenseBoost = EditorGUILayout.IntField("Defense Boost", selectedItem.defenseBoost);
            selectedItem.hpBoost = EditorGUILayout.IntField("HP Boost", selectedItem.hpBoost);

            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("Special Effect");
            selectedItem.equipmentEffect = EditorGUILayout.TextArea(selectedItem.equipmentEffect, GUILayout.Height(40));

            // Preview stats
            if (selectedItem.attackBoost != 0 || selectedItem.defenseBoost != 0 || selectedItem.hpBoost != 0 || !string.IsNullOrEmpty(selectedItem.equipmentEffect))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Stat Summary:", EditorStyles.boldLabel);

                if (selectedItem.attackBoost != 0)
                    EditorGUILayout.LabelField("• Attack: " + (selectedItem.attackBoost > 0 ? "+" : "") + selectedItem.attackBoost);

                if (selectedItem.defenseBoost != 0)
                    EditorGUILayout.LabelField("• Defense: " + (selectedItem.defenseBoost > 0 ? "+" : "") + selectedItem.defenseBoost);

                if (selectedItem.hpBoost != 0)
                    EditorGUILayout.LabelField("• Max HP: " + (selectedItem.hpBoost > 0 ? "+" : "") + selectedItem.hpBoost);

                if (!string.IsNullOrEmpty(selectedItem.equipmentEffect))
                    EditorGUILayout.LabelField("• " + selectedItem.equipmentEffect);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawVisualsSection()
    {
        EditorGUILayout.BeginVertical("box");
        showVisuals = EditorGUILayout.Foldout(showVisuals, "VISUAL & AUDIO", true, EditorStyles.foldoutHeader);

        if (showVisuals)
        {
            EditorGUILayout.Space(5);

            selectedItem.itemPrefab = (GameObject)EditorGUILayout.ObjectField("Item Prefab", selectedItem.itemPrefab, typeof(GameObject), false);
            selectedItem.useAnimation = (AnimationClip)EditorGUILayout.ObjectField("Use Animation", selectedItem.useAnimation, typeof(AnimationClip), false);
            selectedItem.useSound = (AudioClip)EditorGUILayout.ObjectField("Use Sound", selectedItem.useSound, typeof(AudioClip), false);
            selectedItem.useEffect = (ParticleSystem)EditorGUILayout.ObjectField("Use Effect", selectedItem.useEffect, typeof(ParticleSystem), false);

            // Quick access buttons
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (selectedItem.itemPrefab != null && GUILayout.Button("Select Prefab"))
            {
                Selection.activeObject = selectedItem.itemPrefab;
                EditorGUIUtility.PingObject(selectedItem.itemPrefab);
            }

            if (selectedItem.useSound != null && GUILayout.Button("Select Sound"))
            {
                Selection.activeObject = selectedItem.useSound;
                EditorGUIUtility.PingObject(selectedItem.useSound);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void CreateNewItem()
    {
        string folderPath = "Assets/Scripts/Battle/PlayerCharacterStats";

        // Create folders if they don't exist
        if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
        {
            AssetDatabase.CreateFolder("Assets", "Scripts");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Scripts/Battle"))
        {
            AssetDatabase.CreateFolder("Assets/Scripts", "Battle");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Scripts/Battle", "PlayerCharacterStats");
        }

        ShopItem newItem = CreateInstance<ShopItem>();
        newItem.itemName = "New Item";
        newItem.description = "Item description";
        newItem.category = ItemCategory.Consumable;
        newItem.price = 10;
        newItem.spawnChance = 100f;

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/NewItem.asset");
        AssetDatabase.CreateAsset(newItem, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        LoadAllItems();
        selectedItem = newItem;
        selectedItemIndex = shopItems.IndexOf(newItem);

        Debug.Log("New shop item created at: " + uniquePath);
    }

    private void DeleteItem(int index)
    {
        if (index < 0 || index >= shopItems.Count) return;

        string path = AssetDatabase.GetAssetPath(shopItems[index]);
        AssetDatabase.DeleteAsset(path);

        shopItems.RemoveAt(index);

        if (selectedItemIndex == index)
        {
            selectedItem = shopItems.Count > 0 ? shopItems[0] : null;
            selectedItemIndex = shopItems.Count > 0 ? 0 : -1;
        }
        else if (selectedItemIndex > index)
        {
            selectedItemIndex--;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}