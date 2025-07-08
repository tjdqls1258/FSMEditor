using UnityEngine;

namespace FSMEditor
{
    public class FSMRunner : MonoBehaviour //��� �޾Ƽ� ���
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