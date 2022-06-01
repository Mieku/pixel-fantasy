using System;
using DataPersistence;

namespace Handlers
{
    public class ResourcesHandler : Saveable
    {
        public string tester;

        protected override string StateName => "Resources";

        protected override object CaptureState()
        {
            return new Data()
            {
                newTester = tester
            };
        }

        protected override void RestoreState(object stateData)
        {
            var data = (Data)stateData;
            tester = data.newTester;
        }
        

        public struct Data
        {
            public string newTester;
        }
    }
}
