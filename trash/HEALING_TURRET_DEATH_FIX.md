# Healing Turret Not Dying - Fix

## Problem
Healing turret takes damage but doesn't die when health reaches 0.

## Root Cause
The Health component's `m_OnDieEvent` is not connected to the turret's `Die()` method in Unity Inspector.

## Solution

### In Unity Editor:

1. **Select your HealingTurretRed prefab**

2. **Find the Health component** in Inspector

3. **Scroll down to "On Die Event"**

4. **If it's empty:**
   - Click the + button to add an event
   - Drag the HealingTurretRed GameObject into the object field
   - Select Function: `ArmyElement -> Die()`

### Visual Guide:
```
Health (Script)
├─ m_Start Health: 100
├─ m_Health Bar: [Slider reference]
└─ On Die Event (UnityEvent)
    └─ [+] Add Event
        ├─ Runtime: HealingTurretRed (GameObject)
        └─ Function: ArmyElement.Die()
```

## Code Fix Applied

Also changed the death check from `== 0` to `<= 0` in Health.cs to handle floating point precision issues.

**Before:**
```csharp
if (m_Health == 0 && m_OnDieEvent != null)
```

**After:**
```csharp
if (m_Health <= 0 && m_OnDieEvent != null)
```

## Testing

1. Place healing turret in scene
2. Have green army attack it
3. Watch health bar decrease
4. When health reaches 0, turret should:
   - Call Die() method
   - Be removed from army list
   - Destroy GameObject
   - Disappear from scene

## If Still Not Working

Check console for:
- Are you seeing damage being applied?
- Does health reach 0?
- Is Die() being called?

Add this debug to HealingTurret if needed:
```csharp
public new void Die()
{
    Debug.Log("[HealingTurret] Die() called!");
    base.Die();
}
```

## Quick Test Without Inspector Setup

If you don't want to set it up in Inspector, add this to HealingTurret.cs:

```csharp
private void Update()
{
    // Safety check - if health is 0, force die
    if (Health <= 0)
    {
        Die();
    }
}
```

But the proper way is to hook it up in the Inspector like the other turrets.
