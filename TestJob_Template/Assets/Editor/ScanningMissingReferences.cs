using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MissingReferencesScanner : EditorWindow
{
    private enum Category { Prefabs, Materials, ScriptableObjects, Other, All }

    private class MissingReferenceInfo
    {
        public string AssetPath;
        public Object Object;
        public string ObjectType;
        public string FieldPath;
        public string ParentPrefab;
        public Category Category;
    }

    private Vector2 _scroll;
    private bool _isScanning;

    private readonly List<MissingReferenceInfo> _results = new();
    private readonly Dictionary<Category, List<MissingReferenceInfo>> _groupedResults = new();

    private Category _selectedTab = Category.All;

    private bool _includePrefabs = true;
    private bool _includeMaterials = true; 
    private bool _includeScriptableObjects = true;
    private bool _includeOther = false;


    [MenuItem("Tools/Missing References Scanner")]
    public static void OpenWindow()
    {
        var window = GetWindow<MissingReferencesScanner>("Missing References");
        window.minSize = new Vector2(800, 450);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Missing References Scanner", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Searches for broken references (Missing References) only in project assets (Assets/). " +
                                "Results are sorted into tabs depending on the asset type.", MessageType.Info);
        EditorGUILayout.Space();

        DrawFilters();

        EditorGUILayout.Space();

        if (!_isScanning)
        {
            if (GUILayout.Button("Start search Missing References", GUILayout.Height(30)))
                ScanProject();
        }
        else
        {
            GUILayout.Label("In Progress...", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.Space();

        DrawTabs();
        DrawResults();
    }
    private void DrawFilters()
    {
        EditorGUILayout.LabelField("Types to check:", EditorStyles.boldLabel);
        _includePrefabs = EditorGUILayout.ToggleLeft("Prefabs (.prefab)", _includePrefabs);
        _includeMaterials = EditorGUILayout.ToggleLeft("Materials (.mat)", _includeMaterials);
        _includeScriptableObjects = EditorGUILayout.ToggleLeft("ScriptableObjects (.asset)", _includeScriptableObjects);
        _includeOther = EditorGUILayout.ToggleLeft("Other Assets", _includeOther);
    }
    private void DrawTabs()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        foreach (Category cat in System.Enum.GetValues(typeof(Category)))
        {
            GUIStyle style = EditorStyles.miniButton;
            if (GUILayout.Toggle(_selectedTab == cat, cat.ToString(), style))
                _selectedTab = cat;
        }

        EditorGUILayout.EndHorizontal();
    }
    private void DrawResults()
    {
        if (_results.Count == 0)
        {
            EditorGUILayout.HelpBox("No data yet - start a search.", MessageType.None);
            return;
        }

        List<MissingReferenceInfo> currentList;

        if (_selectedTab == Category.All)
        {
            currentList = _results;
        }
        else
        {
            if (_groupedResults.TryGetValue(_selectedTab, out var list))
            {
                currentList = list;
            }
            else
            {
                currentList = null;
            }
        }

        if (currentList == null || currentList.Count == 0)
        {
            EditorGUILayout.HelpBox($"Tab {_selectedTab} is empty - no errors found.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Result: {currentList.Count}", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        string lastParent = null;

        foreach (var info in currentList)
        {
            if (info.ParentPrefab != lastParent)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField($"File_folder: {info.ParentPrefab ?? info.AssetPath}", EditorStyles.miniBoldLabel);
                lastParent = info.ParentPrefab;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Object: {info.ObjectType}");
            EditorGUILayout.LabelField($"Field: {info.FieldPath}");
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Show", GUILayout.Width(100)))
            {
                Selection.activeObject = info.Object;
                EditorGUIUtility.PingObject(info.Object);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }
    private void ScanProject()
    {
        _isScanning = true;
        _results.Clear();
        _groupedResults.Clear();

        try
        {
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            int total = assetPaths.Length;
            int processed = 0;

            foreach (var path in assetPaths)
            {
                processed++;
                if (!path.StartsWith("Assets/"))
                    continue;
                if (!ShouldInclude(path))
                    continue;
                if (processed % 50 == 0)
                {
                    EditorUtility.DisplayProgressBar("Scanning for Missing References", path, (float)processed / total);
                }

                Category category = GetCategory(path);
                var objs = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var o in objs)
                {
                    if (o == null) continue;

                    var so = new SerializedObject(o);
                    var prop = so.GetIterator();

                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                            {
                                var info = new MissingReferenceInfo
                                {
                                    AssetPath = path,
                                    Object = o,
                                    ObjectType = o.GetType().Name,
                                    FieldPath = prop.propertyPath,
                                    ParentPrefab = GetParentPrefabName(path),
                                    Category = category
                                };

                                _results.Add(info);

                                if (!_groupedResults.ContainsKey(category))
                                    _groupedResults[category] = new List<MissingReferenceInfo>();

                                _groupedResults[category].Add(info);
                            }
                        }
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            if (_results.Count == 0)
                EditorUtility.DisplayDialog("Result", "Missing links not found!", "OK");
            else
                EditorUtility.DisplayDialog("Result", $"Found {_results.Count} Missing links", "OK");
        }
        finally
        {
            _isScanning = false;
            EditorUtility.ClearProgressBar();
            Repaint();
        }
    }
    private bool ShouldInclude(string path)
    {
        if (path.EndsWith(".prefab")) return _includePrefabs;
        if (path.EndsWith(".mat")) return _includeMaterials;
        if (path.EndsWith(".asset")) return _includeScriptableObjects;
        return _includeOther;
    }
    private Category GetCategory(string path)
    {
        if (path.EndsWith(".prefab")) return Category.Prefabs;
        if (path.EndsWith(".mat")) return Category.Materials;
        if (path.EndsWith(".asset")) return Category.ScriptableObjects;
        return Category.Other;
    }
    private string GetParentPrefabName(string path)
    {
        if (path.EndsWith(".prefab"))
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            return fileName;
        }
        return null;
    }
}





