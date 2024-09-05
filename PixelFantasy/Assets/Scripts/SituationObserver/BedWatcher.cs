using System.Collections.Generic;
using Characters;
using Managers;
using Systems.Notifications.Scripts;

namespace SituationObserver
{
    public class BedWatcher : ISituationWatcher
    {
        private Dictionary<string, string> _currentNotifications = new Dictionary<string, string>();

        public void CheckSituation()
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

            HandleNotification(bedless);
        }

        private void HandleNotification(List<KinlingData> bedless)
        {
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