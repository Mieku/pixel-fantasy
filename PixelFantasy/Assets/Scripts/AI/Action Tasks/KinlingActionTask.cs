using DataPersistence;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Systems.Game_Setup.Scripts;

namespace AI.Action_Tasks
{
    [Category("Kinling Action Tasks")]
    public class KinlingActionTask : ActionTask
    {
        protected override void OnStop(bool interrupted)
        {
            if (GameManager.Instance.GameIsQuitting) return;
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
