using System.Collections.Generic;
using UnityEngine;

namespace MGJ
{
    [CreateAssetMenu(fileName = "EmailGeneratorConfig", menuName = "MGJ/Email Generator Config")]
    public class EmailGeneratorConfig : ScriptableObject
    {
        [Header("Title Components")]
        public List<string> titlePrefixes = new List<string>();
        public List<string> titleTypes = new List<string>();
        public List<string> titleSuffixes = new List<string>();

        [Header("Sources - Safe")]
        public List<string> safeSources = new List<string>();

        [Header("Sources - Suspicious")]
        public List<string> suspiciousSources = new List<string>();

        [Header("Captured Data - Safe")]
        public List<string> safeCapturedData = new List<string>();

        [Header("Captured Data - Virus")]
        public List<string> virusCapturedData = new List<string>();

        [Header("Signals - Safe")]
        public List<string> safeSignals = new List<string>();

        [Header("Signals - Suspicious")]
        public List<string> suspiciousSignals = new List<string>();
    }
}
