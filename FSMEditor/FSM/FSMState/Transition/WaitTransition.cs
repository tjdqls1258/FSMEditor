using UnityEngine;
namespace FSMEditor
{
    public class WaitTransition : FSMTransition
    {
        public float duration = 1f;
        private float currentTime = 0;

        public override void Init()
        {
            currentTime = Time.time + duration;
        }

        public override bool Excute()
        {
            if(Time.time > currentTime)
                return true;
            else 
                return false;
        }
    }
}