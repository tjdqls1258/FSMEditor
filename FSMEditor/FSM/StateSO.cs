using System;
using UnityEngine;

namespace Util_Patten.FSM
{
    public class StateSO<T> : ScriptableObject where T : Context
    {
#if UNITY_EDITOR
        public Vector2 nodePosition;
#endif

        public ActionSO<T>[] actions;
        public Transition<T>[] transitions;

        public virtual void OnEnter(T context) 
        {
            foreach (var action in actions)
            {
                action.OnEnter(context);
            }
        }

        public StateSO<T> UpdateState(T context)
        {
            foreach (var action in actions)
            {
                action.OnUpdate(context);
            }

            foreach (var transition in transitions)
            {
                if (transition.condition.Evaluate(context) && transition.trueState != null)
                    return transition.trueState;
                else if (transition.falseState != null) //실패시 상태
                    return transition.falseState;
            }

            return this;
        }

        public void OnExit(T context)
        {
            foreach(var action in actions) action.OnExit(context);
        }
    }

    [Serializable]
    public class Transition<T> where T : Context
    {
        public ConditionSO<T> condition;
        public StateSO<T> trueState;
        public StateSO<T> falseState;
    }
}