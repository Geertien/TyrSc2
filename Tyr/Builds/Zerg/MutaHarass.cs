﻿using SC2APIProtocol;
using Tyr.Agents;
using Tyr.Builds.BuildLists;
using Tyr.Micro;
using Tyr.Tasks;
using Tyr.Util;

namespace Tyr.Builds.Zerg
{
    public class MutaHarass : Build
    {
        TimingAttackTask TimingAttackTask = new TimingAttackTask() { RequiredSize = 20, UnitType = UnitTypes.MUTALISK };
        private bool SmellCheese = false;

        public override string Name()
        {
            return "MutaHarass";
        }

        public override void OnStart(Tyr tyr)
        {
            MicroControllers.Add(new HitAndRunController());
            MicroControllers.Add(new TargetFireController(GetPriorities()));
            tyr.TaskManager.Add(TimingAttackTask);
            if (tyr.EnemyRace != Race.Protoss)
                tyr.TaskManager.Add(new WorkerScoutTask());
            tyr.TaskManager.Add(new QueenInjectTask(Main));
            tyr.TaskManager.Add(new QueenDefenseTask());
            tyr.TaskManager.Add(new DefenseTask() { MainDefenseRadius = 20, ExpandDefenseRadius = 15, MaxDefenseRadius = 55 });
            Set += MainBuild();
        }

        public PriorityTargetting GetPriorities()
        {
            PriorityTargetting priorities = new PriorityTargetting();

            priorities.DefaultPriorities.MaxRange = 10;
            priorities.DefaultPriorities[UnitTypes.LARVA] = -1;
            priorities.DefaultPriorities[UnitTypes.EGG] = -1;
            priorities.DefaultPriorities[UnitTypes.ROACH] = -1;

            foreach (uint t in UnitTypes.AirAttackTypes)
                priorities[UnitTypes.MUTALISK][t] = 1;

            return priorities;
        }

        private BuildList MainBuild()
        {
            BuildList result = new BuildList();
            result.Building(UnitTypes.HATCHERY, 2);
            result.Building(UnitTypes.SPAWNING_POOL);
            result.If(() => { return Count(UnitTypes.QUEEN) > 0; });
            result.If(() => { return Completed(UnitTypes.HATCHERY) + Completed(UnitTypes.LAIR) >= 2 && Tyr.Bot.Frame >= 22.4 * 60 * 2.2; });
            result.Building(UnitTypes.SPINE_CRAWLER, Natural, NaturalDefensePos, 2);
            result.Building(UnitTypes.SPINE_CRAWLER, Natural, NaturalDefensePos, () => { return SmellCheese; });
            result.If(() => { return Count(UnitTypes.ZERGLING) >= 20 && Count(UnitTypes.DRONE) >= 20; });
            result.Building(UnitTypes.EXTRACTOR);
            result.Building(UnitTypes.EXTRACTOR);
            result.Building(UnitTypes.SPIRE);
            result.Building(UnitTypes.EXTRACTOR, 4);
            return result;
        }

        public override void OnFrame(Tyr tyr)
        {
            if (tyr.EnemyStrategyAnalyzer.FourRaxDetected
                || (tyr.Frame >= 22.4 * 85 && !tyr.EnemyStrategyAnalyzer.NoProxyTerranConfirmed && tyr.TargetManager.PotentialEnemyStartLocations.Count == 1)
                || tyr.EnemyStrategyAnalyzer.ReaperRushDetected)
            {
                SmellCheese = true;
            }
            
            foreach (Agent agent in tyr.UnitManager.Agents.Values)
            {
                if (agent.Unit.UnitType == UnitTypes.LARVA)
                {
                    if (Count(UnitTypes.DRONE) >= 11 && Count(UnitTypes.SPAWNING_POOL) == 0)
                        break;
                    if (Minerals() >= 50
                        && ExpectedAvailableFood() > FoodUsed() + 2
                        && Count(UnitTypes.DRONE) < 45 - Completed(UnitTypes.EXTRACTOR)
                        && (Count(UnitTypes.DRONE) < 40 - Completed(UnitTypes.EXTRACTOR) || Count(UnitTypes.HATCHERY) + Count(UnitTypes.LAIR) + Count(UnitTypes.HIVE) >= 3)
                        && (Count(UnitTypes.ZERGLING) >= 10 || Count(UnitTypes.DRONE) <= 14)
                        && (Count(UnitTypes.ZERGLING) >= 20 || Count(UnitTypes.DRONE) <= 18))
                    {
                        agent.Order(1342);
                        CollectionUtil.Increment(tyr.UnitManager.Counts, UnitTypes.DRONE);
                    }
                    else if (Completed(UnitTypes.SPAWNING_POOL) > 0
                        && Count(UnitTypes.ZERGLING) < 20
                        && Minerals() >= 50
                        && ExpectedAvailableFood() > FoodUsed() + 4
                        && (Count(UnitTypes.SPINE_CRAWLER) >= 2 || Minerals() >= 200))
                    {
                        agent.Order(1343);
                        CollectionUtil.Increment(tyr.UnitManager.Counts, UnitTypes.ZERGLING);
                        CollectionUtil.Increment(tyr.UnitManager.Counts, UnitTypes.ZERGLING);
                    }
                    else if (Completed(UnitTypes.SPIRE) + Completed(UnitTypes.GREATER_SPIRE) > 0
                        && Minerals() >= 100
                        && Gas() >= 100
                        && ExpectedAvailableFood() > FoodUsed() + 4)
                    {
                        agent.Order(1346);
                        CollectionUtil.Increment(tyr.UnitManager.Counts, UnitTypes.MUTALISK);
                    }
                    else if (Minerals() >= 100 && FoodUsed()
                        + Tyr.Bot.UnitManager.Count(UnitTypes.HATCHERY) * 2
                        + Tyr.Bot.UnitManager.Count(UnitTypes.LAIR) * 2
                        + Tyr.Bot.UnitManager.Count(UnitTypes.HIVE) * 2
                        >= ExpectedAvailableFood() - 2)
                    {
                        agent.Order(1344);
                        CollectionUtil.Increment(tyr.UnitManager.Counts, UnitTypes.OVERLORD);
                        tyr.UnitManager.FoodExpected += 8;
                    }
                }
            }
        }

        public override void Produce(Tyr tyr, Agent agent)
        {
            if (UnitTypes.ResourceCenters.Contains(agent.Unit.UnitType))
            {
                if (Minerals() >= 100 && Completed(UnitTypes.QUEEN) < 2
                    && Completed(UnitTypes.SPAWNING_POOL) > 0)
                {
                    agent.Order(1632);
                    CollectionUtil.Increment(tyr.UnitManager.Counts, UnitTypes.QUEEN);
                }
                else if (Count(UnitTypes.SPINE_CRAWLER) > 0
                    && Minerals() >= 150 && Gas() >= 100
                    && Count(UnitTypes.LAIR) + Count(UnitTypes.HIVE) == 0
                    && Count(UnitTypes.ZERGLING) >= 20)
                    agent.Order(1216);
            }
        }
    }
}
