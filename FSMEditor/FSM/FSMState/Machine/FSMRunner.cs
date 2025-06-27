using UnityEngine;

namespace FSMEditor
{
    public class FSMRunner : MonoBehaviour
    {
        [SerializeField] FSMachine machine;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            machine = machine.Clone();
            machine.StartMachine();
        }

        // Update is called once per frame
        void Update()
        {
            machine.Update();
        }
    }
}