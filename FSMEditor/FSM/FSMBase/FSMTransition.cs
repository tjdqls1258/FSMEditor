using UnityEngine;

namespace FSMEditor
{
    public abstract class FSMTransition : ScriptableObject
    {
        #if UNITY_EDITOR
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        #endif

        public FSMState nextState;

        public abstract void Init();
        public abstract bool Excute();
        public virtual FSMState NextState()
        {
            nextState.Enter();
            return nextState;
        }

        public virtual FSMTransition Clone()
        {
            Init();
            return Instantiate(this);
        }
    }
}