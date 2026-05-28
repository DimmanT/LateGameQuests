using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace LoGiQ
{
    public abstract class JobGiver_ManTurretsCE : ThinkNode_JobGiver
    {
        public float maxDistFromPoint = -1f;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_ManTurretsCE obj = (JobGiver_ManTurretsCE)base.DeepCopy(resolve);
            obj.maxDistFromPoint = maxDistFromPoint;
            return obj;
        }

		protected override Job TryGiveJob(Pawn pawn)
		{
			Thing thing = GenClosest.ClosestThingReachable(GetRoot(pawn), pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.InteractionCell, TraverseParms.For(pawn), maxDistFromPoint, Validator);
			if (thing != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.ManTurret, thing);
				job.expiryInterval = 2000;
				job.checkOverrideOnExpire = true;
				return job;
			}
			return null;
			bool Validator(Thing t)
			{
				if (!t.def.hasInteractionCell)
				{
					return false;
				}
				if (!t.def.HasComp(typeof(CompMannable)))
				{
					return false;
				}
				if (!pawn.CanReserve(t))
				{
					return false;
				}
				return true;
			}
		}

		protected abstract IntVec3 GetRoot(Pawn pawn);
	}

    public class JobGiver_ManTurretsNearSelfCE : JobGiver_ManTurretsCE
    {
        protected override IntVec3 GetRoot(Pawn pawn)
        {
            return pawn.Position;
        }
    }
}
