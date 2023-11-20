using BepInEx;
using RoR2;
using EntityStates.AI.Walker;
using System.Collections.Generic;
using UnityEngine;

namespace TweakAI
{
  [BepInPlugin("com.Nuxlar.TweakAI", "TweakAI", "1.1.0")]

  public class TweakAI : BaseUnityPlugin
  {
    public void Awake()
    {
      On.EntityStates.AI.Walker.LookBusy.OnEnter += Busynt;
      On.EntityStates.AI.Walker.Wander.OnEnter += BetterWander;
    }

    private void Busynt(On.EntityStates.AI.Walker.LookBusy.orig_OnEnter orig, LookBusy self)
    {
      if (self is Guard || self is LookBusy)
        self.outer.SetState(new Wander());
      orig(self);
    }

    private void BetterWander(On.EntityStates.AI.Walker.Wander.orig_OnEnter orig, Wander self)
    {
      orig(self);
      if (!(bool)self.ai || !(bool)self.body)
        return;
      if (self.body.master && self.body.master.currentLifeStopwatch > 0 && self.body.name == "LunarWispBody(Clone)")
        return;
      List<CharacterBody> playerBodies = new();
      foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
      {
        if (cm.teamIndex == TeamIndex.Player)
        {
          CharacterBody cb = cm.GetBody();
          if (cb && cb.isPlayerControlled)
            playerBodies.Add(cb);
        }
      }
      Transform spawnPoint = Stage.instance.GetPlayerSpawnTransform();
      if (playerBodies.Count > 0)
        self.targetPosition = playerBodies[Random.Range(0, playerBodies.Count)].footPosition;
      else if (spawnPoint)
        self.targetPosition = spawnPoint.position;
    }
  }
}