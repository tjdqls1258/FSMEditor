using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Util_Patten.FSM.Editor
{
    public class FSMGraphView : GraphView
    {
        private Type _currentContextType;
        public Type CurrentContextType
        {
            get => _currentContextType;
            set
            {
                _currentContextType = value;
                ClearGraph();
                LoadGraph();
            }
        }

        public Type CurrentStateType => typeof(StateSO<>).MakeGenericType(CurrentContextType);

        public FSMGraphView()
        {
            style.flexGrow = 1;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();
        }

        private void ClearGraph()
        {
            DeleteElements(graphElements.ToList());
        }

        private void LoadGraph()
        {
            if (CurrentContextType == null) return;

            Dictionary<ScriptableObject, FSMStateNode> soToNode = new Dictionary<ScriptableObject, FSMStateNode>();

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (CurrentStateType.IsInstanceOfType(so))
                {
                    var soSer = new SerializedObject(so);
                    Vector2 pos = soSer.FindProperty("nodePosition").vector2Value;

                    FSMStateNode node = new FSMStateNode(so.name, pos, CurrentContextType);
                    node.InitializeFromSO(so); 
                    AddElement(node);

                    soToNode[so] = node; 
                }
            }

            foreach (var kvp in soToNode)
            {
                var so = kvp.Key;
                var node = kvp.Value;
                var soSer = new SerializedObject(so);
                var transProp = soSer.FindProperty("transitions");

                for (int i = 0; i < transProp.arraySize; i++)
                {
                    var element = transProp.GetArrayElementAtIndex(i);

                    var trueSO = element.FindPropertyRelative("trueState").objectReferenceValue as ScriptableObject;
                    if (trueSO != null && soToNode.ContainsKey(trueSO))
                    {
                        var targetNode = soToNode[trueSO];
                        var outputPort = node.transitions[i].truePort;
                        var inputPort = targetNode.inputContainer.Q<Port>();

                        var edge = outputPort.ConnectTo(inputPort);
                        AddElement(edge);
                    }
                }
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port => {
                if (startPort.node != port.node && startPort.direction != port.direction)
                    compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        public void CreateStateNode(string nodeName, Vector2 position)
        {
            if (CurrentContextType == null) return;

            FSMStateNode node = new FSMStateNode(nodeName, position, CurrentContextType);
            AddElement(node);
        }
    }
}