using System;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace TeamFlow.Nodes
{
    /// <summary>
    /// 基础其它类型节点
    /// </summary>
    [NodeTitle("","",SdfIconType.None,false)]
    public abstract class BaseOtherNode:Node
    {
        [SerializeField]
        [HideInInspector]
        protected string mGUID="";

        protected override void Init()
        {
            base.Init();
            if(mGUID!="")  mGUID = Guid.NewGuid().ToString();
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
           
        }

        public abstract override object GetValue(NodePort port);
    }
}