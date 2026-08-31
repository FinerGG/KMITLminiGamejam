using System.Collections.Generic;
using UnityEngine;

namespace MGJ
{
    public class EmailGenerator
    {
        private EmailGeneratorConfig config;
        private int caseCounter = 1;

        public EmailGenerator(EmailGeneratorConfig config)
        {
            this.config = config;
        }

        public ThreatEmail GenerateEmail(bool isVirus)
        {
            ThreatEmail email = new ThreatEmail
            {
                isVirus = isVirus,
                caseNumber = $"{caseCounter:D4}",
                timestamp = Time.time,
                isResolved = false
            };

            caseCounter++;

            // Generate title
            email.emailTitle = GenerateTitle();

            // Generate source
            email.source = isVirus
                ? GetRandom(config.suspiciousSources)
                : GetRandom(config.safeSources);

            // Generate captured data
            List<string> dataPool = isVirus ? config.virusCapturedData : config.safeCapturedData;
            string template = GetRandom(dataPool);
            email.capturedData = FormatCapturedData(template);

            // Generate signals (2-4 signals)
            email.observedSignals = GenerateSignals(isVirus);

            return email;
        }

        private string GenerateTitle()
        {
            string prefix = GetRandom(config.titlePrefixes);
            string type = GetRandom(config.titleTypes);
            string suffix = GetRandom(config.titleSuffixes);

            return $"{prefix} {type} {suffix}";
        }

        private string FormatCapturedData(string template)
        {
            // Replace placeholders
            template = template.Replace("{0}", Random.Range(1000, 9999).ToString());
            template = template.Replace("{1}", Random.Range(10, 200).ToString());
            return template;
        }

        private List<string> GenerateSignals(bool isVirus)
        {
            List<string> signals = new List<string>();
            List<string> pool = isVirus ? config.suspiciousSignals : config.safeSignals;

            int count = Random.Range(2, 5); // 2-4 signals

            // Shuffle and take
            List<string> shuffled = new List<string>(pool);
            for (int i = 0; i < shuffled.Count; i++)
            {
                int randomIndex = Random.Range(i, shuffled.Count);
                string temp = shuffled[i];
                shuffled[i] = shuffled[randomIndex];
                shuffled[randomIndex] = temp;
            }

            for (int i = 0; i < count && i < shuffled.Count; i++)
            {
                signals.Add(shuffled[i]);
            }

            return signals;
        }

        private T GetRandom<T>(List<T> list)
        {
            if (list == null || list.Count == 0) return default(T);
            return list[Random.Range(0, list.Count)];
        }
    }
}
