using UnityEngine;

namespace FSMEditor
{
    public class DebugState : FSMState
    {
        public string message;
        public override void Enter()
        {
            base.Enter();
            Debug.Log($"Enter {message}");
        }

        public override FSMState Excute()
        {
            //Debug.Log($"Excute {message}");
            return base.Excute();

        }

        public override void Exit()
        {
            Debug.Log($"Exit {message}");
        }
    }
}