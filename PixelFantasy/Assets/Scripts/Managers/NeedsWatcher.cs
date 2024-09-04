using System.Collections.Generic;
using Characters;
using NUnit.Framework.Internal;
using Systems.Notifications.Scripts;
using UnityEngine;

namespace Managers
{
    public class NeedsWatcher : Singleton<NeedsWatcher>
    {
        
        private Dictionary<string, string> _currentNotifications = new Dictionary<string, string>();
        
        private void Start()
        {
            GameEvents.MinuteTick += OnTick;
        }

        private void OnDestroy()
        {
            GameEvents.MinuteTick -= OnTick;
        }

        private void OnTick()
        {
            CheckBeds();
        }

        private void CheckBeds()
        {
            List<KinlingData> bedless = new List<KinlingData>();
            var kinlings = KinlingsDatabase.Instance.GetKinlingsDataList();
            foreach (var kinling in kinlings)
            {
                if (kinling.AssignedBed == null)
                {
                    bedless.Add(kinling);
                }
            }

            if (_currentNotifications.ContainsKey("MissingBed"))
            {
                if (bedless.Count == 0)
                {
                    var notificationID = _currentNotifications["MissingBed"];
                    NotificationManager.Instance.ClearNotification(notificationID);
                    _currentNotifications.Remove("MissingBed");
                }
                else
                {
                    var notificationID = _currentNotifications["MissingBed"];
                    string msg = "Kinlings are missing beds:\n";
                    foreach (var kinling in bedless)
                    {
                        msg += "\n-" + kinling.Nickname;
                    }
                    
                    NotificationManager.Instance.UpdateNotification(notificationID, ENotificationType.Warning, "Bedless Kinlings", msg);
                }
            }
            else
            {
                if (bedless.Count > 0)
                {
                    string msg = "Kinlings are missing beds:\n";
                    foreach (var kinling in bedless)
                    {
                        msg += "\n-" + kinling.Nickname;
                    }

                    var data =
                        NotificationManager.Instance.CreateNotification(ENotificationType.Warning, "Bedless Kinlings",
                            msg);
                    
                    _currentNotifications.Add("MissingBed", data.UniqueID);
                }
            }
        }
    }
}
