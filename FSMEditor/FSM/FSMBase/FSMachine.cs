using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FSMEditor
{
    [CreateAssetMenu(fileName = "FSM", menuName = "Scriptable Objects/FSM")]
    public class FSMachine : ScriptableObject
    {
        [HideInInspector] public FSMRoot root;
        protected FSMState currentState;

        [HideInInspector] public bool isRun = false;
        public void StartMachine()
        {
            isRun = true;
            currentState = root.StartRoot();
            currentState.Enter();
        }

        public void Update()
        {
            currentState = currentState.Excute();
        }

        public FSMachine Clone()
        {
            FSMachine machine = Instantiate(this);
            machine.root = root.Clone();
            return machine;
        }
#if UNITY_EDITOR
        public List<FSMState> states = new();
        public List<FSMTransition> transitions = new();
        public FSMState CreateState(System.Type type)
        {
            FSMState state = ScriptableObject.CreateInstance(type) as FSMState;
            state.name = type.Name;
            state.guid = GUID.Generate().ToString();
            states.Add(state);

            AssetDatabase.AddObjectToAsset(state, this);
            AssetDatabase.SaveAssets();
            return state;
        }

        public FSMRoot CreateRoot(System.Type type)
        {
            FSMRoot state = ScriptableObject.CreateInstance(type) as FSMRoot;
            state.name = type.Name;
            state.guid = GUID.Generate().ToString();
            root = state;

            AssetDatabase.AddObjectToAsset(state, this);
            AssetDatabase.SaveAssets();
            return state;
        }

        public FSMTransition CreateTransition(System.Type type)
        {
            FSMTransition transition = ScriptableObject.CreateInstance(type) as FSMTransition;
            transition.name = type.Name;
            transition.guid = GUID.Generate().ToString();
            transitions.Add(transition);

            AssetDatabase.AddObjectToAsset(transition, this);
            AssetDatabase.SaveAssets();
            return transition;
        }

        public void DeleteNode(FSMState state)
        {
            states.Remove(state);

            AssetDatabase.RemoveObjectFromAsset(state);
            AssetDatabase.SaveAssets();
        }
        public void DeleteNode(FSMTransition transition)
        {
            transitions.Remove(transition);

            AssetDatabase.RemoveObjectFromAsset(transition);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(FSMState input, FSMTransition output)
        {
            input.transitionList.Add(output);
        }

        public void AddChild(FSMRoot input, FSMState output)
        {
            input.startState = output;
        }

        public void RemoveChild(FSMState input, FSMTransition output)
        {
            input.transitionList.Remove(output);
            //DecoratorNode decorator = parent as DecoratorNode;
            //if (decorator)
            //{
            //    decorator.child = null;
            //}

            //RootNode rootNode = parent as RootNode;
            //if (rootNode)
            //{
            //    rootNode.child = null;
            //}

            //CompositeNode composite = parent as CompositeNode;
            //if (composite)
            //{
            //    composite.children.Remove(child);
            //}
        }

        public void AddChild(FSMTransition input, FSMState output)
        {
            input.nextState = output;
            //DecoratorNode decorator = parent as DecoratorNode;
            //if (decorator)
            //{
            //    decorator.child = child;
            //}

            //RootNode rootNode = parent as RootNode;
            //if (rootNode)
            //{
            //    rootNode.child = child;
            //}

            //CompositeNode composite = parent as CompositeNode;
            //if (composite)
            //{
            //    composite.children.Add(child);
            //}
        }

        public void RemoveChild(FSMTransition input, FSMState output)
        {
            input.nextState = null;
        }
        public void RemoveChild(FSMRoot input, FSMState output)
        {
            input.startState = null;
        }

        public List<FSMTransition> GetChildren(FSMState parent)
        {
            List<FSMTransition> children = parent.transitionList;

            return children;
        }

        public List<FSMState> GetChildren(FSMTransition parent)
        {
            List<FSMState> children = new();
            children.Add(parent.nextState);

            return children;
        }
#endif
    }
}