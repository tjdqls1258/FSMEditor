using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Util_Patten.FSM.Editor
{
    public class FSMSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private FSMGraphView graphView;
        private EditorWindow window;

        public void Init(FSMGraphView graphView, EditorWindow window)
        {
            this.graphView = graphView;
            this.window = window;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
                
                new SearchTreeGroupEntry(new GUIContent("FSM Nodes"), 1),
                
                new SearchTreeEntry(new GUIContent("State Node"))
                {
                    level = 2,
                    userData = "StateNode" 
                }
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent, context.screenMousePosition - window.position.position);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            switch (SearchTreeEntry.userData)
            {
                case "StateNode":
                    graphView.CreateStateNode("New State", graphMousePosition);
                    return true;
            }
            return false;
        }
    }
}