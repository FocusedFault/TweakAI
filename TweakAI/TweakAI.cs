using BepInEx;
using RoR2;
using RoR2.Navigation;
using EntityStates.AI.Walker;
using System.Collections.Generic;
using UnityEngine;

namespace TweakAI
{
  [BepInPlugin("com.Nuxlar.TweakAI", "TweakAI", "1.0.1")]

  public class TweakAI : BaseUnityPlugin
  {
    public void Awake()
    {
      On.EntityStates.AI.Walker.LookBusy.OnEnter += Busynt;
      On.EntityStates.AI.Walker.Wander.OnEnter += BetterWander;
    }

    private void Busynt(On.EntityStates.AI.Walker.LookBusy.orig_OnEnter orig, LookBusy self)
    {
      orig(self);
      self.ModifyNextState(new Wander());
      if (self is Guard)
        self.ModifyNextState(new Wander());
    }

    private void BetterWander(On.EntityStates.AI.Walker.Wander.orig_OnEnter orig, Wander self)
    {
      orig(self);
      if (!(bool)(Object)self.ai || !(bool)(Object)self.body)
        return;
      if ((bool)(Object)PlayerCharacterMasterController.instances[0].body)
        self.targetPosition = PlayerCharacterMasterController.instances[0].body.footPosition;
      else
      {
        NodeGraph nodes = self.body.isFlying ? SceneInfo.instance.airNodes : SceneInfo.instance.groundNodes;
        HullMask hullMask = HullMask.None;
        switch (self.body.hullClassification)
        {
          case HullClassification.Human:
            hullMask = HullMask.Human;
            break;
          case HullClassification.Golem:
            hullMask = HullMask.Golem;
            break;
          case HullClassification.BeetleQueen:
            hullMask = HullMask.BeetleQueen;
            break;
        }
        List<NodeGraph.NodeIndex> withFlagConditions = nodes.GetActiveNodesForHullMaskWithFlagConditions(hullMask, NodeFlags.None, NodeFlags.NoCharacterSpawn);
        NodeGraph.NodeIndex nodeIndex = withFlagConditions[Random.Range(0, withFlagConditions.Count)];
        Vector3 position;
        nodes.GetNodePosition(nodeIndex, out position);
        self.targetPosition = position;
      }
      self.bodyInputs.pressSprint = true;
      self.ai.SetGoalPosition(self.targetPosition);
      self.PickNewTargetLookPosition();
    }
  }
}