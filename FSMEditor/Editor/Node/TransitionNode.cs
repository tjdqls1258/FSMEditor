#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMEditor
{
    public class TransitionNode : UnityEditor.Experimental.GraphView.Node
    {
        public Action<TransitionNode> OnNodeSelected;
        public FSMTransition transition;
        public Port inport;
        public Port outport;

        public TransitionNode(FSMTransition transition) : base("Assets/FSMEditor/Editor/Node/NodeView.uxml")
        {
            focusable = true;
            this.transition = transition;
            this.title = transition.name;
            this.viewDataKey = transition.guid;

            style.left = transition.position.x;
            style.top = transition.position.y;

            CreateInputPorts();
            CreateOutPorts();
            SetipClasses();

            Label description = this.Q<Label>("description-label");
            description.bindingPath = "description";
            description.Bind(new SerializedObject(transition));
        }

        private void SetipClasses()
        {
            AddToClassList("transition");
        }

        private void CreateOutPorts()
        {
            outport = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));

            if (outport != null)
            {
                outport.focusable = true;
                outport.portName = "";
                outputContainer.Add(outport);
            }
        }

        private void CreateInputPorts()
        {
            inport = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));

            if (inport != null)
            {
                inport.focusable = true;
                inport.portName = "";
                inputContainer.Add(inport);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(transition, "FSM Editor (Set Position)");
            transition.position.x = newPos.x;
            transition.position.y = newPos.y;
            EditorUtility.SetDirty(transition);
        }
        public override void OnSelected()
        {
            base.OnSelected();
            if (OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }
        }
    }
}
#endif