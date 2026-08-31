using System;
using System.Collections.Generic;
using UnityEngine;

namespace MGJ
{
    [Serializable]
    public class ThreatEmail
    {
        public string emailTitle;
        public string source;
        public string capturedData;
        public List<string> observedSignals;
        public bool isVirus;
        public string caseNumber;

        // Runtime data
        public float timestamp;
        public bool isResolved;

        public ThreatEmail()
        {
            observedSignals = new List<string>();
            timestamp = 0f;
            isResolved = false;
        }
    }
}
