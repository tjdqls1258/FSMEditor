using UnityEngine;

namespace FSMEditor
{
    public class FSMRoot : ScriptableObject
    {
#if UNITY_EDITOR
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
#endif

        public FSMState startState;


        public FSMState StartRoot()
        {
            return startState;
        }

        public virtual FSMRoot Clone()
        {
            var state = Instantiate(this);
            state.startState = startState.Clone();
            return state;
        }
    }
}