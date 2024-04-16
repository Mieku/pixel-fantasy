/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using Databrain.Events;

namespace Databrain.Blackboard
{
    [HideDataObjectType]
    [DataObjectOrder(900)]
    [DataObjectFirstClassType("Blackboard", "edit", DatabrainColor.Black)]
    public class BlackboardVariable : DataObject
    {
        [DataObjectDropdown]
        public BlackboardEvent onValueChanged;
    }
}