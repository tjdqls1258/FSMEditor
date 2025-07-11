using System.Collections.Generic;
using UnityEngine;

namespace FSMEditor
{
    public abstract class FSMState : ScriptableObject
    {
#if UNITY_EDITOR
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [TextArea] public string description;
#endif
        [HideInInspector] public BlackBoard blackboard;
        [HideInInspector] public List<FSMTransition> transitionList = new();
        bool enterDone = false;
        public virtual void Enter()
        {
            enterDone = false;
            //Init transitionList
            foreach(var tr in transitionList)
            {
                tr.Init();
            }
            enterDone = true;
        }
        public virtual FSMState Excute()
        {
            if (enterDone == false) return this;
            foreach (var tr in transitionList)
            {
                if (tr.Excute())
                {
                    Exit();
                    return tr.NextState();
                }
            }
            return this;
        }

        public abstract void Exit();

        public virtual FSMState Clone()
        {
            var state = Instantiate(this);
            state.transitionList = transitionList.ConvertAll(x=>x.Clone());
            return state;
        }

        public virtual void BindingData(params object[] data)
        {
            //Data Binding
        }
    }
}