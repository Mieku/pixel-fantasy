using System.Collections;
using UnityEngine.UI;
using static InfinityPBR.Modules.Timeboard;

/*
 * This script will subscribe to the Timeboard, and whenever a major time data is changed, receive
 * an event from the Timeboard.Blackboard. This will cause the UI Text value to update. This way,
 * we can always display the correct time, but only make changes when the time actually changes,
 * avoiding an Update() loop.
 *
 * Note, we know the Timeboard.Blackboard will only send events when something changes, so we do
 * not need to check on whether the event is something we care about -- we also don't care what the
 * content is, for this script. Other scripts may only care about one type of data, such as the
 * season, in which case the "Subject" of the event should be checked.
 */

namespace InfinityPBR.Modules.Demo
{
    public class DateTimeUIText : TimeboardFollower
    {
        private Text _text;

        private void Awake() => _text = GetComponent<Text>(); // Cache the reference
        
        // We will override SubscribeToTimeboard, because we also want to do our setup
        // step, which will set the UI Text value.
        protected override IEnumerator SubscribeToTimeboard()
        {
            // If we don't have the reference, wait one frame
            while (timeboard == null)
                yield return null;

            Subscribe();
            
            // If we haven't setup, then set the time to whatever it is now. An event will
            // not be sent at the start.
            if (_setup)
                UpdateText();
            
            _setup = true;
        }
        
        public override void ReceiveEvent(BlackboardEvent blackboardEvent) => UpdateText();
        
        private void UpdateText() => _text.text = timeboard.gametime.FullDate();
    }
}
