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
    public class WirePuzzleManager : MonoBehaviour
    {
        [Header("Wire Configuration")]
        [SerializeField] private List<WireStretchController> leftWires = new List<WireStretchController>();
        [SerializeField] private List<WireStretchController> rightWires = new List<WireStretchController>();
        [SerializeField] private LayerMask wireLayer;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private float snapDistance = 0.15f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

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
            if (!isActive || isSolved || CameraController.Instance.Camera == null)
                return;

            HandleWireInteraction();
        }

        private void HandleWireInteraction()
        {
            Ray ray = GetInteractionRay();

            // ถ้ายังไม่ได้จับสายไว้
            if (currentWire == null)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, wireLayer))
                {
                    if (Input.GetKeyDown(interactKey) || Input.GetMouseButtonDown(0))
                    {
                        WireStretchController wire = hit.transform.GetComponentInParent<WireStretchController>();
                        if (wire != null && CanInteractWithWire(wire))
                        {
                            StartDraggingWire(wire);
                        }
                    }
                }
                return;
            }

            // กำลังลากสายอยู่
            UpdateWireDrag(ray);

            // ปล่อยสาย
            if (Input.GetKeyUp(interactKey) || Input.GetMouseButtonUp(0))
            {
                ReleaseWire();
            }
        }

        private Ray GetInteractionRay()
        {
            if (useScreenCenter)
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

            Debug.Log($"[WirePuzzle] เริ่มลากสาย: {wire.name}");
        }

        private void UpdateWireDrag(Ray ray)
        {
            // สร้าง Plane ตั้งฉากกับแกน X ผ่านตำแหน่งของสาย
            Plane plane = new Plane(Vector3.right, currentWire.transform.position);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 worldPos = ray.GetPoint(enter);
                currentWire.DragTo(worldPos);
            }
        }

        private void ReleaseWire()
        {
            if (currentWire == null)
                return;

            currentWire.Release();

            // ลองเชื่อมต่อกับสายอีกฝั่ง
            bool connected = TryConnectWire();

            if (!connected)
            {
                OnWireDisconnected?.Invoke();
            }

            // เช็คว่า Puzzle สำเร็จหรือยัง
            CheckPuzzleSolved();

            currentWire = null;
            isLeftWire = false;
            isRightWire = false;
        }

        private bool TryConnectWire()
        {
            List<WireStretchController> targetWires = isLeftWire ? rightWires : (isRightWire ? leftWires : null);

            if (targetWires == null)
                return false;

            foreach (WireStretchController targetWire in targetWires)
            {
                float distance = (currentWire.Tip.position - targetWire.Tip.position).magnitude;

                if (distance < snapDistance)
                {
                    currentWire.ConnectTo(targetWire);
                    OnWireConnected?.Invoke();
                    Debug.Log($"[WirePuzzle] เชื่อมต่อสาย: {currentWire.name} <-> {targetWire.name}");
                    return true;
                }
            }

            return false;
        }

        private void CheckPuzzleSolved()
        {
            if (isSolved)
                return;

            int leftConnected = leftWires.Count(wire => wire.IsConnect);
            int rightConnected = rightWires.Count(wire => wire.IsConnect);

            bool allConnected = (leftConnected == leftWires.Count) && (rightConnected == rightWires.Count);

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

            // รีเซ็ตสายทั้งหมด
            foreach (var wire in leftWires)
                wire.SetConnect(false);

            foreach (var wire in rightWires)
                wire.SetConnect(false);

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
            if (CameraController.Instance.Camera != null && useScreenCenter)
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
