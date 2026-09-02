using UnityEngine;

namespace MGJ
{
    /// <summary>
    /// ปุ่มล็อคประตู วางไว้บน LockObject ที่มี Collider
    /// คลิกครั้งแรก = ล็อค (ประตูค่อยๆปิด), คลิกอีกครั้ง = ปลดล็อค (ประตูค่อยๆเปิด)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DoorLockButton : Interactable
    {
        [Header("References")]
        [SerializeField] private DoorController door;

        private void Awake()
        {
            if (door == null)
                door = GetComponentInParent<DoorController>();

            if (door == null)
                Debug.LogError($"[DoorLockButton] {gameObject.name}: ไม่พบ DoorController!");
        }

        private void Update()
        {
            if (!interact)
                return;

            interact = false;

            if (!active || door == null)
                return;

            // กดได้เฉพาะตอนผู้เล่นอยู่ที่ประตูนี้
            if (!door.IsPlayerAtDoor)
                return;

            door.ToggleLock();
        }
    }
}
