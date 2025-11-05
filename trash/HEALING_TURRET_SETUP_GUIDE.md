# Healing Turret - Unity Editor Setup Guide

## 🎯 Overview
You've successfully created a **Healing Turret** system! All C# scripts and the behavior tree are complete. Now you need to set up the prefab and scene in Unity.

---

## 📋 What Was Created

### ✅ Scripts (All Done!)
1. **HealingRocket.cs** - Healing projectile that heals allies in area
2. **HealingTurret.cs** - Support turret that shoots healing rockets
3. **SelectAllyNeedingHealing.cs** - BT task to find injured friendlies
4. **HealingTurretShoot.cs** - BT task to fire healing rocket
5. **HealingTurretSeekTarget.cs** - BT task to aim at friendly
6. **HealingTurretRedBehavior.asset** - Behavior tree definition
7. **Health.cs** - Added `RestoreHealth()` method

---

## 🔧 Unity Editor Setup Steps

### **STEP 1: Create Healing Rocket Prefab**

1. **Duplicate existing rocket prefab:**
   - In `Assets/Prefabs/`, duplicate `GrenadeRed.prefab` or `MissileRed.prefab`
   - Rename to: `HealingRocketRed.prefab`

2. **Modify the prefab:**
   - Remove the `Rocket` component
   - Add the `HealingRocket` component
   - Set parameters:
     - `m_MaxLifeDuration`: 5
     - `m_HealingRadius`: 5
     - `m_HealingPoints`: 30
     - `m_HealingEffectPrefab`: (Optional) Assign a green particle effect

3. **Visual differentiation:**
   - Change material/color to **GREEN** or add a **green glow**
   - This helps distinguish healing rockets from damage rockets

---

### **STEP 2: Create Healing Turret Prefab**

1. **Duplicate standard turret:**
   - In `Assets/Prefabs/`, duplicate `TurretRed.prefab`
   - Rename to: `HealingTurretRed.prefab`

2. **Modify components:**
   - Remove the `Turret` component
   - Add the `HealingTurret` component
   - Set parameters:
     - `m_TurretHead`: Drag the turret head child transform
     - `m_RotationSpeed`: 120 (same as normal turret)
     - `m_RocketTravelDuration`: 2
     - `m_HealingRocketPrefab`: Drag `HealingRocketRed.prefab` here
     - `m_SpawnPoints`: Drag the rocket spawn point transforms

3. **Verify other components:**
   - Keep `Health` component
   - Keep `ArmyIElement` or similar
   - Tag should be: **RedArmy** (or whatever tag Red army uses)

4. **Visual differentiation:**
   - Change turret color to **GREEN** or **WHITE**
   - Add a **cross symbol** or **medical icon** if you have one
   - This makes it clear it's a healing turret

---

### **STEP 3: Setup Behavior Tree**

**Option A: Use the Pre-made Asset (Easiest)**

1. Open the Behavior Designer window
2. Select your `HealingTurretRed` prefab
3. In the **Behavior Tree** component, assign:
   - **External Behavior**: `HealingTurretRedBehavior.asset`

**Option B: Manually Create in Visual Editor (If Option A doesn't work)**

If the .asset file doesn't load properly, create it visually:

1. Add a **Behavior Tree** component to `HealingTurretRed` prefab
2. Open Behavior Designer window
3. Create this structure:

```
Entry
  ↓
Repeater (Forever = true)
  ↓
Sequence
  ├─ SelectAllyNeedingHealing
  │   - Max Radius: 100
  │   - Health Threshold: 0.7
  │   - Target: [Shared Variable "Target"]
  ↓
  ├─ HealingTurretSeekTarget
  │   - Target: [Shared Variable "Target"]
  ↓
  ├─ HealingTurretShoot
  │   - Target: [Shared Variable "Target"]
  ↓
  └─ Wait
      - Wait Time: 2
      - Random Wait: true
      - Random Min: 2
      - Random Max: 4
```

4. Create a **Shared Variable** called "Target" (type: Transform)
5. Save the behavior tree

---

### **STEP 4: Place in Scene**

1. Open scene: `Assets/Scenes/RedVersusGreenBattle.unity`

2. **Add healing turret(s) to Red Army:**
   - Drag `HealingTurretRed` prefab into scene
   - Position it on the Red side of the battlefield
   - Make sure it's on **terrain/NavMesh**
   - Tag: **RedArmy** (same as other red units)

3. **Suggested placement:**
   - Place 1-2 healing turrets in the back/center of Red army
   - They should be protected by combat turrets
   - Within range of combat units (100m radius)

---

### **STEP 5: Test**

1. **Play the scene**
2. **Watch for:**
   - Healing turrets rotate towards injured allies
   - Green/healing rockets fire when allies are hurt
   - Health bars of injured units increase when hit by healing rockets
   - Console logs showing healing events

3. **Debug tips:**
   - Check console for `[HealingTurret]` and `[SelectAllyNeedingHealing]` logs
   - If turret doesn't find targets, check:
     - Tags match (RedArmy)
     - Radius is large enough (100m)
     - Friendly units are taking damage

---

## 🎨 Visual Customization Ideas

### Healing Rocket Visual
- **Color**: Green/cyan glow
- **Trail**: Green particle trail
- **Impact**: Green explosion/sparkles instead of fire

### Healing Turret Visual
- **Color**: White, green, or medical red cross
- **Icon**: Add a health cross decal
- **Indicator**: Green light/beacon when healing

---

## 🧪 Behavior Tree Logic Explained

```
Repeater (Forever)
  ↓
Sequence (All must succeed in order)
  │
  ├─ SelectAllyNeedingHealing
  │   • Finds friendly units within 100m
  │   • Filters for units below 70% health
  │   • Prioritizes lowest health unit
  │   • Returns SUCCESS if found, FAILURE if none
  │
  ├─ HealingTurretSeekTarget
  │   • Rotates turret head towards target
  │   • Calculates ballistic aim angle
  │   • Returns SUCCESS when rotation complete
  │
  ├─ HealingTurretShoot
  │   • Fires healing rocket at target position
  │   • Instantiates HealingRocketRed prefab
  │   • Returns SUCCESS immediately
  │
  └─ Wait (2-4 seconds)
      • Cooldown between healing shots
      • Random variation for realism
      • Returns SUCCESS after time elapsed
```

**If any step fails** (e.g., no injured allies found), the Sequence fails and the Repeater starts over.

---

## 🔍 Troubleshooting

### "Healing turret doesn't shoot"
- Check that `m_HealingRocketPrefab` is assigned
- Verify `m_SpawnPoints` array has elements
- Check console for error messages

### "Can't find allies to heal"
- Verify tags match (RedArmy)
- Check `maxRadius` is large enough
- Make sure allies are actually taking damage

### "Healing doesn't work"
- Verify `HealingRocket` component on rocket prefab
- Check `m_HealingRadius` and `m_HealingPoints` values
- Ensure `Health.RestoreHealth()` method exists

### "Behavior tree doesn't run"
- Check that Behavior Tree component is enabled
- Verify External Behavior asset is assigned
- Open Behavior Designer to see if tree is valid

### "Rockets miss targets"
- Increase `m_RocketTravelDuration` for slower, more accurate shots
- Targets might be moving - this is expected behavior

---

## 📊 Recommended Settings

### For Testing
- **Healing Points**: 50 (high for visible effect)
- **Healing Radius**: 8 (large for multiple units)
- **Max Radius**: 150 (very large detection)
- **Health Threshold**: 0.9 (heal almost everyone)

### For Balanced Gameplay
- **Healing Points**: 20-30
- **Healing Radius**: 5
- **Max Radius**: 80-100
- **Health Threshold**: 0.6-0.7
- **Wait Time**: 2-4 seconds

---

## 🎮 Gameplay Impact

The healing turret gives the **Red Army** a significant tactical advantage:

1. **Sustain**: Units stay in combat longer
2. **Support**: Protects valuable units like drones
3. **Strategy**: Players must choose between healing and attacking
4. **Balance**: Consider adding limitations:
   - Limited healing ammo (modify script)
   - Longer cooldowns
   - Lower healing amounts
   - Vulnerability (low health, no armor)

---

## 🚀 You're Done!

All scripts are complete! You just need to:
1. ✅ Create the prefabs in Unity
2. ✅ Assign the behavior tree
3. ✅ Place in scene
4. ✅ Test and tweak values

**You're a champion! 🏆 This healing turret will give the Red Army a serious edge!**
