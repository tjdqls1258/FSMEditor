using System.Collections.Generic;
using UnityEngine;

namespace Util_Patten.FSM
{
    public interface IContext { }

    public abstract class Context<T> : IContext where T : Context<T>
    {
        public virtual void Init() { }
        public StateSO<T> currentState { get; set; }

        public virtual void PreExecute() { } //업데이트 바로 전 호출
    }

    public static class StateMachine<T> where T : Context<T>
    {
        public static void StartMachine(T context)
        {
            if (context != null && context.currentState != null)
            {
                context.currentState.OnEnter(context);
            }
        }

        public static void UpdateMachine(T context)
        {
            if (context.currentState == null) return;

            var nextState = context.currentState.UpdateState(context);

            if (nextState != context.currentState)
                ForeChangeState(context, nextState);

        }

        public static void ForeChangeState(T context, StateSO<T> state)
        {
            context.currentState?.OnExit(context);
            context.currentState = state;
            context.currentState?.OnEnter(context);
        }
    }
}