using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

//By Dreadrith#3238
//https://discord.gg/ZsPfrGn
//Github: https://github.com/Dreadrith/DreadScripts
//Gumroad: https://gumroad.com/dreadrith

namespace DreadScripts.SelectionHelper
{
    public class SceneObjectSelector : EditorWindow
    {


        #region Pref Keys
        public const string PREF_PREFIX = "DS_SelectionHelperData_";
        public static readonly string PREF_ONCOLOR = $"{PREF_PREFIX}SelectedColor";
        public static readonly string PREF_OFFCOLOR = $"{PREF_PREFIX}DeselectedColor";
        public static readonly string PREF_MINSIZE = $"{PREF_PREFIX}MinSize";
        public static readonly string PREF_MAXSIZE = $"{PREF_PREFIX}MaxSize";
        public static readonly string PREF_SIZE = $"{PREF_PREFIX}Size";
        public static readonly string PREF_HUMANOID = $"{PREF_PREFIX}OnlyHumanoid";

        public static void PrefSetColor(string key, Color value)
        {
            PlayerPrefs.SetFloat($"{key}R", value.r);
            PlayerPrefs.SetFloat($"{key}G", value.g);
            PlayerPrefs.SetFloat($"{key}B", value.b);
        }

        public static Color PrefGetColor(string key, Color defaultColor) => new Color(
            PlayerPrefs.GetFloat($"{key}R", defaultColor.r),
            PlayerPrefs.GetFloat($"{key}G", defaultColor.g),
            PlayerPrefs.GetFloat($"{key}B", defaultColor.b));

        public static void PrefSetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
        public static bool PrefGetBool(string key, bool defaultValue) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        #endregion

        [InitializeOnLoadMethod]
        public static void LoadSettings()
        {
            OnColor = PrefGetColor(PREF_ONCOLOR, new Color(0.4f, 0.85f, 0.65f));
            OffColor = PrefGetColor(PREF_OFFCOLOR, new Color(0.8f, 0.15f, 0.35f));
            minHandleSize = PlayerPrefs.GetFloat(PREF_MINSIZE, 0.005f);
            maxHandleSize = PlayerPrefs.GetFloat(PREF_MAXSIZE, 0.04f);

            handleSize = PlayerPrefs.GetFloat(PREF_SIZE, 0.00525f);
            onlyHumanoid = PrefGetBool(PREF_HUMANOID, false);

            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;
            Selection.selectionChanged -= OnSelectionChange;
            Selection.selectionChanged += OnSelectionChange;
        }

        public static void SaveSettings()
        {
            PrefSetColor(PREF_ONCOLOR, OnColor);
            PrefSetColor(PREF_OFFCOLOR, OffColor);
            PlayerPrefs.SetFloat(PREF_MINSIZE, minHandleSize);
            PlayerPrefs.SetFloat(PREF_MAXSIZE, maxHandleSize);
            PlayerPrefs.SetFloat(PREF_SIZE, handleSize);
            PrefSetBool(PREF_HUMANOID, onlyHumanoid);
            SceneView.RepaintAll();
        }

        public static void Disable()
        {
            SceneView.duringSceneGui -= OnScene;
            Selection.selectionChanged -= OnSelectionChange;
        }

        [MenuItem("DreadTools/Scripts Settings/Scene Object Selector")]
        public static void showWindow()
        {
            GetWindow<SceneObjectSelector>("Scene Selector Settings");
        }

        private static bool selecting;
        private static bool onlyHumanoid;
        private static float handleSize, minHandleSize, maxHandleSize;
        private static Transform[] sceneObjects;
        private static bool ignoreDBones = true, includeRoots = true;
        private static bool hasDbones = (null != System.Type.GetType("DynamicBone"));
        private static bool[] bitmask;
        private static Color OnColor;
        private static Color OffColor;
        private static void OnScene(SceneView sceneview)
        {
            Handles.BeginGUI();
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorGUIUtility.IconContent("CapsuleCollider2D Icon"), GUIStyle.none, GUILayout.Width(20), GUILayout.Height(20)))
            {
                Event e = Event.current;
                if (e.button == 0)
                {
                    selecting = !selecting;
                    if (selecting)
                    {
                        if (!onlyHumanoid)
                        {
                            sceneObjects = FindObjectsOfType<Transform>();

                            if (hasDbones && ignoreDBones)
                            {
                                System.Type dboneType = System.Type.GetType("DynamicBone");
                                List<Transform> dbones = new List<Transform>();
                                Object[] dboneScripts = FindObjectsOfType(dboneType);
                                foreach (Object b in dboneScripts)
                                {
                                    SerializedObject sb = new SerializedObject(b);
                                    List<Transform> exclusionList = new List<Transform>();
                                    SerializedProperty excProp = sb.FindProperty("m_Exclusions");
                                    for (int i = 0; i < excProp.arraySize; i++)
                                        exclusionList.Add((Transform) excProp.GetArrayElementAtIndex(i).objectReferenceValue);
                                    GetBoneChildren(dbones, exclusionList, (Transform) sb.FindProperty("m_Root").objectReferenceValue, includeRoots);
                                }

                                sceneObjects = sceneObjects.Except(dbones).ToArray();
                            }
                        }
                        else
                        {
                            var list = new List<Transform>();
                            foreach (var a in FindObjectsOfType<Animator>().Where(a => a.isHuman && a.avatar))
                            {
                                for (int i = 0; i < 55; i++)
                                {
                                    var t = a.GetBoneTransform((HumanBodyBones) i);
                                    if (t) list.Add(t);
                                }
                            }

                            sceneObjects = list.ToArray();
                        }
                        refreshBitMask();
                    }
                }
                if (e.button == 1)
                {
                    GetWindow<SceneObjectSelector>(false, "Scene Selector Settings", true);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (selecting)
            {
                //a
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                handleSize = GUILayout.VerticalSlider(handleSize, maxHandleSize, minHandleSize, GUILayout.Height(50), GUILayout.Width(15));
                if (EditorGUI.EndChangeCheck())
                    SaveSettings();
                
                GUILayout.Space(1);
                EditorGUILayout.EndHorizontal();
            }
            Handles.EndGUI();

            if (selecting)
            {
                if (sceneObjects.Length > 0)
                {
                    for (int i = 0; i < sceneObjects.Length; i++)
                    {
                        if (!sceneObjects[i])
                            continue;
                        int controlID = sceneObjects[i].GetHashCode();
                        Event e = Event.current;
                        if (bitmask[i])
                            Handles.color = OnColor;
                        else
                            Handles.color = OffColor;
                        Handles.SphereHandleCap(controlID, sceneObjects[i].position, Quaternion.identity, handleSize, EventType.Repaint);
                        switch (e.GetTypeForControl(controlID))
                        {
                            case EventType.MouseDown:
                                if (HandleUtility.nearestControl == controlID && e.button == 0)
                                {
                                    if (e.control)
                                    {
                                        if (!Selection.objects.Contains(sceneObjects[i].gameObject))
                                        {
                                            Selection.objects = Selection.objects.Concat(new GameObject[] { sceneObjects[i].gameObject }).ToArray();
                                        }
                                        else
                                        {
                                            Selection.objects = Selection.objects.Except(new GameObject[] { sceneObjects[i].gameObject }).ToArray();
                                        }
                                    }
                                    else
                                    {
                                        Selection.activeObject = sceneObjects[i].gameObject;
                                    }
                                    e.Use();
                                }
                                break;
                            case EventType.Layout:
                                float distance = HandleUtility.DistanceToCircle(sceneObjects[i].position, handleSize / 2f);
                                HandleUtility.AddControl(controlID, distance);
                                break;
                        }
                    }
                }
            }

        }

        private static void GetBoneChildren(List<Transform> dbones, List<Transform> exclusionList, Transform parent, bool first = false)
        {
            if (exclusionList.Contains(parent)) return;

            if (!first) dbones.Add(parent);
            for (int i = 0; i < parent.childCount; i++)
                GetBoneChildren(dbones, exclusionList, parent.GetChild(i));
            
        }


        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            using (new GUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 20;
                EditorGUILayout.LabelField("Handle Size");

                minHandleSize = EditorGUILayout.FloatField(minHandleSize, GUILayout.Width(40));
                handleSize = GUILayout.HorizontalSlider(handleSize, minHandleSize, maxHandleSize);
                maxHandleSize = EditorGUILayout.FloatField(maxHandleSize, GUILayout.Width(40));
                EditorGUIUtility.labelWidth = 0;
            }

            OnColor = EditorGUILayout.ColorField("Selected", OnColor);
            OffColor = EditorGUILayout.ColorField("Deselected", OffColor);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 70;

                onlyHumanoid = EditorGUILayout.ToggleLeft("Only Humanoid", onlyHumanoid);
                    if (hasDbones)
                    {
                        ignoreDBones = EditorGUILayout.ToggleLeft("Ignore D-Bones", ignoreDBones);
                        includeRoots = EditorGUILayout.ToggleLeft("Include D-Bone Roots", includeRoots);
                    }
            }

            if (EditorGUI.EndChangeCheck())
                SaveSettings();

            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Made by Dreadrith#3238", "boldlabel"))
                    Application.OpenURL("https://github.com/Dreadrith/DreadScripts");
            }

        }

        private static void OnSelectionChange()
        {
            if (selecting) refreshBitMask();
        }
        private static void refreshBitMask()
        {
            bitmask = new bool[sceneObjects.Length];
            for (int i = 0; i < sceneObjects.Length; i++)
            {
                if (!sceneObjects[i])
                {
                    sceneObjects = sceneObjects.Except(new Transform[] { sceneObjects[i] }).ToArray();
                    refreshBitMask();
                    break;
                }

                if (Selection.objects.Contains(sceneObjects[i].gameObject))
                    bitmask[i] = true;
                else
                    bitmask[i] = false;
            }
        }
        
        
    }
}
