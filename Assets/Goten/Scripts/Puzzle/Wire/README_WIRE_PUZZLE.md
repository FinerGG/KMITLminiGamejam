# Wire Puzzle System - คู่มือการใช้งานและแก้ไข

## 📋 สรุปปัญหาที่พบ

### 1. **PlayerController.Instance.randScreen ไม่มีอยู่จริง**
- ไฟล์เดิมอ้างอิงถึง `PlayerController.Instance.randScreen` ที่ไม่มีใน PlayerController ทั้ง 2 ตัว
- แก้ไขโดยใช้ Camera.main และคำนวณ screen center เอง

### 2. **ความขัดแย้งของ PlayerController**
- มี PlayerController 2 ตัว:
  - `Playground.PlayerController` (มีระบบเต็ม)
  - `MGJ.PlayerController` (ว่างเปล่า)
- แก้ไขโดยไม่ต้องพึ่ง PlayerController

### 3. **ขาดระบบ Integration**
- ไม่มีการเชื่อมต่อกับระบบ Interactable ที่มีอยู่
- ไม่มี Event System สำหรับแจ้งเตือนเมื่อ Puzzle สำเร็จ

---

## 🎯 ไฟล์ใหม่ที่สร้าง

### 1. **WirePuzzleManager_FIXED.cs**
ปรับปรุงจากเดิม:
- ✅ แก้ไข `PlayerController.Instance.randScreen` → ใช้ `Camera.main` แทน
- ✅ เพิ่ม namespace `MGJ.Puzzle`
- ✅ เพิ่มระบบ UnityEvent (OnPuzzleSolved, OnWireConnected, OnWireDisconnected)
- ✅ เพิ่มฟังก์ชัน Public API ที่ใช้งานง่าย
- ✅ รองรับทั้งการคลิกที่จอกลาง และใช้เมาส์
- ✅ เพิ่มระบบ Activate/Deactivate/Reset Puzzle
- ✅ เพิ่มการตรวจสอบความสมบูรณ์ (Completion Percentage)

### 2. **WireStretchController_FIXED.cs**
ปรับปรุงจากเดิม:
- ✅ แก้ไข typo `setConnect` → `SetConnect`
- ✅ เพิ่ม namespace `MGJ.Puzzle`
- ✅ เพิ่มระบบ Color Feedback (ถูก=เขียว, ผิด=แดง)
- ✅ รองรับทั้ง Standard และ URP Material
- ✅ เพิ่มการตรวจสอบว่าต่อถูกหรือผิด
- ✅ เพิ่มฟังก์ชัน Disconnect() สำหรับยกเลิกการเชื่อมต่อ
- ✅ เพิ่ม Gizmos แสดงการเชื่อมต่อที่ถูกต้อง

### 3. **WirePuzzleInteractable.cs** (ไฟล์ใหม่)
เชื่อมต่อกับระบบเกม:
- ✅ ทำให้ Wire Puzzle สามารถ interact ได้
- ✅ รองรับการเปลี่ยนกล้อง
- ✅ รองรับการปิดการเคลื่อนที่ของผู้เล่น
- ✅ รองรับ UI สำหรับ Puzzle
- ✅ กด ESC เพื่อออกจาก Puzzle

---

## 🛠️ วิธีติดตั้งในโปรเจค

### ขั้นตอนที่ 1: ลบไฟล์เก่า (หรือเปลี่ยนชื่อ)
```
WirePuzzleManager.cs → WirePuzzleManager_OLD.cs
WireStretchController.cs → WireStretchController_OLD.cs
```

### ขั้นตอนที่ 2: Rename ไฟล์ใหม่
```
WirePuzzleManager_FIXED.cs → WirePuzzleManager.cs
WireStretchController_FIXED.cs → WireStretchController.cs
```

### ขั้นตอนที่ 3: Setup ใน Unity Editor

#### A. สร้าง Wire Puzzle GameObject
1. สร้าง Empty GameObject ชื่อ "WirePuzzle"
2. Add Component: `WirePuzzleManager`
3. Add Component: `WirePuzzleInteractable` (ถ้าต้องการใช้ระบบ Interact)

#### B. สร้าง Wire Objects
สำหรับแต่ละสาย:

```
GameObject: Wire_Left_1
├─ WireStretchController (Script)
├─ Collider (สำหรับให้คลิก)
│   └─ Layer: Wire
├─ WireMesh (Child - Cube)
│   └─ Renderer (Material)
└─ WireTip (Child - Empty Transform)
```

#### C. การตั้งค่า WirePuzzleManager
```
Inspector:
├─ Wire Configuration
│   ├─ Left Wires: [Wire_Left_1, Wire_Left_2, ...]
│   ├─ Right Wires: [Wire_Right_1, Wire_Right_2, ...]
│   └─ Wire Layer: Wire
├─ Interaction Settings
│   ├─ Interaction Distance: 3
│   ├─ Snap Distance: 0.15
│   └─ Interact Key: E
├─ Camera Settings
│   ├─ Player Camera: Main Camera
│   └─ Use Screen Center: ✓
└─ Events
    ├─ OnPuzzleSolved()
    ├─ OnWireConnected()
    └─ OnWireDisconnected()
```

#### D. การตั้งค่า WireStretchController
```
Inspector:
├─ References
│   ├─ Wire Mesh: WireMesh (child)
│   ├─ Wire Tip: WireTip (child)
│   └─ Correct Wire: Wire_Right_X (ที่ต้องต่อด้วย)
├─ Wire Properties
│   ├─ Min Length: 0.1
│   ├─ Max Length: 1.0
│   ├─ Default Color: White
│   ├─ Connected Color: Green
│   └─ Incorrect Color: Red
└─ Visual Feedback
    ├─ Enable Color Feedback: ✓
    └─ Wire Renderer: (auto-assigned)
```

---

## 📝 การใช้งาน API

### เปิด/ปิด Puzzle
```csharp
using MGJ.Puzzle;

// เปิด Puzzle
WirePuzzleManager puzzle = GetComponent<WirePuzzleManager>();
puzzle.ActivatePuzzle();

// ปิด Puzzle
puzzle.DeactivatePuzzle();

// รีเซ็ต Puzzle
puzzle.ResetPuzzle();
```

### เช็คสถานะ Puzzle
```csharp
// เช็คว่าสำเร็จหรือยัง
bool solved = puzzle.IsSolved();

// เช็คว่าเปิดอยู่หรือไม่
bool active = puzzle.IsActive();

// นับสายที่เชื่อมต่อแล้ว
int connected = puzzle.GetConnectedWireCount();

// ดู % ความสำเร็จ
float percent = puzzle.GetCompletionPercentage(); // 0-100
```

### เชื่อมต่อกับ Events
```csharp
void Start()
{
    WirePuzzleManager puzzle = GetComponent<WirePuzzleManager>();
    
    // ใน Inspector หรือ
    puzzle.OnPuzzleSolved.AddListener(OnPuzzleComplete);
    puzzle.OnWireConnected.AddListener(OnWireSnap);
    puzzle.OnWireDisconnected.AddListener(OnWireRelease);
}

void OnPuzzleComplete()
{
    Debug.Log("✓ Puzzle สำเร็จ!");
    // ปลดล็อกประตู, ให้รางวัล, ฯลฯ
}

void OnWireSnap()
{
    // เล่น Sound Effect
}

void OnWireRelease()
{
    // เล่น Sound Effect
}
```

---

## 🎮 การควบคุม

### สำหรับผู้เล่น
- **คลิกซ้าย / กด E**: จับสาย
- **ลากเมาส์**: ลากสาย
- **ปล่อยคลิก / ปล่อย E**: ปล่อยสาย
- **ESC**: ออกจาก Puzzle

### สำหรับ Designer
- **Use Screen Center = ✓**: ใช้จุดกึ่งกลางจอ (FPS mode)
- **Use Screen Center = ✗**: ใช้ตำแหน่งเมาส์ (Point & Click mode)

---

## 🔧 ปัญหาที่อาจเจอและวิธีแก้

### 1. สายไม่ยืด / ไม่ลาก
**สาเหตุ**: Wire Mesh หรือ Wire Tip ไม่ได้ assign
**แก้ไข**: ตรวจสอบใน Inspector ว่า Reference ครบหรือไม่

### 2. คลิกแล้วไม่จับสาย
**สาเหตุ**: 
- Layer ไม่ตรงกับ Wire Layer ใน Manager
- ไม่มี Collider บนสาย
- Interaction Distance น้อยเกินไป

**แก้ไข**:
```csharp
// ตรวจสอบ Layer
GameObject wire = GameObject.Find("Wire_Left_1");
Debug.Log("Layer: " + LayerMask.LayerToName(wire.layer));

// ต้องตรงกับ Wire Layer ใน WirePuzzleManager
```

### 3. ต่อสายแล้วไม่ติด
**สาเหตุ**: Snap Distance น้อยเกินไป
**แก้ไข**: เพิ่ม Snap Distance เป็น 0.2 - 0.3

### 4. ต่อถูกแล้วแต่ Puzzle ไม่สำเร็จ
**สาเหตุ**: Correct Wire ไม่ได้กำหนดใน WireStretchController
**แก้ไข**: ต้องกำหนด Correct Wire ให้ทุกเส้น

---

## 🎨 การปรับแต่ง Visual

### เปลี่ยนสีสาย
```csharp
// ใน WireStretchController Inspector
Default Color: (1, 1, 1) สีขาว
Connected Color: (0, 1, 0) สีเขียว
Incorrect Color: (1, 0, 0) สีแดง
```

### ปรับความเร็วการเคลื่อนไหว
```csharp
Rotate Speed: 15 (ความเร็วการหมุน)
Stretch Speed: 15 (ความเร็วการยืด)
Return Speed: 10 (ความเร็วกลับสู่เดิม)
```

---

## 📊 ตัวอย่าง Scene Setup

```
WirePuzzle
├─ WirePuzzleManager
├─ WirePuzzleInteractable
├─ LeftWires
│   ├─ Wire_L1 (Red → Correct: Wire_R2)
│   ├─ Wire_L2 (Blue → Correct: Wire_R1)
│   └─ Wire_L3 (Green → Correct: Wire_R3)
└─ RightWires
    ├─ Wire_R1 (Blue)
    ├─ Wire_R2 (Red)
    └─ Wire_R3 (Green)
```

---

## ✅ Checklist ก่อนใช้งาน

- [ ] เปลี่ยนชื่อไฟล์ _FIXED.cs → .cs
- [ ] สร้าง Layer ชื่อ "Wire"
- [ ] สร้าง Wire Objects (Left + Right)
- [ ] Assign Wire Mesh และ Wire Tip ให้ทุกเส้น
- [ ] กำหนด Correct Wire ให้ทุกเส้น
- [ ] Assign Left Wires และ Right Wires ใน Manager
- [ ] ตั้งค่า Wire Layer ใน Manager
- [ ] ทดสอบลากสาย
- [ ] ทดสอบต่อสาย
- [ ] ทดสอบ Event OnPuzzleSolved

---

## 📞 สรุป

### ไฟล์เดิมมีปัญหา:
1. ❌ `PlayerController.Instance.randScreen` ไม่มีอยู่
2. ❌ ไม่มี Event System
3. ❌ ไม่มีการตรวจสอบว่าต่อถูกหรือผิด
4. ❌ ไม่มี Color Feedback
5. ❌ ไม่มีระบบ Interaction

### ไฟล์ใหม่แก้ไขแล้ว:
1. ✅ ใช้ Camera.main แทน
2. ✅ มี UnityEvent สมบูรณ์
3. ✅ ตรวจสอบว่าต่อถูกหรือผิด (correctWire)
4. ✅ มี Color Feedback (เขียว=ถูก, แดง=ผิด)
5. ✅ มี WirePuzzleInteractable สำหรับ interact

---

**หมายเหตุ**: อย่าลืมทดสอบใน Unity Editor ก่อนใช้งานจริง!
