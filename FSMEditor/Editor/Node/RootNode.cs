#if UNITY_EDITOR
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FSMEditor
{
    public class RootNode : UnityEditor.Experimental.GraphView.Node
    {
        public FSMRoot root;
        public Port inport;
        public Port outport;

        public RootNode(FSMRoot root)
        {
            focusable = true;
            this.root = root;
            this.title = root.name;
            this.viewDataKey = root.guid;

            style.left = root.position.x;
            style.top = root.position.y;

            CreateOutPorts();
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
            root.position.x = newPos.x;
            root.position.y = newPos.y;
        }
        public override void OnSelected()
        {
            base.OnSelected();
        }
    }
}

#endif