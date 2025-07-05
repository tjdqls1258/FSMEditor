using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMEditor
{
    public class FSMachineEditor : EditorWindow
    {
        FSMachineView machineView;
        InspectorView inspectorView;
        IMGUIContainer blackboardView;

        SerializedObject machineObject;
        SerializedProperty blackboardproperty;

        const string EditorPath = "Assets/FSMEditor/Editor/FSMView/FSMEditor";

        [MenuItem("Tools/FSMEditor")]
        public static void OpenWindow()
        {
            FSMachineEditor wnd = GetWindow<FSMachineEditor>();
            wnd.titleContent = new GUIContent("FSMEditor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if(Selection.activeObject is FSMachine)
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{EditorPath}.uxml");
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{EditorPath}.uss");
            root.styleSheets.Add(styleSheet);

            machineView = root.Q<FSMachineView>();
            inspectorView = root.Q<InspectorView>();
            blackboardView = root.Q<IMGUIContainer>();
            blackboardView.onGUIHandler = () =>
            {
                if (machineObject == null) return;
                machineObject.Update();
                EditorGUILayout.PropertyField(blackboardproperty);
                machineObject.ApplyModifiedProperties();
            };

            machineView.OnNodeSelected = OnNodeSelectionChanged;
            machineView.OnTransitionNodeSelected = OnTransitionSelectionChanged;
            OnSelectionChange();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;

            }
        }

        private void OnSelectionChange()
        {
            FSMachine machine = Selection.activeObject as FSMachine;
            if(!machine)
            {
                if(Selection.activeGameObject)
                {
                    FSMRunner runner = Selection.activeGameObject.GetComponent<FSMRunner>();
                    if(runner)
                    {
                        machine = runner.machine;
                    }
                }
            }
            if (Application.isPlaying)
            {
                if(machine)
                    machineView.PopulateView(machine);
            }
            else
            {
                if (machine && AssetDatabase.CanOpenAssetInEditor(machine.GetInstanceID()))
                {
                    machineView.PopulateView(machine);
                }
            }

            if(machine != null )
            {
                machineObject = new SerializedObject(machine);
                blackboardproperty = machineObject.FindProperty("blackBoard");
            }
        }

        private void OnNodeSelectionChanged(StateNode node)
        {
            inspectorView.UpdateSelection(node);
        }

        private void OnTransitionSelectionChanged(TransitionNode node)
        {
            inspectorView.UpdateSelection(node);
        }

        private void OnInspectorUpdate()
        {
            machineView?.UpdateNodeState();
        }
    }
}