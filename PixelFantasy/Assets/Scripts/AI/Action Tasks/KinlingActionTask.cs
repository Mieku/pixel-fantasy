using DataPersistence;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace AI.Action_Tasks
{
    [Category("Kinling Action Tasks")]
    public class KinlingActionTask : ActionTask
    {
        protected override void OnStop(bool interrupted)
        {
            if(DataPersistenceManager.WorldIsClearing) return;
            OnStopInternal(interrupted);
        }

        /// <summary>
        /// Safe version of OnStop that does not trigger on clearing world
        /// </summary>
        protected virtual void OnStopInternal(bool interrupted)
        {
            
        }
    }
}
