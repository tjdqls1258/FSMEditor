using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Util_Patten.FSM.Editor
{
    public class FSMGraphWindow : EditorWindow
    {
        private FSMGraphView graphView;
        private FSMSearchWindow searchWindow;

        [MenuItem("Tools/FSM Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<FSMGraphWindow>();
            window.titleContent = new GUIContent("FSM Editor");
            window.Show();
        }

        private void OnEnable() { ConstructGraphView(); GenerateToolbar(); }
        private void OnDisable() { if (graphView != null) rootVisualElement.Remove(graphView); }

        private void ConstructGraphView()
        {
            graphView = new FSMGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            searchWindow = ScriptableObject.CreateInstance<FSMSearchWindow>();
            searchWindow.Init(graphView, this);
            graphView.nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void GenerateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            var contextTypes = TypeCache.GetTypesDerivedFrom<Context>().Where(t => !t.IsAbstract).ToList();
            List<string> typeNames = contextTypes.Select(t => t.Name).ToList();

            DropdownField contextDropdown = new DropdownField("Target Context", typeNames, 0);
            contextDropdown.style.width = 250;

            contextDropdown.RegisterValueChangedCallback(evt =>
            {
                int index = typeNames.IndexOf(evt.newValue);
                graphView.CurrentContextType = contextTypes[index];
            });

            if (contextTypes.Count > 0)
                graphView.CurrentContextType = contextTypes[0]; 

            toolbar.Add(contextDropdown);

            Button saveButton = new Button(() => SaveData()) { text = "Save Data (Bake to SO)" };
            toolbar.Add(saveButton);

            Button loadButton = new Button(() => LoadData()) { text = "Load Data" };
            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }

        private void SaveData()
        {
            var nodes = graphView.nodes.ToList().Cast<FSMStateNode>().ToList();
            if (nodes.Count == 0) return;

            Type stateBaseType = typeof(StateSO<>).MakeGenericType(graphView.CurrentContextType);
            Type specificStateType = TypeCache.GetTypesDerivedFrom(stateBaseType).FirstOrDefault(t => !t.IsAbstract);
            Type actionBaseType = typeof(ActionSO<>).MakeGenericType(graphView.CurrentContextType);
            Type transitionBaseType = typeof(Transition<>).MakeGenericType(graphView.CurrentContextType);

            Dictionary<FSMStateNode, ScriptableObject> nodeToSOMap = new Dictionary<FSMStateNode, ScriptableObject>();

            foreach (var node in nodes)
            {
                ScriptableObject so;
                if (node.BoundSO != null)
                {
                    so = node.BoundSO;
                }
                else
                {
                    so = ScriptableObject.CreateInstance(specificStateType);
                    string folderPath = "Assets/FSM_Data";
                    if (!AssetDatabase.IsValidFolder(folderPath)) AssetDatabase.CreateFolder("Assets", "FSM_Data");

                    AssetDatabase.CreateAsset(so, $"{folderPath}/{node.StateName}_{node.GUID.Substring(0, 5)}.asset");
                    node.BoundSO = so;
                }
                nodeToSOMap.Add(node, so);
            }

            foreach (var node in nodes)
            {
                ScriptableObject so = nodeToSOMap[node];

                stateBaseType.GetField("nodePosition").SetValue(so, node.GetPosition().position);

                Array actionArray = Array.CreateInstance(actionBaseType, node.actionFields.Count);
                for (int i = 0; i < node.actionFields.Count; i++)
                {
                    actionArray.SetValue(node.actionFields[i].value, i);
                }
                stateBaseType.GetField("actions").SetValue(so, actionArray);

                Array transArray = Array.CreateInstance(transitionBaseType, node.transitions.Count);
                for (int i = 0; i < node.transitions.Count; i++)
                {
                    object transInst = Activator.CreateInstance(transitionBaseType);
                    var tData = node.transitions[i];

                    transitionBaseType.GetField("condition").SetValue(transInst, tData.conditionField.value);

                    if (tData.truePort.connections.Count() > 0)
                    {
                        var targetNode = tData.truePort.connections.First().input.node as FSMStateNode;
                        transitionBaseType.GetField("trueState").SetValue(transInst, nodeToSOMap[targetNode]);
                    }

                    if (tData.falsePort.connections.Count() > 0)
                    {
                        var targetNode = tData.falsePort.connections.First().input.node as FSMStateNode;
                        transitionBaseType.GetField("falseState").SetValue(transInst, nodeToSOMap[targetNode]);
                    }

                    transArray.SetValue(transInst, i);
                }
                stateBaseType.GetField("transitions").SetValue(so, transArray);

                EditorUtility.SetDirty(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void LoadData()
        {
            var existingNodes = graphView.nodes.ToList();
            var existingEdges = graphView.edges.ToList();
            foreach (var n in existingNodes) graphView.RemoveElement(n);
            foreach (var e in existingEdges) graphView.RemoveElement(e);

            Type stateBaseType = typeof(StateSO<>).MakeGenericType(graphView.CurrentContextType);
            Type specificStateType = TypeCache.GetTypesDerivedFrom(stateBaseType).FirstOrDefault(t => !t.IsAbstract);
            if (specificStateType == null) return;

            string[] guids = AssetDatabase.FindAssets($"t:{specificStateType.Name}");
            if (guids.Length == 0)
            {
                Debug.Log("불러올 데이터가 없습니다.");
                return;
            }

            Dictionary<ScriptableObject, FSMStateNode> soToNodeMap = new Dictionary<ScriptableObject, FSMStateNode>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                Vector2 pos = (Vector2)stateBaseType.GetField("nodePosition").GetValue(so);

                FSMStateNode node = new FSMStateNode(so.name, pos, graphView.CurrentContextType);
                node.BoundSO = so; 

                Array actions = (Array)stateBaseType.GetField("actions").GetValue(so);
                if (actions != null)
                {
                    foreach (var action in actions)
                    {
                        node.AddActionField();
                        node.actionFields.Last().value = (UnityEngine.Object)action; // 필드에 에셋 끼워넣기
                    }
                }

                Array transitions = (Array)stateBaseType.GetField("transitions").GetValue(so);
                if (transitions != null)
                {
                    Type transitionBaseType = typeof(Transition<>).MakeGenericType(graphView.CurrentContextType);
                    foreach (var trans in transitions)
                    {
                        node.AddTransitionPort();
                        var cond = (UnityEngine.Object)transitionBaseType.GetField("condition").GetValue(trans);
                        node.transitions.Last().conditionField.value = cond;
                    }
                }

                graphView.AddElement(node);
                soToNodeMap.Add(so, node); 
            }

            foreach (var kvp in soToNodeMap)
            {
                ScriptableObject so = kvp.Key;
                FSMStateNode node = kvp.Value;

                Array transitions = (Array)stateBaseType.GetField("transitions").GetValue(so);
                if (transitions != null)
                {
                    Type transitionBaseType = typeof(Transition<>).MakeGenericType(graphView.CurrentContextType);
                    for (int i = 0; i < transitions.Length; i++)
                    {
                        var trans = transitions.GetValue(i);
                        var trueState = (ScriptableObject)transitionBaseType.GetField("trueState").GetValue(trans);
                        var falseState = (ScriptableObject)transitionBaseType.GetField("falseState").GetValue(trans);

                        if (trueState != null && soToNodeMap.ContainsKey(trueState))
                        {
                            Edge edge = node.transitions[i].truePort.ConnectTo(soToNodeMap[trueState].inputPort);
                            graphView.AddElement(edge);
                        }

                        if (falseState != null && soToNodeMap.ContainsKey(falseState))
                        {
                            Edge edge = node.transitions[i].falsePort.ConnectTo(soToNodeMap[falseState].inputPort);
                            graphView.AddElement(edge);
                        }
                    }
                }
            }
        }

        private void ConnectTransitionState(SerializedProperty transitionProp, string propertyName, Port port, Dictionary<FSMStateNode, ScriptableObject> nodeToSO)
        {
            var edge = port.connections.FirstOrDefault();
            if (edge != null)
            {
                var targetNode = edge.input.node as FSMStateNode;
                if (targetNode != null && nodeToSO.ContainsKey(targetNode))
                {
                    transitionProp.FindPropertyRelative(propertyName).objectReferenceValue = nodeToSO[targetNode];
                }
            }
        }
    }
}