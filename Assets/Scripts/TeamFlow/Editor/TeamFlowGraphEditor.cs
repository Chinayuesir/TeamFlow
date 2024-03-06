using System;
using QFramework;
using TeamFlow.Nodes;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace TeamFlow
{
    [CustomNodeGraphEditor(typeof(TeamFlowGraph))]
    public class TeamFlowGraphEditor:NodeGraphEditor
    {
        public override string GetNodeMenuName(System.Type type) {
            if (type.Namespace != null && type.Namespace.Contains("TeamFlow")) {
                return base.GetNodeMenuName(type);//.Replace("X Node/Examples/Logic Toy/", "");
            } else return null;
        }

        public override Node CreateNode(Type type, Vector2 position)
        {
            Node node = base.CreateNode(type, position);
            var customNodeTitle = type.GetAttribute<Node.NodeTitleAttribute>();
            if (customNodeTitle != null)
            {
                node.name = customNodeTitle.title;
                return node;
            }
            else
            {
                node.name = "未命名";
                return default(Node);
            }
        }

        public override Color GetTypeColor(Type type)
        {
           if(type==typeof(bool))
               return Color.green;
           else if (type == typeof(string))
               return Color.magenta;
           else if(type==typeof(UnitaskPort))
               return new Color(0.18f, 0.8f, 0.443f);
           else  return new Color(0.18f, 0.8f, 0.443f);;
        }
        
        public override Color GetPortColor(NodePort port)
        {
            if(port.ValueType==typeof(bool))
                return Color.green;
            else if (port.ValueType == typeof(string))
                return Color.magenta;
            else if(port.ValueType==typeof(UnitaskPort))
                return new Color(0.18f, 0.8f, 0.443f);
            else  return new Color(0.18f, 0.8f, 0.443f);;
        }
    }
}