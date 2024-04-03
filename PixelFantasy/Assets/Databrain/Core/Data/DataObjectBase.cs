/*
 *	DATABRAIN
 *	(c) 2024 Giant Grey
 *	www.databrain.cc
 *	
 */
using UnityEngine;

#if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1
using Sirenix.OdinInspector;
#endif

namespace Databrain
{
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1
    public class DataObjectBase : SerializedScriptableObject{}
#else
    public class DataObjectBase : ScriptableObject{}
#endif
}

