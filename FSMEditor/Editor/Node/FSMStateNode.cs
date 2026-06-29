using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Util_Patten.FSM.Editor
{
    public class FSMStateNode : Node
    {
        public string GUID;
        public string StateName;
        public Type TargetContextType;

        public ScriptableObject BoundSO;
        public Port inputPort;

        public List<ObjectField> actionFields = new List<ObjectField>();

        public class TransitionData
        {
            public ObjectField conditionField;
            public Port truePort;
            public Port falsePort;
        }
        public List<TransitionData> transitions = new List<TransitionData>();

        public FSMStateNode(string nodeName, Vector2 position, Type targetContextType)
        {
            this.TargetContextType = targetContextType; 
            title = $"{nodeName} ({targetContextType.Name})"; 
            StateName = nodeName;
            GUID = Guid.NewGuid().ToString();

            SetPosition(new Rect(position, new Vector2(250, 200)));

            inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "Enter";
            inputContainer.Add(inputPort);

            Label actionLabel = new Label("Actions");
            actionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            extensionContainer.Add(actionLabel);

            Button addActionBtn = new Button(() => AddActionField()) { text = "+ Add Action" };
            extensionContainer.Add(addActionBtn);

            Label transLabel = new Label("Transitions");
            transLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            transLabel.style.marginTop = 10;
            extensionContainer.Add(transLabel);

            Button addTransitionBtn = new Button(() => AddTransitionPort()) { text = "+ Add Transition" };
            extensionContainer.Add(addTransitionBtn);

            expanded = true;
            RefreshExpandedState();
            RefreshPorts();
        }

        public void AddActionField()
        {
            Type actionBaseType = typeof(ActionSO<>).MakeGenericType(TargetContextType);

            ObjectField objField = new ObjectField
            {
                objectType = actionBaseType,
                allowSceneObjects = false
            };
            actionFields.Add(objField);
            extensionContainer.Insert(extensionContainer.IndexOf(extensionContainer.Q<Button>()), objField);
            RefreshExpandedState();
        }

        public void AddTransitionPort()
        {
            var transData = new TransitionData();

            var container = new VisualElement();
            container.style.marginTop = 10;
            container.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 0.5f); 
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;
            container.style.paddingBottom = 5;

            Type conditionBaseType = typeof(ConditionSO<>).MakeGenericType(TargetContextType);
            ObjectField condField = new ObjectField("Condition")
            {
                objectType = conditionBaseType,
                allowSceneObjects = false
            };
            transData.conditionField = condField;
            container.Add(condField);

            var portContainer = new VisualElement();
            portContainer.style.flexDirection = FlexDirection.Column;
            portContainer.style.justifyContent = Justify.FlexEnd; 

            transData.truePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            transData.truePort.portName = "True";
            transData.truePort.portColor = Color.green;
            portContainer.Add(transData.truePort);

            transData.falsePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            transData.falsePort.portName = "False";
            transData.falsePort.portColor = Color.red;
            portContainer.Add(transData.falsePort);

            container.Add(portContainer);
            extensionContainer.Add(container);

            transitions.Add(transData);

            RefreshExpandedState();
            RefreshPorts();
        }

        public void InitializeFromSO(ScriptableObject so)
        {
            var serialized = new SerializedObject(so);

            var actionProp = serialized.FindProperty("actions");
            for (int i = 0; i < actionProp.arraySize; i++)
            {
                AddActionField();
                var actionValue = actionProp.GetArrayElementAtIndex(i).objectReferenceValue;
                actionFields.Last().value = actionValue;
            }

            var transProp = serialized.FindProperty("transitions");
            for (int i = 0; i < transProp.arraySize; i++)
            {
                AddTransitionPort(); 
                var transElement = transProp.GetArrayElementAtIndex(i);

                transitions.Last().conditionField.value = transElement.FindPropertyRelative("condition").objectReferenceValue;
            }
        }
    }
}