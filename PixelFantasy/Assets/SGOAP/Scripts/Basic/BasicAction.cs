namespace SGoap
{
    /// <summary>
    /// A very basic action that is paired with basic Agent data with it.
    /// </summary>
    public abstract class BasicAction : Action, IDataBind<KinlingAgentData>
    {
        public KinlingAgentData AgentData;

        public virtual float StaggerTime => 0;
        public override bool PrePerform() => !Cooldown.Active;

        public override bool PostPerform()
        {
            Cooldown.Run(CooldownTime);
            AgentData.Cooldown.Run(StaggerTime);
            return true;
        }

        public void Bind(KinlingAgentData data)
        {
            AgentData = data;
        }
    }
}