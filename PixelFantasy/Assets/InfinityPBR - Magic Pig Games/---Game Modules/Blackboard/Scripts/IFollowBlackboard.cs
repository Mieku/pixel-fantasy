namespace InfinityPBR.Modules
{
    public interface IFollowBlackboard
    {
        void ReceiveChange(BlackboardNote blackboardNote);
        void ReceiveEvent(BlackboardEvent blackboardEvent);
    }
}