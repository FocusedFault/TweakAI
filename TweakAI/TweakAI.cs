using BepInEx;
using RoR2;
using RoR2.CharacterAI;
using System.Linq;
using System.Collections.Generic;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace TweakAI
{
  [BepInPlugin("com.Nuxlar.TweakAI", "TweakAI", "1.2.0")]

  public class TweakAI : BaseUnityPlugin
  {
    public void Awake()
    {
      IL.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += TargetMainlyPlayers;
      On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += InfiniVision;
    }

    private void TargetMainlyPlayers(ILContext il)
    {
      ILCursor c = new ILCursor(il);

      if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<BullseyeSearch>(nameof(BullseyeSearch.GetResults))))
      {
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((IEnumerable<HurtBox> results, BaseAI instance) =>
        {
          if (instance && instance.body.teamComponent && instance.body.teamComponent.teamIndex != TeamIndex.Player)
          {
            // Filter results to only target players (don't target player allies like drones)
            IEnumerable<HurtBox> playerControlledTargets = results.Where(hurtBox =>
                            {
                              GameObject entityObject = HurtBox.FindEntityObject(hurtBox);
                              return entityObject && entityObject.TryGetComponent(out CharacterBody characterBody) && characterBody.isPlayerControlled;
                            });

            // If there are no players, use the default target so that the AI doesn't end up doing nothing
            return playerControlledTargets.Any() ? playerControlledTargets : results;
          }
          else
            return results;
        });
      }
    }

    private HurtBox InfiniVision(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
    {
      if (self && self.body.teamComponent && self.body.teamComponent.teamIndex != TeamIndex.Player)
      {
        maxDistance = float.PositiveInfinity;
        filterByLoS = false;
        full360Vision = true;
      }

      return orig(self, maxDistance, full360Vision, filterByLoS);
    }
  }
}