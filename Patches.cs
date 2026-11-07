// Copyright (C) 2026 SlightDust
// SPDX-License-Identifier: GPL-3.0-or-later

using EntityStates.SolusVendor;
using HarmonyLib;
using RoR2;
using UnityEngine.Networking;

namespace SpexSoftlockFix
{
    internal static class SpexRunState
    {
        private static Run _run;
        private static CharacterMaster _submitter;

        internal static CharacterMaster Submitter
        {
            get
            {
                Sync();
                return _submitter;
            }
        }

        internal static void Record(CharacterMaster master)
        {
            Sync();
            _submitter = master;
        }

        private static void Sync()
        {
            if (Run.instance != _run)
            {
                _run = Run.instance;
                _submitter = null;
            }
        }
    }

    [HarmonyPatch(typeof(SetStateOnHurt), nameof(SetStateOnHurt.SetPain))]
    internal static class PreventHurtStealPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(SetStateOnHurt __instance)
        {
            if (!SpexSoftlockFixPlugin.PreventSpexAnger.Value || !__instance)
                return true;

            if (!__instance.GetComponent<SolusVendorController>())
                return true;

            SpexSoftlockFixPlugin.Log.LogDebug("blocked SPEX hurt-state transition");
            return false;
        }
    }

    [HarmonyPatch(typeof(SolusVendorMainState), "CanExecuteSkill")]
    internal static class PreventVendorSkillPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            if (!SpexSoftlockFixPlugin.PreventSpexAnger.Value)
                return true;

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(SolusVendorShrineController), nameof(SolusVendorShrineController.HandleGivenItem))]
    internal static class RecordSubmitterPatch
    {
        [HarmonyPostfix]
        private static void Postfix(SolusVendorShrineController __instance, int intPickupIndex)
        {
            if (!SpexSoftlockFixPlugin.RefundExposedCerebellum.Value)
                return;

            PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(intPickupIndex));
            if (pickupDef == null || pickupDef.itemIndex != DLC3Content.Items.MasterCore.itemIndex)
                return;

            Interactor interactor = Traverse.Create(__instance).Field<Interactor>("interactor").Value;
            if (!interactor)
                return;

            CharacterBody body = interactor.GetComponent<CharacterBody>();
            CharacterMaster master = body ? body.master : null;
            if (!master)
                return;

            SpexRunState.Record(master);
        }
    }

    [HarmonyPatch(typeof(StealItem), nameof(StealItem.OnExit))]
    internal static class RefundCerebellumPatch
    {
        [HarmonyPostfix]
        private static void Postfix(StealItem __instance)
        {
            if (!SpexSoftlockFixPlugin.RefundExposedCerebellum.Value)
                return;

            if (!NetworkServer.active)
                return;

            CharacterMaster master = SpexRunState.Submitter;
            if (!master || !master.inventory)
                return;

            ItemIndex core = DLC3Content.Items.MasterCore.itemIndex;
            ItemIndex encrypted = DLC3Content.Items.MasterBattery.itemIndex;
            if (master.inventory.GetItemCountEffective(core) > 0)
                return;
            if (master.inventory.GetItemCountEffective(encrypted) > 0)
                return;

            master.inventory.GiveItemPermanent(core, 1);
            SpexSoftlockFixPlugin.Log.LogInfo("refunded exposed cerebellum to " + master.name);

            if (SpexSoftlockFixPlugin.AnnounceRefund.Value)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = IsChineseLanguage()
                        ? "<color=#c8ffc8>[SPEX Fix] 暴露小脑已返还给供奉者</color>"
                        : "<color=#c8ffc8>[SPEX Fix] Exposed Cerebellum refunded to the offering player</color>"
                });
            }
        }

        private static bool IsChineseLanguage()
        {
            string languageName = Language.currentLanguageName;
            return !string.IsNullOrEmpty(languageName) && languageName.StartsWith("zh");
        }
    }
}
