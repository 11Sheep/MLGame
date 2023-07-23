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

        public void SendMatchFinishedEvent(string winner, string winReason, int currentRound, int humansWins, int computerWins, int humanPoints, int computerPoints)
        {
            var props = new Value();
            props["winner"] = winner;
            props["winReason"] = winReason;
            props["currentRound"] = currentRound;
            props["currentRound"] = currentRound;
            props["humansWins"] = humansWins;
            props["computerWins"] = computerWins;
            props["humanPoints"] = humanPoints;
            props["computerPoints"] = computerPoints;

            Mixpanel.Track("matchResult", props);
        }
        
        public void SendCompetitionFinishedEvent(string winner)
        {
            var props = new Value();
            props["winner"] = winner;

            Mixpanel.Track("competitionResult", props);
        }

        private void OnApplicationQuit()
        {
            Mixpanel.Flush();
        }
    }
}