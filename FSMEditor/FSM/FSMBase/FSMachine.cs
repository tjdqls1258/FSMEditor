using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FSMEditor
{
    [CreateAssetMenu(fileName = "FSM", menuName = "Scriptable Objects/FSM")]
    public class FSMachine : ScriptableObject
    {
        [HideInInspector] public FSMRoot root;
        protected FSMState m_currentState;
        public List<FSMState> states = new();
        public List<FSMTransition> transitions = new();
        public BlackBoard blackBoard = new();
        [HideInInspector] public bool isRun = false;
        public void StartMachine()
        {
            isRun = true;
            m_currentState = root.StartRoot();
            m_currentState.Enter();
        }

        public void Update()
        {
            if(isRun)
                m_currentState = m_currentState.Excute();
        }

        public void Travers(Action<FSMRoot, FSMState, FSMTransition> action)
        {
            action.Invoke(root, null, null);
            states.ForEach(x=> action.Invoke(null, x, null));
            transitions.ForEach(t=> action.Invoke(null, null, t));
        }


        public FSMachine Clone()
        {
            FSMachine machine = Instantiate(this);
            machine.root = root.Clone();
            machine.states = states.ConvertAll<FSMState>(s => s.Clone());
            machine.transitions = transitions.ConvertAll<FSMTransition>(t => t.Clone());
            return machine;
        }

        //필요시 값 추가
        public void Bind(params object[] data)
        {
            Travers((r, s, t) =>
            {
                if(r != null)
                {
                    r.BindingData(data);
                }
                if(s != null)
                {
                    s.blackboard = blackBoard;
                    s.BindingData(data);
                }
                if(t != null)
                {
                    t.blackBoard = blackBoard;
                    t.BindingData(data);
                }
            });
        }

        public List<FSMState> FindState(Type type)
        {
            return states.FindAll(x => x.GetType() == type);
        }

        public List<FSMTransition> FindTransition(Type type)
        {
            return transitions.FindAll(x => x.GetType() == type);
        }
#if UNITY_EDITOR
        public FSMState CreateState(System.Type type)
        {
            FSMState state = ScriptableObject.CreateInstance(type) as FSMState;
            state.name = type.Name;
            state.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "FSM Editor (AddState)");
            states.Add(state);

            if(!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(state, this);
            }
            
            Undo.RegisterCreatedObjectUndo(state, "FSM Editor (AddState)");
            AssetDatabase.SaveAssets();
            return state;
        }

        public FSMRoot CreateRoot(System.Type type)
        {
            FSMRoot state = ScriptableObject.CreateInstance(type) as FSMRoot;
            state.name = type.Name;
            state.guid = GUID.Generate().ToString();
            root = state;

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(state, this);
            }

            AssetDatabase.SaveAssets();
            return state;
        }

        public FSMTransition CreateTransition(System.Type type)
        {
            FSMTransition transition = ScriptableObject.CreateInstance(type) as FSMTransition;
            transition.name = type.Name;
            transition.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "FSM Editor (AddTransition)");
            transitions.Add(transition);

            AssetDatabase.AddObjectToAsset(transition, this);
            Undo.RegisterCreatedObjectUndo(transition, "FSM Editor (AddTransition)");
            AssetDatabase.SaveAssets();
            return transition;
        }

        public void DeleteNode(FSMState state)
        {
            Undo.RecordObject(this, "FSM Editor (DeleteState)");
            states.Remove(state);

            //AssetDatabase.RemoveObjectFromAsset(state);
            Undo.DestroyObjectImmediate(state);
            AssetDatabase.SaveAssets();
        }
        public void DeleteNode(FSMTransition transition)
        {
            Undo.RecordObject(this, "FSM Editor (DeleteTransition)");
            transitions.Remove(transition);

            //AssetDatabase.RemoveObjectFromAsset(transition);

            Undo.DestroyObjectImmediate(transition);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(FSMState input, FSMTransition output)
        {
            Undo.RecordObject(input, "FSM Editor (AddChild)");
            input.transitionList.Add(output);
            EditorUtility.SetDirty(input);
        }

        public void AddChild(FSMRoot input, FSMState output)
        {
            Undo.RecordObject(input, "FSM Editor (AddChild)");
            input.startState = output;
            EditorUtility.SetDirty(input);
        }

        public void AddChild(FSMTransition input, FSMState output)
        {
            Undo.RecordObject(input, "FSM Editor (AddChild)");
            input.nextState = output;
            EditorUtility.SetDirty(input);
        }

        public void RemoveChild(FSMState input, FSMTransition output)
        {
            Undo.RecordObject(input, "FSM Editor (RemoveChild)");
            input.transitionList.Remove(output);
            EditorUtility.SetDirty(input);
        }

        public void RemoveChild(FSMTransition input, FSMState output)
        {
            Undo.RecordObject(input, "FSM Editor (RemoveChild)");
            input.nextState = null;
            EditorUtility.SetDirty(input);
        }

        public void RemoveChild(FSMRoot input, FSMState output)
        {
            Undo.RecordObject(input, "FSM Editor (RemoveChild)");
            input.startState = null;
            EditorUtility.SetDirty(input);
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

        public FSMState GetCurrentState()
        {
            return m_currentState;
        }
#endif
    }
}