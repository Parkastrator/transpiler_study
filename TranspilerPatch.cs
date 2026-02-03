using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NPOI.SS.Formula.Functions;
using transpiler_study;
using Code = HarmonyLib.Code;

namespace transpiler_study;

[BepInPlugin(ModInfo.Guid, ModInfo.Name, ModInfo.Version)]
internal class TranspilerMod : BaseUnityPlugin
{
    internal static TranspilerMod? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        var harmony = new Harmony(ModInfo.Guid);
        harmony.PatchAll();
    }
    

   
    [HarmonyPatch]
    class Patchy
    {

        private static long really_hurt_them(long damage)
        {
            Msg.Nerun("This attack was going to do " + damage.ToString() +" damage.");
            Msg.Nerun("MUWAHAHAHAHA");
            return 99999999999;
        }

        private static void say_damage(long damage)
        {
            Msg.Nerun(damage.ToString());
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Card), nameof(Card.DamageHP), new Type[] { typeof(long), typeof(int), typeof(int), typeof(AttackSource), typeof(Card), typeof(bool), typeof(Thing), typeof(Chara) })]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true, //
                    new CodeMatch(OpCodes.Stloc_3)) //looks for first instance of this opcode being called
                //.Advance(-1) we're not using this but this is for like, moving the current position around. You could use Advance(-1) to start inserting code RIGHT BEFORE the point where your codematcher has stopped.
                .ThrowIfInvalid("Did not find it")//This just throws an error message if our CodeMatcher sequences does not show up (it won't)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),//This pushes the value in argument one (this is damage) onto the stack
                    Transpilers.EmitDelegate(really_hurt_them),//This calls the function above
                    new CodeInstruction(OpCodes.Starg_S, 1)//This store the value returned from really_hurt_them into the place where argument one (again, damage) is.
                    )
                        
                .InstructionEnumeration();
        }
     
    }
    

}