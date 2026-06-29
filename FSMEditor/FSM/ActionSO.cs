using UnityEngine;

namespace Util_Patten.FSM
{

    public abstract class ActionSO<T> : ScriptableObject where T :Context<T>
    {
        public virtual void OnEnter(T context) { }
        public virtual void OnUpdate(T context) { }
        public virtual void OnExit(T context) { }
    }

    public abstract class ConditionSO<T> : ScriptableObject where T : Context<T>
    {
        public abstract bool Evaluate(T context);
    }
}