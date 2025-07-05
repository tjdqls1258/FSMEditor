#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace FSMEditor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Port inport;
        public Port outport;

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
    }
}
#endif