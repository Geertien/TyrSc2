﻿using SC2APIProtocol;
using Tyr.Agents;
using Tyr.Util;

namespace Tyr.Micro
{
    public class FearVikingsController : CustomController
    {
        public bool Stopped = false;
        public bool DetermineAction(Agent agent, Point2D target)
        {
            if (Stopped || agent.Unit.UnitType != UnitTypes.VOID_RAY)
                return false;
            

            float dist = 12 * 12;
            Unit fleeTarget = null;
            foreach (Unit enemy in Tyr.Bot.Enemies())
            {
                if (enemy.UnitType != UnitTypes.VIKING_FIGHTER)
                    continue;

                float newDist = agent.DistanceSq(enemy);
                if (newDist < dist)
                {
                    fleeTarget = enemy;
                    dist = newDist;
                }
            }

            if (fleeTarget != null)
            {
                PotentialHelper helper = new PotentialHelper(agent.Unit.Pos);
                helper.Magnitude = 8;
                helper.From(fleeTarget.Pos);
                agent.Order(Abilities.MOVE, helper.Get());
                return true;
            }
            
            return false;
        }
    }
}
