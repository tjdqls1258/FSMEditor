#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMEditor
{
    public class RootNode : UnityEditor.Experimental.GraphView.Node
    {
        public FSMRoot root;
        public Port inport;
        public Port outport;

        public RootNode(FSMRoot root) : base("Assets/FSMEditor/Editor/Node/NodeView.uxml")
        {
            focusable = true;
            this.root = root;
            this.title = root.name;
            this.viewDataKey = root.guid;

            style.left = root.position.x;
            style.top = root.position.y;

            CreateOutPorts();
            SetipClasses();
        }

        private void SetipClasses()
        {
            AddToClassList("root");
        }

        private void CreateOutPorts()
        {
            outport = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));

            if (outport != null)
            {
                outport.focusable = true;
                outport.portName = "";
                outputContainer.Add(outport);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(root, "FSM Editor (Set Position)");
            root.position.x = newPos.x;
            root.position.y = newPos.y;
            EditorUtility.SetDirty(root);
        }
        public override void OnSelected()
        {
            base.OnSelected();
        }
    }
}

#endif