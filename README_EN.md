# SPEX Softlock Fix

Fixes a soft-lock in Risk of Rain 2's Alloyed Collective DLC: after offering the **Exposed Cerebellum** at the shrine in the **Computational Exchange** to open the Decrypted Portal, accidentally damaging SPEX ejects you from the bazaar — and the Neural Sanctum becomes unreachable for the rest of the run.

## How the soft-lock happens

1. When the offering shrine (`SolusVendorShrineController.HandleGivenItem`) receives `MasterCore` (the Exposed Cerebellum), it permanently removes the item with `RemoveItemPermanent`, switches itself to the `Disabled` state, and drives the state machine into `SolusWebPortalSequence` to open the Decrypted Portal.
2. SPEX (`EntityStates.SolusVendor`) enters `StealItem` when it takes damage above the hit threshold: it steals random items, and in `OnExit` it activates the scene object `KickOutOfShop`, ejecting everyone from the bazaar while permanently disabling itself.
3. The result: the cerebellum is consumed, the shrine is closed, and SPEX is inert. Re-entering the bazaar gives you nothing to offer and no portal to walk through.

## What this mod does

| Config entry | Default | Effect |
| --- | --- | --- |
| `Server.PreventSpexAnger` | `true` | Blocks `SetStateOnHurt.SetPain()` (only on objects carrying `SolusVendorController`) and `SolusVendorMainState.CanExecuteSkill()`, so SPEX never enters the steal state and the ejection is never triggered. |
| `Server.RefundExposedCerebellum` | `true` | Records the player who actually handed over the Exposed Cerebellum; when `StealItem.OnExit` fires (the ejection), the item is refunded if that player has neither the Exposed nor the Encrypted Cerebellum in their inventory. |
| `Server.AnnounceRefund` | `true` | Broadcasts a chat message to the team when a refund happens. |

The refund goes only to **the player who made the offering** — not to the host, and not to a random player in the lobby. In multiplayer, if somebody else fires the stray shot that causes the ejection, the item still returns to the player who offered it. The inventory is checked first, so the item is never duplicated.

## Installation (r2modman)

1. Open r2modman and select your **Risk of Rain 2** profile.
2. Search for `SPEX Softlock Fix` on the **Online** tab (available once published to Thunderstore), or use **Settings → Import local mod** and pick the downloaded zip.
3. Click **Start modded**.

In multiplayer only the **host** needs the mod installed; all logic is server-side.

## Compatibility

- Only SPEX-specific classes and methods are patched, so other enemies keep their hit-stun, freeze, and taunt reactions.
- `SetPain` is a shared method, but the prefix checks for `SolusVendorController` first and lets everything else run vanilla.
- If a DLC update renames the relevant classes, the patches fail softly instead of crashing the game; the four Harmony hook lines are visible in the log.

## Known limitations

- The refund is tracked **per run**, so it only triggers if the Exposed Cerebellum was offered during the same run.
- If a player disconnects and reconnects, their `CharacterMaster` changes and the record is lost (no refund in that case).
- With `PreventSpexAnger` enabled, SPEX no longer reacts to being damaged by stealing items and ejecting everyone. This is an intentional behavior change.

## Building

Requires the .NET SDK plus `RoR2.dll` from your game folder and BepInEx 5 with HarmonyX:

```bash
dotnet build -c Release
dotnet build -c Release -p:GameDir="<game dir>" -p:BepInExCoreDir="<profile>\BepInEx\core"
```

中文说明见 [README.md](README.md)。
