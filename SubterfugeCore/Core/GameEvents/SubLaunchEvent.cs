﻿using Microsoft.Xna.Framework;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;
using System;

namespace SubterfugeCore.GameEvents
{
    public class SubLaunchEvent : GameEvent
    {
        private GameTick launchTime;
        private Outpost sourceOutpost;
        private ITargetable destination;
        private int drillerCount;
        private Sub launchedSub;

        public SubLaunchEvent(GameTick launchTime, Outpost sourceOutpost, int drillerCount, ITargetable destination)
        {
            this.launchTime = launchTime;
            this.sourceOutpost = sourceOutpost;
            this.drillerCount = drillerCount;
            this.destination = destination;
            this.launchedSub = new Sub(sourceOutpost, destination, launchTime, drillerCount, sourceOutpost.getOwner());

            if (destination.GetType().Equals(typeof(Outpost)))
            {
                SubArriveEvent arrivalEvent = new SubArriveEvent(launchedSub, this.destination, launchedSub.getExpectedArrival());
                GameServer.timeMachine.addEvent(arrivalEvent);
            } else
            {
                Vector2 targetLocation = this.destination.getTargetLocation(sourceOutpost.getPosition(), this.launchedSub.getSpeed());
                GameTick arrival = this.launchTime.advance((int)Math.Floor((targetLocation - sourceOutpost.getPosition()).Length() / this.launchedSub.getSpeed()));
                SubCombatEvent combatEvent = new SubCombatEvent(this.launchedSub, (Sub)destination, arrival,  targetLocation);
                GameServer.timeMachine.addEvent(combatEvent);
            }
            this.createCombatEvents();
        }

        public override void eventBackwardAction()
        {
            GameState state = GameServer.timeMachine.getState();

            sourceOutpost.addDrillers(this.drillerCount);
            state.removeSub(this.launchedSub);
        }

        public override void eventForwardAction()
        {
            if (sourceOutpost.hasDrillers(drillerCount))
            {
                GameState state = GameServer.timeMachine.getState();

                sourceOutpost.removeDrillers(drillerCount);
                state.addSub(this.launchedSub);
            }
        }

        public void createCombatEvents()
        {
            // Determine any combat events that may exist along the way.
            // First determine if any subs are on the same path.
            if (this.destination.GetType().Equals(typeof(Outpost)))
            {
                // Interpolate to launch time to determine combats!
                GameTick currentTick = GameServer.timeMachine.getCurrentTick();
                GameServer.timeMachine.goTo(this.getTick());
                GameState interpolatedState = GameServer.timeMachine.getState();


                foreach (Sub sub in interpolatedState.getSubsOnPath(this.sourceOutpost, (Outpost)this.destination))
                {
                    // Don't combat with yourself
                    if(sub == this.getActiveSub())
                        continue;

                    // Determine if we combat it
                    if (sub.getDirection() == this.getActiveSub().getDirection())
                    {
                        if (this.getActiveSub().getExpectedArrival() < sub.getExpectedArrival())
                        {
                            // We can catch it. Determine when and create a combat event.
                        }
                    }
                    else
                    {
                        // Sub is moving towards us.
                        if (sub.getOwner() != this.getActiveSub().getOwner())
                        {
                            // Combat will occur
                            // Determine when and create a combat event.

                            // Determine the number of ticks between the incoming sub & the launched sub.
                            int ticksBetweenSubs = sub.getExpectedArrival() - this.launchTime;

                            // Determine the speed ratio as a number between 0-0.5
                            double speedRatio = (sub.getSpeed() / this.getActiveSub().getSpeed()) - 0.5;

                            int ticksUntilCombat = (int)Math.Floor(speedRatio * ticksBetweenSubs);

                            // Determine collision location:
                            Vector2 combatLocation = Vector2.Multiply(this.getActiveSub().getDirection(), (float)ticksUntilCombat);

                            SubCombatEvent combatEvent = new SubCombatEvent(sub, this.getActiveSub(), GameServer.timeMachine.getState().getCurrentTick().advance(ticksUntilCombat), combatLocation);
                            GameServer.timeMachine.addEvent(combatEvent);
                        }
                    }
                }
                // Go back to the original point in time.
                GameServer.timeMachine.goTo(currentTick);
            }
        }

        public override GameTick getTick()
        {
            return this.launchTime;
        }

        public Sub getActiveSub()
        {
            return this.launchedSub;
        }

        public ITargetable getDestination()
        {
            return this.destination;
        }

        public Outpost getSourceOutpost()
        {
            return this.sourceOutpost;
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }
    }
}
