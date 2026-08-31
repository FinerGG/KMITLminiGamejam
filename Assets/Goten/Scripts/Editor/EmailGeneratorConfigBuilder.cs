using UnityEngine;
using UnityEditor;

namespace MGJ.Editor
{
    public class EmailGeneratorConfigBuilder : EditorWindow
    {
        [MenuItem("MGJ/Create Email Generator Config")]
        public static void CreateConfig()
        {
            EmailGeneratorConfig config = ScriptableObject.CreateInstance<EmailGeneratorConfig>();

            // Title Components
            config.titlePrefixes.AddRange(new string[]
            {
                "Night shift", "Morning", "Security", "System", "Network", "Camera",
                "Evening", "Weekly", "Daily", "Incident", "Service", "Update"
            });

            config.titleTypes.AddRange(new string[]
            {
                "handover", "alert", "report", "update", "service", "token",
                "codec", "sync", "diagnostic", "maintenance", "backup", "patch"
            });

            config.titleSuffixes.AddRange(new string[]
            {
                "#1842", "#2156", "#0094", "#7721", "#9383",
                ".sys", ".dll", ".exe", "_v2", "_final", "_backup"
            });

            // Safe Sources
            config.safeSources.AddRange(new string[]
            {
                "SOC Lead",
                "IT Admin",
                "Security Team",
                "Internal VLAN / CCTV-CORE",
                "Network Operations",
                "System Administrator",
                "DevOps Team",
                "Security Operations Center"
            });

            // Suspicious Sources
            config.suspiciousSources.AddRange(new string[]
            {
                "Unknown sender",
                "External relay",
                "CCTV Server / System32",
                "Unverified domain",
                "External IP / 203.45.67.89",
                "Temporary service account",
                "Expired certificate domain"
            });

            // Safe Captured Data
            config.safeCapturedData.AddRange(new string[]
            {
                "สรุปเหตุการณ์ละเอียดแบบอยู่ในระบบ Ticket ภายใน หมายเลข INC-{0}",
                "Service: cam.sync ∙ Token age: {0} min ∙ Policy: SV-12",
                "SHA-256: {0} ∙ Signed: Aegis Systems ∙ Modified: {1} days ago",
                "Scheduled maintenance window ∙ Approved by IT Ops ∙ Ticket: MNT-{0}",
                "Configuration backup ∙ Version: {0} ∙ Verified checksum",
                "Service token renewal ∙ Expiry: {0} hours ∙ Auto-generated"
            });

            // Virus Captured Data
            config.virusCapturedData.AddRange(new string[]
            {
                "SHA-256: {0} ∙ Signed: Unknown ∙ Modified: {1} hours ago",
                "Executable attachment detected ∙ Hash unknown ∙ No signature",
                "Service token expired ∙ Source unverified ∙ Unusual request pattern",
                "Unrecognized binary ∙ Origin: external relay ∙ Suspicious behavior",
                "Encrypted payload ∙ No valid certificate ∙ Unknown protocol",
                "Abnormal file size: {0}KB ∙ Packed executable ∙ Hidden sections"
            });

            // Safe Signals
            config.safeSignals.AddRange(new string[]
            {
                "โดเมนภายใน",
                "อ้างอิง Ticket ที่ตรวจสอบได้",
                "เครือข่ายภายใน",
                "สอดคล้องกับ Service Policy",
                "Digital signature verified",
                "ส่งจากแหล่งที่เชื่อถือได้",
                "ตรวจสอบ checksum ถูกต้อง",
                "อยู่ในช่วงเวลาบำรุงรักษา",
                "Approved by Security Team",
                "Certificate valid and trusted"
            });

            // Suspicious Signals
            config.suspiciousSignals.AddRange(new string[]
            {
                "โดเมนภายนอก",
                "ไม่มีการอ้างอิง Ticket",
                "แหล่งที่มาไม่ชัดเจน",
                "ไม่สอดคล้องกับ Policy",
                "No digital signature",
                "Unusual file extension",
                "Token expired",
                "Unverified sender",
                "ส่งนอกเวลางาน",
                "IP address blacklisted",
                "Abnormal traffic pattern",
                "Certificate expired or invalid",
                "Packed or obfuscated code",
                "Known malware hash pattern"
            });

            // Save as asset
            string path = "Assets/ScriptableObjects/EmailGeneratorConfig.asset";

            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"Email Generator Config created at: {path}");

            Selection.activeObject = config;
        }
    }
}
