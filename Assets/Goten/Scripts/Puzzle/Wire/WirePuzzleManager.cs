using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MGJ.Puzzle
{
    /// <summary>
    /// จัดการระบบ Puzzle ต่อสายไฟ
    /// รองรับการลากสายจากซ้ายไปขวา หรือขวาไปซ้าย
    /// เมื่อต่อถูกทั้งหมดจะทริกเกอร์ OnPuzzleSolved event
    /// </summary>
    public class WirePuzzleManager : Singleton<WirePuzzleManager>
    {
        [Header("Wire Configuration")]
        [SerializeField] private List<WireStretchController> leftWires = new List<WireStretchController>();
        [SerializeField] private List<WireStretchController> rightWires = new List<WireStretchController>();
        [SerializeField] private LayerMask wireLayer;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private float snapDistance = 0.15f;

        [Header("Camera Settings")]
        [SerializeField] private bool useScreenCenter = true;
        [SerializeField] private Vector2 screenCenterOffset = new Vector2(0.5f, 0.5f);

        [Header("Puzzle State")]
        [SerializeField] private bool startActive = false;
        [SerializeField] private bool lockOnSolve = true;

        [Header("Randomization")]
        [SerializeField] private bool randomizeOnStart = true;
        [SerializeField] private bool shuffleWirePositions = true;
        [SerializeField] private List<Color> availableColors = new List<Color>()
        {
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.5f, 0f, 1f), // Purple
            new Color(1f, 0.75f, 0.8f), // Pink
            new Color(0.5f, 0.5f, 0.5f) // Gray
        };

        [Header("Events")]
        public UnityEvent OnPuzzleSolved;
        public UnityEvent OnWireConnected;
        public UnityEvent OnWireDisconnected;

        private bool isActive = false;
        private bool isSolved = false;
        private WireStretchController currentWire;
        private bool isLeftWire = false;
        private bool isRightWire = false;
        private Plane dragPlane;
        private Vector3 lastDragWorldPosition;
        private bool hasDragWorldPosition;

        private void Awake()
        {
            isActive = startActive;

            // Validate wire lists
            ValidateWires();

            // สุ่มสีและตำแหน่ง
            if (randomizeOnStart)
            {
                RandomizePuzzle();
            }
        }

        private void ValidateWires()
        {
            leftWires.RemoveAll(wire => wire == null);
            rightWires.RemoveAll(wire => wire == null);

            if (leftWires.Count == 0 || rightWires.Count == 0)
                Debug.LogWarning($"[WirePuzzleManager] {gameObject.name}: มีสายไฟไม่เพียงพอ! ควรมีอย่างน้อยฝั่งละ 1 เส้น");
        }

        private void Update()
        {
            if (!isActive || isSolved || CameraController.Instance == null || CameraController.Instance.Camera == null)
                return;

            HandleWireInteraction();
        }

        private void HandleWireInteraction()
        {
            Ray ray = GetInteractionRay();

            // ถ้ายังไม่ได้จับสายไว้
            if (currentWire == null)
            {
                WireStretchController wire = FindWireUnderRay(ray);
                if (wire != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        StartDraggingWire(wire);
                    }
                }
                return;
            }

            // กำลังลากสายอยู่
            UpdateWireDrag(ray);

            // ปล่อยสาย
            if (Input.GetMouseButtonUp(0))
            {
                ReleaseWire(ray);
            }
        }

        private Ray GetInteractionRay()
        {
            // ใช้จุดกลางเฉพาะตอนล็อกเมาส์ (เช่น controller/crosshair)
            // เมื่อปลดล็อกเมาส์ใน puzzle ให้ ray ตามตำแหน่ง cursor จริงเสมอ
            if (useScreenCenter && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector3 screenPoint = new Vector3(
                    Screen.width * screenCenterOffset.x,
                    Screen.height * screenCenterOffset.y,
                    0f
                );
                return CameraController.Instance.Camera.ScreenPointToRay(screenPoint);
            }
            else
            {
                return CameraController.Instance.Camera.ScreenPointToRay(Input.mousePosition);
            }
        }

        private bool CanInteractWithWire(WireStretchController wire)
        {
            // เช็คว่าสายนี้อยู่ในรายการหรือไม่
            return leftWires.Contains(wire) || rightWires.Contains(wire);
        }

        private void StartDraggingWire(WireStretchController wire)
        {
            currentWire = wire;
            isLeftWire = leftWires.Contains(wire);
            isRightWire = rightWires.Contains(wire);
            dragPlane = new Plane(Vector3.right, wire.transform.position);
            hasDragWorldPosition = false;
            wire.BeginDrag();

            Debug.Log($"[WirePuzzle] เริ่มลากสาย: {wire.name}");
        }

        private void UpdateWireDrag(Ray ray)
        {
            if (dragPlane.Raycast(ray, out float enter) && enter >= 0f)
            {
                Vector3 worldPos = ray.GetPoint(enter);
                lastDragWorldPosition = worldPos;
                hasDragWorldPosition = true;
                currentWire.DragTo(worldPos);
            }
        }

        private void ReleaseWire(Ray releaseRay)
        {
            if (currentWire == null)
                return;

            currentWire.Release();

            // ลองเชื่อมต่อกับสายอีกฝั่ง
            bool connected = TryConnectWire(releaseRay);

            if (!connected)
            {
                // ไม่มีสายไฟรอบๆ -> กลับไปที่เดิม พร้อมเปลี่ยนสีเป็นสีปกติ
                currentWire.ReturnToDefault();
                OnWireDisconnected?.Invoke();
            }

            // เช็คว่า Puzzle สำเร็จหรือยัง
            CheckPuzzleSolved();

            currentWire = null;
            isLeftWire = false;
            isRightWire = false;
            hasDragWorldPosition = false;
        }

        private bool TryConnectWire(Ray releaseRay)
        {
            List<WireStretchController> targetWires = isLeftWire ? rightWires : (isRightWire ? leftWires : null);

            if (targetWires == null)
                return false;

            // เลือกสายที่ ray ชี้โดนก่อน ระยะ world-space อย่างเดียวไม่เสถียร
            WireStretchController closestWire = FindTargetUnderRay(releaseRay, targetWires);

            // ถ้าไม่มี collider ที่ ray โดน ให้ใช้ตำแหน่งปล่อยล่าสุดเป็น fallback
            // เพื่อไม่ใช้ Tip ที่ animation ยังอัปเดตไม่ทันในเฟรมปล่อย
            if (closestWire == null)
            {
                Vector3 releasePosition = hasDragWorldPosition
                    ? lastDragWorldPosition
                    : currentWire.Tip.position;
                float closestDistanceSqr = snapDistance * snapDistance;

                foreach (WireStretchController targetWire in targetWires)
                {
                    if (targetWire == null || targetWire == currentWire)
                        continue;

                    float distanceSqr = (targetWire.Tip.position - releasePosition).sqrMagnitude;
                    if (distanceSqr <= closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        closestWire = targetWire;
                    }
                }
            }

            // ถ้าพบสายที่ใกล้พอ
            if (closestWire != null)
            {
                bool isCorrect = currentWire.ConnectTo(closestWire);
                OnWireConnected?.Invoke();

                Debug.Log(isCorrect
                    ? $"[WirePuzzle] ✓ เชื่อมต่อถูกต้อง: {currentWire.name} <-> {closestWire.name}"
                    : $"[WirePuzzle] ✗ เชื่อมต่อผิด: {currentWire.name} <-> {closestWire.name}");

                return true;
            }

            // ไม่พบสายรอบๆ
            return false;
        }

        private WireStretchController FindTargetUnderRay(Ray ray, List<WireStretchController> targetWires)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, wireLayer, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
                return null;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                WireStretchController candidate = hit.transform.GetComponentInParent<WireStretchController>();
                if (candidate != null && candidate != currentWire && targetWires.Contains(candidate))
                    return candidate;
            }

            return null;
        }

        private WireStretchController FindWireUnderRay(Ray ray)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, wireLayer, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
                return null;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
                WireStretchController candidate = hit.transform.GetComponentInParent<WireStretchController>();
                if (candidate != null && CanInteractWithWire(candidate))
                    return candidate;
            }

            return null;
        }

        private void CheckPuzzleSolved()
        {
            if (isSolved)
                return;

            bool allConnected = leftWires.Count > 0 && rightWires.Count == leftWires.Count &&
                leftWires.All(wire => wire != null && wire.IsCorrectConnection);

            if (allConnected)
            {
                isSolved = true;
                OnPuzzleSolved?.Invoke();
                Debug.Log("[WirePuzzle] ✓ Puzzle สำเร็จ!");

                if (lockOnSolve)
                    isActive = false;
            }
        }

        #region Public Methods

        public void ActivatePuzzle()
        {
            if (!isSolved)
            {
                isActive = true;
                Debug.Log("[WirePuzzle] เปิดใช้งาน Puzzle");
            }
        }

        public void DeactivatePuzzle()
        {
            isActive = false;

            if (currentWire != null)
            {
                currentWire.Release();
                currentWire = null;
            }

            Debug.Log("[WirePuzzle] ปิดใช้งาน Puzzle");
        }

        public void ResetPuzzle()
        {
            isSolved = false;
            currentWire = null;
            isLeftWire = false;
            isRightWire = false;
            hasDragWorldPosition = false;

            // รีเซ็ตสายทั้งหมด
            foreach (var wire in leftWires)
            {
                wire.Disconnect();
                wire.ReturnToDefault();
            }

            foreach (var wire in rightWires)
            {
                wire.Disconnect();
                wire.ReturnToDefault();
            }

            // สุ่มใหม่
            if (randomizeOnStart)
            {
                RandomizePuzzle();
            }

            Debug.Log("[WirePuzzle] รีเซ็ต Puzzle");
        }

        public bool IsSolved() => isSolved;
        public bool IsActive() => isActive;

        public int GetConnectedWireCount()
        {
            return leftWires.Count(wire => wire.IsConnect) + rightWires.Count(wire => wire.IsConnect);
        }

        public float GetCompletionPercentage()
        {
            int totalWires = leftWires.Count + rightWires.Count;
            if (totalWires == 0) return 0f;

            return (float)GetConnectedWireCount() / totalWires * 100f;
        }

        #endregion

        #region Randomization

        /// <summary>
        /// สุ่มสีและตำแหน่งของสายไฟ
        /// </summary>
        private void RandomizePuzzle()
        {
            if (leftWires.Count == 0 || rightWires.Count == 0)
            {
                Debug.LogWarning("[WirePuzzle] ไม่สามารถสุ่มได้ เพราะไม่มีสายไฟ");
                return;
            }

            // สุ่มตำแหน่งสายก่อน (ถ้าเปิดใช้งาน)
            if (shuffleWirePositions)
            {
                ShuffleWirePositions();
            }

            // สุ่มสีและกำหนดคู่ที่ถูกต้อง
            AssignRandomColors();

            Debug.Log("[WirePuzzle] สุ่ม Puzzle เรียบร้อย");
        }

        /// <summary>
        /// สุ่มสลับตำแหน่งสายทั้งสองฝั่ง
        /// </summary>
        private void ShuffleWirePositions()
        {
            // เก็บตำแหน่งเดิม
            List<Vector3> leftPositions = leftWires.Select(w => w.transform.position).ToList();
            List<Vector3> rightPositions = rightWires.Select(w => w.transform.position).ToList();

            // สุ่มตำแหน่งฝั่งซ้าย
            leftPositions = leftPositions.OrderBy(x => Random.value).ToList();
            for (int i = 0; i < leftWires.Count; i++)
            {
                leftWires[i].transform.position = leftPositions[i];
            }

            // สุ่มตำแหน่งฝั่งขวา
            rightPositions = rightPositions.OrderBy(x => Random.value).ToList();
            for (int i = 0; i < rightWires.Count; i++)
            {
                rightWires[i].transform.position = rightPositions[i];
            }

            // WireStretchController บันทึกจุดพักใน Awake ก่อนการสุ่มตำแหน่ง
            // จึงต้องอัปเดต endpoint หลังย้ายตำแหน่งเสร็จ
            foreach (WireStretchController wire in leftWires)
                wire.RefreshRestPosition();
            foreach (WireStretchController wire in rightWires)
                wire.RefreshRestPosition();
        }

        /// <summary>
        /// สุ่มสีให้สายฝั่งซ้าย และกำหนดสีให้สายฝั่งขวาตามคู่ที่ต้องต่อ
        /// </summary>
        private void AssignRandomColors()
        {
            // ตรวจสอบว่ามีสีเพียงพอหรือไม่
            int maxWires = Mathf.Max(leftWires.Count, rightWires.Count);
            if (availableColors.Count < maxWires)
            {
                Debug.LogWarning($"[WirePuzzle] สีไม่เพียงพอ! ต้องการ {maxWires} สี แต่มีแค่ {availableColors.Count} สี");
                // เติมสีสุ่มเพิ่ม
                while (availableColors.Count < maxWires)
                {
                    availableColors.Add(GetRandomColor());
                }
            }

            // สุ่มสีจาก availableColors (ไม่ซ้ำกัน)
            List<Color> shuffledColors = availableColors.OrderBy(x => Random.value).Take(maxWires).ToList();

            // กำหนดสีให้สายฝั่งซ้ายและเก็บข้อมูลคู่
            for (int i = 0; i < leftWires.Count; i++)
            {
                Color wireColor = shuffledColors[i % shuffledColors.Count];

                // ตั้งสีให้สายฝั่งซ้าย
                leftWires[i].SetDefaultColor(wireColor);

                // หาคู่ที่ถูกต้อง (correctWire) และเปลี่ยนสีให้ตรงกัน
                WireStretchController correctWire = leftWires[i].GetCorrectWire();
                if (correctWire != null)
                {
                    correctWire.SetDefaultColor(wireColor);
                }
                else
                {
                    Debug.LogWarning($"[WirePuzzle] สาย {leftWires[i].name} ไม่มี CorrectWire ที่กำหนด!");
                }
            }
        }

        /// <summary>
        /// สุ่มสีที่ไม่ใช่สีเขียวและสีแดง
        /// </summary>
        private Color GetRandomColor()
        {
            Color randomColor;
            int maxAttempts = 10;
            int attempts = 0;

            do
            {
                randomColor = new Color(
                    Random.value,
                    Random.value,
                    Random.value,
                    1f
                );
                attempts++;
            }
            while (IsColorTooSimilar(randomColor, Color.red, 0.3f) ||
                   IsColorTooSimilar(randomColor, Color.green, 0.3f) &&
                   attempts < maxAttempts);

            return randomColor;
        }

        /// <summary>
        /// เช็คว่าสีใกล้เคียงกันเกินไปหรือไม่
        /// </summary>
        private bool IsColorTooSimilar(Color a, Color b, float threshold)
        {
            float distance = Mathf.Sqrt(
                Mathf.Pow(a.r - b.r, 2) +
                Mathf.Pow(a.g - b.g, 2) +
                Mathf.Pow(a.b - b.b, 2)
            );
            return distance < threshold;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (currentWire != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentWire.Tip.position, snapDistance);
            }

            // แสดงรัศมีการ interact
            if (CameraController.Instance != null && CameraController.Instance.Camera != null && useScreenCenter)
            {
                Ray ray = GetInteractionRay();
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // แสดงการเชื่อมต่อของสาย
            Gizmos.color = Color.yellow;

            if (leftWires != null && rightWires != null)
            {
                foreach (var leftWire in leftWires)
                {
                    if (leftWire == null) continue;

                    foreach (var rightWire in rightWires)
                    {
                        if (rightWire == null) continue;

                        if (leftWire.IsConnect)
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(leftWire.Tip.position, rightWire.Tip.position);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
