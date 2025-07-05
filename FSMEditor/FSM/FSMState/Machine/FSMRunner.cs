using UnityEngine;

namespace FSMEditor
{
    public class FSMRunner : MonoBehaviour
    {
        public FSMachine machine;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            machine = machine.Clone();
            machine.Bind();
            machine.StartMachine();
        }

        // Update is called once per frame
        void Update()
        {
            machine.Update();
        }
    }
}