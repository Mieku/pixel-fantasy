using System.Collections.Generic;
using NodeCanvas.Framework;

namespace AI.Action_Tasks
{
    public class SelectFirstFromList : KinlingActionTask
    {
        public BBParameter<List<string>> StringList;
        public BBParameter<string> ResultString;
    
        protected override void OnExecute()
        {
            // Check if the list is not null and has at least one element
            if (StringList.value != null && StringList.value.Count > 0)
            {
                // Get the first string in the list
                ResultString.value = StringList.value[0];

                // Set the task as successful
                EndAction(true);
            }
            else
            {
                // If the list is null or empty, set the task as failed
                EndAction(false);
            }
        }
    }
}
