﻿using SC2APIProtocol;
using Tyr.Agents;

namespace Tyr.Micro
{
    public class ZerglingController : CustomController
    {
        private static ulong BanelingHunter = 0;
        private static int BanelingHunterFrame = -1;
        public bool DetermineAction(Agent agent, Point2D target)
        {
            if (agent.Unit.UnitType != UnitTypes.ZERGLING)
                return false;

            if (agent.Unit.Tag == BanelingHunter)
            {
                Unit targetBaneling = null;
                float distance = 9;
                foreach (Unit enemy in Tyr.Bot.Enemies())
                {
                    if (enemy.UnitType != UnitTypes.BANELING)
                        continue;
                    float newDist = agent.DistanceSq(enemy);
                    if (newDist < distance)
                    {
                        targetBaneling = enemy;
                        distance = newDist;
                    }
                }
                if (targetBaneling != null)
                {
                    agent.Order(Abilities.ATTACK, targetBaneling.Tag);
                    BanelingHunter = agent.Unit.Tag;
                    BanelingHunterFrame = Tyr.Bot.Frame;
                    return true;
                }
                BanelingHunterFrame = 0;
                BanelingHunter = 0;
            }

            PotentialHelper potential = new PotentialHelper(agent.Unit.Pos);
            potential.Magnitude = 4;
            bool flee = false;
            foreach (Unit enemy in Tyr.Bot.Enemies())
                if (enemy.UnitType == UnitTypes.BANELING && agent.DistanceSq(enemy) <= 3 * 3)
                {
                    potential.From(enemy.Pos);
                    flee = true;
                    if (Tyr.Bot.Frame - BanelingHunterFrame >= 2)
                    {
                        agent.Order(Abilities.ATTACK, enemy.Tag);
                        BanelingHunter = agent.Unit.Tag;
                        BanelingHunterFrame = Tyr.Bot.Frame;
                    }
                }

            if (!flee)
                return false;
            agent.Order(Abilities.MOVE, potential.Get());
            return true;
        }
    }
}
