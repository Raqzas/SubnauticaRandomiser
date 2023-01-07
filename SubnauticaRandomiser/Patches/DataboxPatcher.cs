﻿using System.Collections.Generic;
using HarmonyLib;
using SubnauticaRandomiser.Objects;
using UnityEngine;

namespace SubnauticaRandomiser.Patches
{
    // [HarmonyPatch]
    internal class DataboxPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlueprintHandTarget), nameof(BlueprintHandTarget.Start))]
        internal static bool PatchDatabox(ref BlueprintHandTarget __instance)
        {
            Dictionary<RandomiserVector, TechType> boxDict = InitMod._Serializer.Databoxes;
            Vector3 position = __instance.gameObject.transform.position;

            foreach (RandomiserVector vector in boxDict.Keys)
            {
                if (vector.EqualsUnityVector(position))
                {
                    InitMod._Log.Debug($"[D] Replacing databox {position} with {boxDict[vector].AsString()}");
                    __instance.unlockTechType = boxDict[vector];
                    break;
                }
            }
            
            return true;
        }
    }
}
