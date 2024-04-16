/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Blackboard
{
    [HideDataObjectType]
    // [DataObjectOrder(900)]
    
    public class BlackboardGenericVariable<T> : BlackboardVariable, IEquatable<T>
    {

        [ExposeToInspector]
        [SerializeField]
        protected T _value;

        
        public T Value
        {
            get
            {
                if (!isRuntimeInstance)
                {
                    if (runtimeClone == null)
                    {
                        // There's no runtime clone for this blackboard variable (example: PrefabVariable)
                        return _value;
                    }
                    else
                    {
                        return (runtimeClone as BlackboardGenericVariable<T>).Value;
                    }
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                if (!isRuntimeInstance)
                {
                    if (runtimeClone == null)
                    {
                        if (this.Equals(value))
                            return;

                        _value = value;

                        onValueChanged?.Raise(this);
                    }
                    else
                    {
                        (runtimeClone as BlackboardGenericVariable<T>).Value = value;
                    }
                }
                else
                {
                    if (this.Equals(value))
                        return;

                    _value = value;

                    onValueChanged?.Raise(this);
                }
            }
        }
        public virtual bool Equals(T other)
        {
            return EqualityComparer<T>.Default.Equals(this._value, other);
        }
    }
}