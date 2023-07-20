using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Utils;
using Utils.Singleton;
using mixpanel;

namespace Utils
{
    public class AnalyticUtils : Singleton<AnalyticUtils>
    {
        public void Initialize()
        {
            Mixpanel.Identify(SystemInfo.deviceUniqueIdentifier);
            Mixpanel.People.Increment("num_of_sessions", 1);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Mixpanel.Flush();
            }
        }

        private void OnApplicationQuit()
        {
            Mixpanel.Flush();
        }
    }
}