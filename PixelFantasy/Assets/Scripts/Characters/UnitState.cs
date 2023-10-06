using System.Collections.Generic;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using Zones;

namespace Characters
{
    public class UnitState : MonoBehaviour
    {
        [SerializeField] private UID _uid;
        
        public string FirstName, LastName;
        public Abilities Abilities;
        public TaskPriorities Priorities;
        
        public Schedule Schedule = new Schedule();
        public Building AssignedHome;
        public Building AssignedWorkplace;
        
        private List<JobState> _jobHistory = new List<JobState>();

        public string FullName => FirstName + " " + LastName;
        public string JobName => CurrentJob.JobNameWithTitle;
        public string UID => _uid.uniqueID;
        
        public void SetLoadData(UnitStateData data)
        {
            FirstName = data.FirstName;
            LastName = data.LastName;
        }

        public UnitStateData GetStateData()
        {
            return new UnitStateData
            {
                FirstName = FirstName,
                LastName = LastName,
            };
        }
        
        public struct UnitStateData
        {
            public string FirstName, LastName;
        }

        public JobState CurrentJob
        {
            get
            {
                foreach (var job in _jobHistory)
                {
                    if (job.IsCurrentJob)
                    {
                        return job;
                    }
                }
                
                // Create a Worker Job State and return it
                var workerData = Librarian.Instance.GetJob("Worker");
                JobState workerJobState = new JobState(workerData, 0, true);
                return workerJobState;
            }
        }

        public void ChangeJob(JobData newJob)
        {
            // Unassign currentJob
            CurrentJob.IsCurrentJob = false;

            // Check if the job already is in history
            foreach (var job in _jobHistory)
            {
                if (job.JobData == newJob)
                {
                    job.IsCurrentJob = true;
                    return;
                }
            }

            // If doesn't already exist, create new state
            JobState newJobState = new JobState(newJob, 0, true);
            if (newJob.JobName != "Worker")
            {
                _jobHistory.Add(newJobState);
            }

            if (newJob.RequiredTool != null)
            {
                var claimedTool = InventoryManager.Instance.ClaimItemGlobal(newJob.RequiredTool);
                if (claimedTool != null)
                {
                    var unit = GetComponent<Unit>();
                    unit.Equipment.AssignDesiredEquipment(claimedTool.State as GearState);
                }
            }
        }

        public List<JobState> JobHistory => _jobHistory;
    }
}
