#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FSMEditor
{
    public class StateNode : UnityEditor.Experimental.GraphView.Node
    {
        public Action<StateNode> OnNodeSelected;
        public FSMState state;
        public Port inport;
        public Port outport;

        public StateNode(FSMState state)
        {
            focusable = true;
            this.state = state;
            this.title = state.name;
            this.viewDataKey = state.guid;

            style.left = state.position.x;
            style.top = state.position.y;

            CreateInputPorts();
            CreateOutPorts();
        }

        private void CreateOutPorts()
        {
            outport = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));

            if(outport != null)
            {
                outport.focusable = true;
                outport.portName = "";
                outputContainer.Add(outport);
            }
        }

        private void CreateInputPorts()
        {
            if (state is FSMRoot)
                return;
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
            state.position.x = newPos.x;
            state.position.y = newPos.y;
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