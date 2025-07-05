#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.VersionControl.Asset;

namespace FSMEditor
{
    public class FSMachineView : GraphView
    {
        private const string SHEET_PATH = "Assets/FSMEditor/Editor/FSMView/FSMEditor.uss";
        public Action<StateNode> OnNodeSelected;
        public Action<TransitionNode> OnTransitionNodeSelected;
        FSMachine machine;
        public new class UxmlFactory : UxmlFactory<FSMachineView, GraphView.UxmlTraits>
        {

        }
        public FSMachineView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            string filePath = Application.dataPath;

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(SHEET_PATH);
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            PopulateView(machine);
            AssetDatabase.SaveAssets();
        }

        StateNode FindStateNodeView(FSMState node)
        {
            return GetNodeByGuid(node.guid) as StateNode;
        }

        RootNode FindRootNodeView(FSMRoot root)
        {
            return GetNodeByGuid(root.guid) as RootNode;
        }

        TransitionNode FindTransitionNodeView(FSMTransition node)
        {
            return GetNodeByGuid(node.guid) as TransitionNode;
        }

        internal void PopulateView(FSMachine machine)
        {
            this.machine = machine;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            if(machine.root == null)
            {
                machine.root = machine.CreateRoot(typeof(FSMRoot)) as FSMRoot;
                EditorUtility.SetDirty(machine);
                AssetDatabase.SaveAssets();
            }
            CreateNodeView(machine.root);
            machine.states.ForEach(n => CreateNodeView(n));
            machine.transitions.ForEach(n => CreateTransition(n));

            RootNode root = FindRootNodeView(machine.root);
            if (machine.root.startState != null)
            {
                StateNode state = FindStateNodeView(machine.root.startState);
                Edge edge = root.outport.ConnectTo(state.inport);
                edge.focusable = true;
                AddElement(edge);
            }

            machine.states.ForEach(n =>
            {
                var children = machine.GetChildren(n);
                foreach (var c in children)
                {
                    StateNode input = FindStateNodeView(n);
                    TransitionNode output = FindTransitionNodeView(c);

                    Edge edge = input.outport.ConnectTo(output.inport);
                    edge.focusable = true;
                    AddElement(edge);
                }
            });
            machine.transitions.ForEach(t =>
            {
                var children = machine.GetChildren(t);
                foreach (var c in children)
                {
                    if (c == null) continue;
                    TransitionNode input = FindTransitionNodeView(t);
                    StateNode output = FindStateNodeView(c);
                    
                    Edge edge = input.outport.ConnectTo(output.inport);
                    edge.focusable = true;
                    AddElement(edge);
                }
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endport => endport.direction != startPort.direction && endport.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if(graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    StateNode nodeView = elem as StateNode;
                    if (nodeView != null)
                    {
                        machine.DeleteNode(nodeView.state);
                    }

                    TransitionNode tnode = elem as TransitionNode;
                    if(tnode != null)
                    {
                        machine.DeleteNode(tnode.transition);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null)
                    {
                        edge.focusable = true;

                        if (edge.output.node is RootNode && edge.input.node is StateNode)
                        {
                            RootNode from = edge.output.node as RootNode;
                            StateNode to = edge.input.node as StateNode;
                            edge.focusable = true;
                            machine.RemoveChild(from.root, to.state);
                            return;
                        }
                        else if(edge.output.node is RootNode && edge.input.node is TransitionNode)
                        {
                            return;
                        }
                        if (edge.output.node is TransitionNode && edge.input.node is StateNode)
                        {
                            TransitionNode from = edge.output.node as TransitionNode;
                            StateNode to = edge.input.node as StateNode;

                            machine.RemoveChild(from.transition, to.state);
                        }
                        else if (edge.output.node is StateNode && edge.input.node is TransitionNode)
                        {
                            StateNode from = edge.output.node as StateNode;
                            TransitionNode to = edge.input.node as TransitionNode;

                            if (edge.output.node is FSMRoot)
                                return;

                            machine.RemoveChild(from.state, to.transition);
                        }
                    }
                });
            }

            if(graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    if((edge.output.node.GetType() == edge.input.node.GetType()) ||
                    (edge.output.node is RootNode && edge.input.node is TransitionNode))
                    {
                        edge.ClearBindings();
                        edge.Clear();
                        return;
                    }
                    
                    if(edge.output.node is RootNode && edge.input.node is StateNode)
                    {
                        RootNode from = edge.output.node as RootNode;
                        StateNode to = edge.input.node as StateNode;
                        edge.focusable = true;
                        machine.AddChild(from.root, to.state);
                    }
                    else if (edge.output.node is TransitionNode && edge.input.node is StateNode)
                    {
                        TransitionNode from = edge.output.node as TransitionNode;
                        StateNode to = edge.input.node as StateNode;
                        edge.focusable = true;
                        machine.AddChild(from.transition, to.state);
                    }
                    else if(edge.output.node is StateNode && edge.input.node is TransitionNode)
                    {
                        StateNode from = edge.output.node as StateNode;
                        TransitionNode to = edge.input.node as TransitionNode;

                        edge.focusable = true;
                        machine.AddChild(from.state, to.transition);
                    }
                });
            }
            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);
            if (machine == null) return;
            evt.menu.AppendAction("Delete", (a) => 
            {
                if (selection.Any(x => x is RootNode))
                {
                    selection.RemoveAll(x=> x is RootNode);
                }
                DeleteSelection();
            });

            {
                var types = TypeCache.GetTypesDerivedFrom<FSMTransition>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", (a) => CreateTransition(type));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<FSMState>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", (a) => CreateState(type));
                }
            }

            //{
            //    var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
            //    foreach (var type in types)
            //    {
            //        evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", (a) => CreateState(type));
            //    }
            //}
        }

        void CreateState(System.Type type)
        {
            FSMState state = machine.CreateState(type);
            CreateNodeView(state);
        }
        void CreateNodeView(FSMState state)
        {
            StateNode fsmState = new StateNode(state);
            fsmState.OnNodeSelected = OnNodeSelected;
            AddElement(fsmState);
        }
        void CreateNodeView(FSMRoot root)
        {
            RootNode fsmState = new RootNode(root);
            fsmState.focusable = false;
            AddElement(fsmState);
        }

        void CreateTransition(System.Type type)
        {
            FSMTransition state = machine.CreateTransition(type);
            CreateTransition(state);
        }
        void CreateTransition(FSMTransition tr)
        {
            TransitionNode fsmState = new TransitionNode(tr);
            fsmState.OnNodeSelected = OnTransitionNodeSelected;
            AddElement(fsmState);
        }

        public void UpdateNodeState()
        {
            if (Application.isPlaying == false) return;
            if (machine == null || machine.GetCurrentState() == null) return;
            nodes.ForEach((x) =>
            {
                StateNode state = x as StateNode;
                if (state != null)
                {
                    if (state.state.guid == machine.GetCurrentState().guid)
                        state.RunState();
                    else
                        state.NotRunState();
                }
            });
        }
    }
}
#endif