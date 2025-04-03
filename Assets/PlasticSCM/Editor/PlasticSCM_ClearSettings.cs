using UnityEditor;

public class PlasticSCM_ClearSettings
{

    [MenuItem("PlasticSCM Utils/Clear Pending Changes Tree Persistent Data")]
    internal static void ClearPendingChangesTreePersistentData()
    {
        Clear(PENDING_CHANGES_TABLE_SETTINGS_NAME);
        Clear(BRANCHES_LIST_TABLE_SETTINGS_NAME);
        Clear(INCOMING_TABLE_SETTINGS_NAME);
        Clear(CHANGESETS_TABLE_SETTINGS_NAME);
    }
    static void Clear(string treeSettingsName)
    {
        EditorPrefs.DeleteKey(
            GetSettingKey(treeSettingsName, VISIBLE_COLUMNS_KEY));
        EditorPrefs.DeleteKey(
            GetSettingKey(treeSettingsName, COLUMNS_WIDTHS_KEY));
        EditorPrefs.DeleteKey(
            GetSettingKey(treeSettingsName, SORT_COLUMN_INDEX_KEY));
        EditorPrefs.DeleteKey(
            GetSettingKey(treeSettingsName, SORT_ASCENDING_KEY));
    }
    
    static string GetSettingKey(string treeSettingsName, string key)
    {
        return string.Format(treeSettingsName, PlayerSettings.productGUID, key);
    }

    static string VISIBLE_COLUMNS_KEY = "VisibleColumns";
    static string COLUMNS_WIDTHS_KEY = "ColumnWidths";
    static string SORT_COLUMN_INDEX_KEY = "SortColumnIdx";
    static string SORT_ASCENDING_KEY = "SortAscending";
    static string PENDING_CHANGES_TABLE_SETTINGS_NAME = "{0}_PendingChangesTreeV2_{1}";
    static string BRANCHES_LIST_TABLE_SETTINGS_NAME = "{0}_BranchesList_{1}";
    static string INCOMING_TABLE_SETTINGS_NAME = "{0}_DeveloperIncomingChangesTreeV2_{1}";
    static string CHANGESETS_TABLE_SETTINGS_NAME = "{0}_ChangesetsListV2_{1}";
    

}
