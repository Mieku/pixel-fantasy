namespace InfinityPBR.Modules
{
    public interface ISaveable
    {
        /*
         * Save and Load state will be unique for each class they are handling, and is something you get to write
         * for yourself, so that you get exactly what you want saved.
         */
        public object SaveState();
        public void LoadState(string jsonEncodedState);

        /*
         * Note: SaveableObjectId needs to be unique on the object which has Savable script on it. It does not need
         * to be random, but it can be, so long as it is the same all the time. If it is randomly generated ensure this
         * happens at edit time, not runtime, and is then the same, from that point on.
         */
        public string SaveableObjectId();
        
        /*
         * Pre and post save/load actions are optional, and can be activated when the blackboardEvent is received, if
         * you object allow follow the MainBlackboard. See code example below.
         */
        public void PreSaveActions();
        public void PostSaveActions();

        public void PreLoadActions();
        public void PostLoadActions();
    }
}

/*
    // Example of how to utilize the PreSave and PostSaveActions with the ReceiveEvent method from the Blackboard.
    public override void ReceiveEvent(BlackboardEvent blackboardEvent)
        {
            // Listen for the "Save Started" event for "Save and Load", "Game Data", "Save Start"
            if (blackboardEvent.topic == "Save and Load" 
                && blackboardEvent.gameId == "Game Data" 
                && blackboardEvent.status == "Save Start" 
                && (string)blackboardEvent.obj == Uid())
            {
                PreSaveActions();
            }
            
            if (blackboardEvent.topic == "Save and Load" 
                && blackboardEvent.gameId == "Game Data" 
                && blackboardEvent.status == "Save End" 
                && (string)blackboardEvent.obj == Uid())
            {
                PostSaveActions();
            }
        }
*/