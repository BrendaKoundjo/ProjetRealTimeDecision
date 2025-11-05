# 🏥 Healing Turret - Visual Architecture

## 🎯 System Overview Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                     RED ARMY BATTLEFIELD                         │
│                                                                   │
│  [Drone]         [Drone]         [Turret]                       │
│   💚 80%          ❤️ 40%          💛 60%                         │
│                                                                   │
│                                                                   │
│                  [HealingTurret] 🏥                              │
│                       │                                          │
│                       │ Detects injured ally                     │
│                       ↓                                          │
│                   Rotates turret head                            │
│                       │                                          │
│                       ↓                                          │
│                  Fires healing rocket 🟢                         │
│                       │                                          │
│                       ↓                                          │
│                  ╔═══════════╗                                   │
│                  ║  💚 💚 💚  ║ ← Healing Area (5m radius)       │
│                  ║ 💚 [Drone] 💚 ║                               │
│                  ║  💚 💚 💚  ║                                   │
│                  ╚═══════════╝                                   │
│                       ↓                                          │
│                  Drone: 40% → 70% HP! ✨                         │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Behavior Tree Visual

```
                    Entry
                      │
                      ↓
              ┌───────────────┐
              │   Repeater    │
              │  (Forever)    │
              └───────┬───────┘
                      │
                      ↓
              ┌───────────────┐
              │   Sequence    │
              │  (All must    │
              │   succeed)    │
              └───────┬───────┘
                      │
        ┌─────────────┼─────────────┐
        │             │             │
        ↓             ↓             ↓
  ┌─────────┐   ┌─────────┐   ┌─────────┐
  │ Select  │   │  Seek   │   │ Shoot   │
  │  Ally   │   │ Target  │   │ Healing │
  │ Needing │   │ (Rotate)│   │ Rocket  │
  │ Healing │   │         │   │         │
  └─────────┘   └─────────┘   └─────────┘
        │             │             │
        └─────────────┼─────────────┘
                      │
                      ↓
              ┌───────────────┐
              │   Wait        │
              │   2-4 sec     │
              └───────────────┘
                      │
                      ↓
                 (Loop back)
```

**Decision Logic:**
- ❌ No injured allies? → Sequence FAILS → Loop restarts
- ✅ Found injured ally? → Continue to aim
- ✅ Aimed successfully? → Continue to shoot
- ✅ Shot fired? → Wait, then loop

---

## 📦 Class Hierarchy

```
                 MonoBehaviour
                       │
        ┌──────────────┼──────────────┐
        │              │              │
   IArmyElement    Rigidbody      Action
        │              │         (BehaviorDesigner)
        ↓              │              │
   ArmyElement         │         ┌────┴────┬─────────────┐
        │              │         │         │             │
   ┌────┴────┐         │    Select    Seek Task    Shoot Task
   │         │         │     Ally        │             │
 Turret  Drone         │    Needing      │             │
   │                   │    Healing      │             │
   ↓                   │                 │             │
HealingTurret          │                 │             │
   │                   ↓                 ↓             ↓
   │            HealingRocket   HealingTurretSeek  HealingTurretShoot
   │                   
   └──> Shoots ───────→ HealingRocket
```

---

## 🚀 Rocket Lifecycle

```
1. SPAWN
   ┌─────────────────┐
   │ HealingTurret   │
   │ calls Shoot()   │
   └────────┬────────┘
            │
            ↓
   ┌─────────────────┐
   │ Instantiate     │
   │ HealingRocket   │
   └────────┬────────┘
            │
            ↓
2. FLIGHT (Ballistic Trajectory)
   ┌─────────────────┐
   │ Apply gravity   │
   │ + initial vel   │
   │ ↗︎ ↗︎ ↗︎ → → ↘︎ ↘︎ ↘︎  │
   └────────┬────────┘
            │
            ↓
3. IMPACT
   ┌─────────────────┐
   │ Coroutine ends  │
   │ at target pos   │
   └────────┬────────┘
            │
            ↓
4. HEALING
   ┌─────────────────────────────┐
   │ Physics.OverlapSphere       │
   │ Find all colliders in 5m    │
   └──────────┬──────────────────┘
              │
              ↓
   ┌─────────────────────────────┐
   │ Filter by Tag (RedArmy)     │
   └──────────┬──────────────────┘
              │
              ↓
   ┌─────────────────────────────┐
   │ For each friendly:          │
   │   health.RestoreHealth(30)  │
   └──────────┬──────────────────┘
              │
              ↓
5. CLEANUP
   ┌─────────────────┐
   │ Destroy rocket  │
   │ gameObject      │
   └─────────────────┘
```

---

## 🎮 Target Selection Algorithm

```
START: SelectAllyNeedingHealing.OnUpdate()
   │
   ↓
┌──────────────────────────┐
│ Find all ArmyElements    │
│ in scene                 │
└──────┬───────────────────┘
       │
       ↓
┌──────────────────────────┐
│ Filter by:               │
│ • Same tag (RedArmy)     │
│ • Not self               │
│ • Distance < 100m        │
│ • Health < 70% max       │
└──────┬───────────────────┘
       │
       ↓
    List empty?
       │
   ┌───┴───┐
  YES     NO
   │       │
   ↓       ↓
FAILURE  ┌──────────────────┐
         │ Sort by Health   │
         │ (lowest first)   │
         └────┬─────────────┘
              │
              ↓
         ┌──────────────────┐
         │ target = list[0] │
         │ (lowest HP ally) │
         └────┬─────────────┘
              │
              ↓
           SUCCESS
```

---

## 💊 Healing Calculation

```
Before Healing:
┌─────────────┐
│   Health    │
│             │
│ ████░░░░░░  │ 40/100 HP
│             │
└─────────────┘

Healing Rocket Impact:
┌─────────────┐
│   +30 HP    │ ← RestoreHealth(30)
└─────────────┘

After Healing:
┌─────────────┐
│   Health    │
│             │
│ ███████░░░  │ 70/100 HP
│             │
└─────────────┘

Clamped to Max:
If current = 85 HP
   +30 HP = 115 HP
   → Clamped to 100 HP (max)
```

---

## 🔧 Component Dependencies

```
HealingTurretRed (GameObject)
  ├─ Transform
  ├─ Collider
  ├─ Rigidbody (if needed)
  │
  ├─ HealingTurret (Script) ⭐
  │   ├─ m_TurretHead → Transform
  │   ├─ m_HealingRocketPrefab → GameObject
  │   ├─ m_SpawnPoints[] → Transform[]
  │   ├─ m_RotationSpeed → float
  │   └─ m_RocketTravelDuration → float
  │
  ├─ Health (Script)
  │   ├─ m_StartHealth → float
  │   └─ m_HealthBar → Slider
  │
  ├─ Behavior Tree (Component)
  │   └─ ExternalBehavior → HealingTurretRedBehavior.asset
  │
  └─ Tag: "RedArmy"


HealingRocketRed (Prefab)
  ├─ Transform
  ├─ Rigidbody ⭐
  ├─ Collider (Trigger)
  │
  ├─ HealingRocket (Script) ⭐
  │   ├─ m_MaxLifeDuration → float
  │   ├─ m_HealingRadius → float
  │   ├─ m_HealingPoints → float
  │   └─ m_HealingEffectPrefab → GameObject
  │
  └─ Tag: "RedArmy"
```

---

## 🎯 Combat Scenario Example

```
T=0s: Battle starts
  Red: 5 Drones, 3 Turrets, 1 HealingTurret
  Green: 5 Drones, 3 Turrets

T=10s: Combat engaged
  Red Drone #2: 60% HP (damaged by green turret)
  ↓
  HealingTurret: Detects Drone #2
  ↓
  HealingTurret: Rotates towards Drone #2

T=12s: Healing shot fired
  HealingRocket flies towards Drone #2
  ↓
  Impact at T=14s
  ↓
  Drone #2: 60% → 90% HP ✨

T=16s: Wait period (cooldown)

T=18s: Scan for next injured ally...

RESULT: Red Army sustains longer!
        Green Army can't match healing
        Red Victory! 🏆
```

---

## 🏗️ File Structure

```
ProjetRealTimeDecision/
│
├─ Assets/
│  │
│  ├─ Scripts/
│  │  ├─ Army/
│  │  │  ├─ HealingTurret.cs ⭐ NEW
│  │  │  ├─ HealingRocket.cs ⭐ NEW
│  │  │  ├─ Health.cs (Modified) ⭐
│  │  │  ├─ Turret.cs (Original)
│  │  │  └─ ...
│  │  │
│  │  └─ MyBehaviorTrees/
│  │     ├─ SelectAllyNeedingHealing.cs ⭐ NEW
│  │     ├─ HealingTurretShoot.cs ⭐ NEW
│  │     ├─ HealingTurretSeekTarget.cs ⭐ NEW
│  │     ├─ HealingTurretRedBehavior.asset ⭐ NEW
│  │     └─ ...
│  │
│  └─ Prefabs/ (YOU CREATE THESE)
│     ├─ HealingTurretRed.prefab 🔨 TODO
│     ├─ HealingRocketRed.prefab 🔨 TODO
│     └─ ...
│
├─ HEALING_TURRET_SETUP_GUIDE.md ⭐ NEW
├─ HEALING_TURRET_SUMMARY.md ⭐ NEW
└─ HEALING_TURRET_VISUAL_GUIDE.md ⭐ NEW (this file)
```

---

## 🎨 Recommended Visual Design

### Healing Rocket
```
  Front View:        Side View:
     ╱╲               ═════╗
    ╱  ╲              ═════╣ → Green glow
   ╱ 🟢 ╲             ═════╝
  ╱  💚  ╲           ═══╗═══
 ╱________╲          ═══╩═══
    ║  ║                ↑
    ╚══╝            Rocket body
```

**Color Scheme:**
- Body: White or light green
- Glow: Bright green (#00FF00)
- Trail: Green particles
- Impact: Green starburst

### Healing Turret
```
    Top View:
      ╔═══╗
      ║ + ║  ← Medical cross
      ╚═╤═╝
    ┌───┴───┐
    │ ◯   ◯ │  ← Barrel openings
    └───┬───┘
        │
    ╔═══╧═══╗
    ║       ║  ← Turret body
    ║  🏥   ║  ← Healing icon
    ╚═══════╝
        │
    ┌───┴───┐
    │ ▓▓▓▓▓ │  ← Base
    └───────┘
```

**Color Scheme:**
- Body: White, light blue, or medical green
- Cross: Red or green
- Base: Same as Red Army (red)
- Beacon: Green pulsing light

---

This visual guide should help you understand exactly how the healing turret system works! 🎯
