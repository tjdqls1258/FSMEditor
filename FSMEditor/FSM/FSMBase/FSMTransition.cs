using UnityEngine;

namespace FSMEditor
{
    public abstract class FSMTransition : ScriptableObject
    {
        #if UNITY_EDITOR
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [TextArea] public string description;
#endif

        [HideInInspector] public FSMState nextState;
        [HideInInspector] public BlackBoard blackBoard;
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