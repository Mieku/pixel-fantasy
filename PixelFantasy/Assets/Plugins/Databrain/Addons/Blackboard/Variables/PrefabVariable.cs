/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using UnityEngine;

namespace Databrain.Blackboard
{
    [DataObjectIcon("category", DatabrainColor.Black)]
    [DataObjectTypeName("Prefabs")]
    public class PrefabVariable : BlackboardGenericVariable<GameObject>{}
}
