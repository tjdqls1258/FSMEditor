using System;
using System.Collections.Generic;
using UnityEngine;


namespace Util_Patten.FSM
{
    public class FSMManager : MonoBehaviour
    {
        private interface IContextRunner
        {
            public void RunAll();
        }

        private class ContextRunner<T> : IContextRunner where T : Context<T>
        {
            private readonly List<T> m_contextList = new List<T>();

            public void AddContext(T context)
            {
                m_contextList.Add(context);
                StateMachine<T>.StartMachine(context);
            }

            public void RemoveContext(T context) => m_contextList.Remove(context);

            public void RunAll()
            {
                foreach (var context in m_contextList)
                {
                    context.PreExecute();
                    StateMachine<T>.UpdateMachine(context);
                }
            }
        }

        private readonly Dictionary<Type, IContextRunner> m_contextRunners = new();

        private void Update()
        {
            foreach (var context in m_contextRunners.Values)
                context.RunAll();
        }

        public void UpdateContextSingle<T>(T context) where T : Context<T>
        {
            StateMachine<T>.UpdateMachine(context);
        }

        public void AddContext<T>(T context) where T : Context<T>
        {
            var type = typeof(T);
            if(!m_contextRunners.TryGetValue(type, out var con))
            {
                con = new ContextRunner<T>();
                m_contextRunners.Add(type, con);
            }
            ((ContextRunner<T>)con).AddContext(context);
        }

        public void RemoveContext<T>(T context) where T : Context<T>
        {
            if (!m_contextRunners.TryGetValue(typeof(T), out var con))
                ((ContextRunner<T>)con).RemoveContext(context);
        }
    }
}