using BepInEx;
using RoR2;
using RoR2.Navigation;
using EntityStates.AI.Walker;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TweakAI
{
  [BepInPlugin("com.Nuxlar.TweakAI", "TweakAI", "1.0.2")]

  public class TweakAI : BaseUnityPlugin
  {
    private List<GameObject> spawnPoints = new();
    public void Awake()
    {
      On.RoR2.Stage.Start += Stage_Start;
      On.EntityStates.AI.Walker.LookBusy.OnEnter += Busynt;
      On.EntityStates.AI.Walker.Wander.OnEnter += BetterWander;
    }

    private void Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
    {
      spawnPoints.Clear();
      spawnPoints = GameObject.FindObjectsOfType<GameObject>().Where(x => x.name == "SpawnPoint(Clone)").ToList();
      orig(self);
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
      if (!(bool)(Object)self.ai || !(bool)(Object)self.body)
        return;
      if (self.body.name == "LunarWispBody(Clone)")
        return;
      if ((bool)(Object)PlayerCharacterMasterController.instances[Random.Range(0, PlayerCharacterMasterController.instances.Count - 1)].body)
        self.targetPosition = PlayerCharacterMasterController.instances[0].body.footPosition;
      else if (spawnPoints.Count() > 0)
        self.targetPosition = spawnPoints[Random.Range(0, spawnPoints.Count() - 1)].transform.position;
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
      self.ai.SetGoalPosition(self.targetPosition);
      self.PickNewTargetLookPosition();
    }
  }
}