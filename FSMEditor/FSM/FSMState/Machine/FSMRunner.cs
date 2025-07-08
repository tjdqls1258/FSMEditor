using UnityEngine;

namespace FSMEditor
{
    public class FSMRunner : MonoBehaviour //상속 받아서 사용
    {
        public FSMachine machine;

        public virtual void InitFSM(params object[] data)
        {
            machine = machine.Clone();
            machine.Bind(data);
        }
        public virtual void StartFSM()
        {
            machine.StartMachine();
        }

        protected virtual void Update()
        {
            machine.Update();
        }
    }
}