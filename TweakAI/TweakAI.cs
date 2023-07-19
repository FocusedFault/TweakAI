using BepInEx;
using RoR2;
using RoR2.Navigation;
using RoR2.CharacterAI;
using EntityStates;
using EntityStates.AI.Walker;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TweakAI
{
  [BepInPlugin("com.Nuxlar.TweakAI", "TweakAI", "1.0.0")]

  public class TweakAI : BaseUnityPlugin
  {
    private GameObject mWormMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MagmaWorm/MagmaWormMaster.prefab").WaitForCompletion();
    private GameObject eWormMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricWormMaster.prefab").WaitForCompletion();
    private GameObject[] masterArray = new GameObject[]
    {
      // Enemies
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/LemurianMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bison/BisonMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vulture/VultureMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/FlyingVermin/FlyingVerminMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Vermin/VerminMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MinorConstructMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBruiser/ClayBruiserMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ClayGrenadier/ClayGrenadierMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LemurianBruiser/LemurianBruiserMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Gup/GupMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/HermitCrab/HermitCrabMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/ImpMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Jellyfish/JellyfishMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GreaterWisp/GreaterWispMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Wisp/WispMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RoboBallBoss/RoboBallMiniMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Parent/ParentMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/GolemMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/MiniMushroomMaster.prefab").WaitForCompletion(),
      // Bosses
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBoss/ClayBossMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Gravekeeper/GravekeeperMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scav/ScavMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RoboBallBoss/RoboBallBossMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/TitanMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabMaster.prefab").WaitForCompletion(),
      Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MegaConstructMaster.prefab").WaitForCompletion()
    };

    public void Awake()
    {
      WormMasterChanges();
      GeneralMasterChanges();
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
    }

    private void WormMasterChanges()
    {
      AISkillDriver blinkDriver1 = mWormMaster.GetComponents<AISkillDriver>().Where<AISkillDriver>(x => x.customName == "Blink").First<AISkillDriver>();
      AISkillDriver blinkDriver2 = eWormMaster.GetComponents<AISkillDriver>().Where<AISkillDriver>(x => x.customName == "Blink").First<AISkillDriver>();
      Destroy(blinkDriver1);
      Destroy(blinkDriver2);
    }

    private void GeneralMasterChanges()
    {
      foreach (GameObject master in this.masterArray)
      {
        EntityStateMachine component = master.GetComponent<EntityStateMachine>();
        if ((bool)(UnityEngine.Object)component)
        {
          component.initialStateType = new SerializableEntityStateType(typeof(Combat));
          component.mainStateType = new SerializableEntityStateType(typeof(Wander));
        }
      }
    }
  }
}