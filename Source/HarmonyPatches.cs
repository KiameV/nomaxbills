using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace NoMaxBills
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("com.showhair.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message(
				"NoMaxBills:" + Environment.NewLine +
                "  Transpiler:" + Environment.NewLine +
				"    BillStack.DoListing");
		}
	}

    [HarmonyPatch(typeof(BillStack), "DoListing")]
    public static class Patch_BillStack_DoListing
	{
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo billStackCount = AccessTools.Property(typeof(BillStack), nameof(BillStack.Count)).GetGetMethod();
            List<CodeInstruction> instructionList = instructions.ToList();
            bool found = false;
			for (int i = 0; i < instructionList.Count; ++i)
			{
#if DEBUG && TRANSPILER
				printTranspiler(instructionList[i]);
#endif
				CodeInstruction instruction = instructionList[i];
				if (!found && 
					instructionList[i].opcode == OpCodes.Ldc_I4_S &&
					Convert.ToInt32(instructionList[i].operand) == 15)
				{
                    found = true;
					instructionList[i].operand = (object)125;
				}
				yield return instruction;
            }
            if (!found)
            {
                Log.Error("No Max Bills could not inject itself properly. This is due to other mods modifying the same code this mod needs to modify.");
            }
        }

#if DEBUG && TRANSPILER
        static void printTranspiler(CodeInstruction i, string pre = "")
        {
            Log.Warning("CodeInstruction: " + pre + " opCode: " + i.opcode + " operand: " + i.operand);
        }

        static string printTranspiler(IEnumerable<Label> labels)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (labels == null)
            {
                sb.Append("<null labels>");
            }
            else
            {
                foreach (Label l in labels)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(l);
                }
            }
            if (sb.Length == 0)
            {
                sb.Append("<empty labels>");
            }
            return sb.ToString();
        }
#endif
	}
}