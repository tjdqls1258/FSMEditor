using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMEditor
{
    public class FSMachineEditor : EditorWindow
    {
        FSMachineView treeView;
        InspectorView inspectorView;
        const string EditorPath = "Assets/FSMEditor/Editor/FSMView/FSMEditor";

        [MenuItem("Tools/FSMEditor")]
        public static void ShowExample()
        {
            FSMachineEditor wnd = GetWindow<FSMachineEditor>();
            wnd.titleContent = new GUIContent("FSMEditor");
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

            treeView = root.Q<FSMachineView>();
            inspectorView = root.Q<InspectorView>();
            treeView.OnNodeSelected = OnNodeSelectionChanged;
            treeView.OnTransitionNodeSelected = OnTransitionSelectionChanged;
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            FSMachine tree = Selection.activeObject as FSMachine;
            if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
            {
                treeView.PopulateView(tree);
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
    }
}