using UnityEngine;

namespace MGJ.Puzzle
{
    /// <summary>
    /// ควบคุมการยืดและหดของสายไฟ
    /// รองรับการลาก, ปล่อย, และเชื่อมต่อกับสายอื่น
    /// </summary>
    public class WireStretchController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform wireMesh;   // Mesh ของสาย (cube)
        [SerializeField] private Transform wireTip;    // จุดปลายสาย (empty transform)
        [SerializeField] private WireStretchController correctWire; // สายที่ต้องต่อด้วย (ถูกต้อง)

        [Header("Wire Properties")]
        [SerializeField] private float minLength = 0.1f;
        [SerializeField] private float maxLength = 1f;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color connectedColor = Color.green;
        [SerializeField] private Color incorrectColor = Color.red;

        [Header("Animation Settings")]
        [SerializeField] private float rotateSpeed = 15f;
        [SerializeField] private float stretchSpeed = 15f;
        [SerializeField] private float returnSpeed = 10f;

        [Header("Visual Feedback")]
        [SerializeField] private bool enableColorFeedback = true;
        [SerializeField] private Renderer wireRenderer;

        public Transform Tip => wireTip;
        public bool IsConnect => isConnected;
        public bool IsCorrectConnection => isConnected && lastConnectedWire == correctWire;

        // Private variables
        private Vector3 targetWorldPos;
        private bool isDragging;
        private float currentLength;
        private Vector3 defaultScale;
        private Vector3 tipStartPos;
        private Vector3 meshLocalStartPos;
        private Quaternion startRotation;
        private bool isConnected;
        private WireStretchController lastConnectedWire;
        private Material wireMaterial;

        private void Awake()
        {
            InitializeWire();
        }

        private void InitializeWire()
        {
            if (wireMesh == null)
            {
                Debug.LogError($"[WireController] {gameObject.name}: wireMesh ไม่ได้ถูกกำหนด!");
                return;
            }

            if (wireTip == null)
            {
                Debug.LogError($"[WireController] {gameObject.name}: wireTip ไม่ได้ถูกกำหนด!");
                return;
            }

            // เก็บค่าเริ่มต้น
            defaultScale = wireMesh.localScale;
            currentLength = defaultScale.x;
            startRotation = transform.localRotation;
            meshLocalStartPos = wireMesh.localPosition;
            tipStartPos = wireTip.position;

            // ตั้งค่า Material
            if (wireRenderer == null)
                wireRenderer = wireMesh.GetComponent<Renderer>();

            if (wireRenderer != null && enableColorFeedback)
            {
                wireMaterial = wireRenderer.material;
                UpdateWireColor(defaultColor);
            }
        }

        private void LateUpdate()
        {
            if (isDragging)
                UpdateWireStretch();
            else
                ReturnToRest();
        }

        #region Dragging Logic

        public void DragTo(Vector3 worldPos)
        {
            // ถ้าลากสายที่เชื่อมต่ออยู่ ให้ตัดการเชื่อมต่อ
            if (isConnected)
            {
                Disconnect();
            }

            targetWorldPos = worldPos;
            isDragging = true;
        }

        public void Release()
        {
            isDragging = false;
        }

        private void UpdateWireStretch()
        {
            Vector3 rootPos = transform.position;
            Vector3 dir = targetWorldPos - rootPos;

            if (dir.sqrMagnitude < 0.0001f)
                return;

            // จำกัดระยะ
            float distance = Mathf.Clamp(dir.magnitude, minLength, maxLength);

            // หมุนสาย
            Quaternion targetRot = Quaternion.LookRotation(dir, transform.up) * Quaternion.Euler(0f, -90f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
            wireMesh.rotation = transform.rotation;

            // ยืดสาย
            currentLength = Mathf.Lerp(currentLength, distance, Time.deltaTime * stretchSpeed);

            Vector3 scale = defaultScale;
            scale.x = currentLength;
            wireMesh.localScale = scale;

            // ขยับ mesh ให้อยู่กึ่งกลาง
            wireMesh.localPosition = meshLocalStartPos + Vector3.right * (currentLength * 0.5f);

            // เลื่อน tip ไปตามปลายสาย (85% เพื่อไม่ให้ติดขอบ)
            wireTip.position = rootPos + dir.normalized * (currentLength * 0.85f);
        }

        private void ReturnToRest()
        {
            // ถ้าเชื่อมต่ออยู่ ให้คงสภาพ
            if (isConnected && lastConnectedWire != null)
            {
                DragTo(lastConnectedWire.transform.position);
                return;
            }

            // กลับสู่สภาพเริ่มต้น
            currentLength = Mathf.Lerp(currentLength, defaultScale.x, Time.deltaTime * returnSpeed);

            Vector3 scale = defaultScale;
            scale.x = currentLength;
            wireMesh.localScale = scale;

            transform.localRotation = Quaternion.Lerp(transform.localRotation, startRotation, Time.deltaTime * returnSpeed);
            wireMesh.localRotation = Quaternion.Lerp(wireMesh.localRotation, startRotation, Time.deltaTime * returnSpeed);

            wireMesh.localPosition = meshLocalStartPos + Vector3.right * (currentLength * 0.5f);
            wireTip.position = Vector3.Lerp(wireTip.position, tipStartPos, Time.deltaTime * returnSpeed);
        }

        #endregion

        #region Connection Logic

        /// <summary>
        /// เชื่อมต่อกับสายอื่น
        /// </summary>
        /// <returns>true = ต่อถูก, false = ต่อผิด</returns>
        public bool ConnectTo(WireStretchController targetWire)
        {
            if (targetWire == null)
            {
                Debug.LogWarning($"[WireController] {gameObject.name}: พยายามเชื่อมต่อกับสายที่เป็น null");
                return false;
            }

            // ลากไปยังตำแหน่งของสายเป้าหมาย
            DragTo(targetWire.transform.position);

            // เช็คว่าต่อถูกหรือไม่
            bool isCorrect = (targetWire == correctWire);

            // ตั้งสถานะการเชื่อมต่อ
            SetConnect(true);
            targetWire.SetConnect(true);

            lastConnectedWire = targetWire;

            // 2.1 ถ้าถูกสายไฟ -> เปลี่ยนสีเป็นสีที่ถูก
            if (isCorrect)
            {
                UpdateWireColor(connectedColor);
                targetWire.UpdateWireColor(connectedColor);
                Debug.Log($"[WireController] ✓ เชื่อมต่อถูกต้อง: {gameObject.name} -> {targetWire.gameObject.name}");
            }
            // 2.2 ถ้าผิดสายไฟ -> เปลี่ยนสีเป็นสีที่ผิด
            else
            {
                UpdateWireColor(incorrectColor);
                targetWire.UpdateWireColor(incorrectColor);
                Debug.Log($"[WireController] ✗ เชื่อมต่อผิด: {gameObject.name} -> {targetWire.gameObject.name}");
            }

            return isCorrect;
        }

        public void SetConnect(bool connected)
        {
            isConnected = connected;

            if (!connected)
            {
                lastConnectedWire = null;
            }
        }

        public void Disconnect()
        {
            if (lastConnectedWire != null)
            {
                lastConnectedWire.SetConnect(false);
                lastConnectedWire.ReturnToDefault();
            }

            SetConnect(false);
            ReturnToDefault();

            Debug.Log($"[WireController] ตัดการเชื่อมต่อ: {gameObject.name}");
        }

        /// <summary>
        /// 1. ถ้ารอบๆไม่มีสายไฟเลย -> กลับไปที่เดิม พร้อมเปลี่ยนสีเป็นสีปกติ
        /// </summary>
        public void ReturnToDefault()
        {
            SetConnect(false);
            UpdateWireColor(defaultColor);
        }

        #endregion

        #region Visual Feedback

        private void UpdateWireColor(Color color)
        {
            if (!enableColorFeedback || wireMaterial == null)
                return;

            // รองรับทั้ง Standard Shader และ URP
            if (wireMaterial.HasProperty("_Color"))
                wireMaterial.color = color;
            else if (wireMaterial.HasProperty("_BaseColor"))
                wireMaterial.SetColor("_BaseColor", color);
        }

        #endregion

        #region Public Utilities

        public void SetCorrectWire(WireStretchController wire)
        {
            correctWire = wire;
        }

        public WireStretchController GetCorrectWire()
        {
            return correctWire;
        }

        public void SetDefaultColor(Color color)
        {
            defaultColor = color;
            UpdateWireColor(color);
        }

        public Color GetDefaultColor()
        {
            return defaultColor;
        }

        public float GetCurrentLength()
        {
            return currentLength;
        }

        public float GetStretchPercentage()
        {
            return (currentLength - minLength) / (maxLength - minLength) * 100f;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (wireTip == null) return;

            // แสดงจุดปลายสาย
            Gizmos.color = isConnected ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(wireTip.position, 0.05f);
        }

        private void OnDrawGizmosSelected()
        {
            if (correctWire != null && wireTip != null)
            {
                // แสดงเส้นเชื่อมไปยังสายที่ถูกต้อง
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(wireTip.position, correctWire.Tip.position);
            }
        }

        #endregion
    }
}
