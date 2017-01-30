using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astrogator {

	using static DebugTools;
	using static PhysicsTools;
	using static KerbalTools;

	/// Container for all the transfers we want to know about.
	public class AstrogationModel {

		/// <summary>
		/// Transfers to calculate and show in the window.
		/// </summary>
		public List<TransferModel> transfers { get; private set; }

		/// <summary>
		/// Origin body for all the current transfers.
		/// </summary>
		public CelestialBody body { get; private set; }

		/// <summary>
		/// Vessel for calculating transfers.
		/// </summary>
		public Vessel vessel { get; private set; }

		/// <summary>
		/// The inclination past which we refuse to do any calculations.
		/// Most of our approximations and heuristics work best at equatorial
		/// and get less accurate rapidly.
		/// </summary>
		public const double maxInclination = Tau / 12.0;

		/// <summary>
		/// True if the vessel's inclination is too big to be worth bothering.
		/// </summary>
		public bool badInclination {
			get {
				// Orbit.inclination is in degrees
				return vessel != null
					&& Math.Abs(vessel.orbit.inclination * Mathf.Deg2Rad) > maxInclination;
			}
		}

		/// <summary>
		/// Construct a model object for the given origin objects.
		/// </summary>
		/// <param name="b">Body to start at, overridden by v</param>
		/// <param name="v">Vessel to start at</param>
		public AstrogationModel(CelestialBody b = null, Vessel v = null)
		{
			body = b;
			vessel = v;
			transfers = new List<TransferModel>();

			if (!badInclination) {
				CreateTransfers(b, v);
			}
		}

		/// <summary>
		/// Re-initialize a model object for the given origin objects.
		/// </summary>
		/// <param name="b">Body to start at, overridden by v</param>
		/// <param name="v">Vessel to start at</param>
		public void Reset(CelestialBody b = null, Vessel v = null)
		{
			body = b;
			vessel = v;
			transfers = new List<TransferModel>();

			if (!badInclination) {
				CreateTransfers(b, v);
			}
		}

		/// <returns>
		/// Description of the transfers contained in this model.
		/// </returns>
		public string OriginDescription {
			get {
				if (vessel != null) {
					return vessel.GetName();
				} else if (body != null) {
					return body.theName;
				} else {
					return "";
				}
			}
		}

		private void CreateTransfers(CelestialBody body, Vessel vessel)
		{
			DbgFmt("Fabricating transfers");

			CelestialBody origin = StartBody(body, vessel);

			for (CelestialBody b = origin, toSkip = null;
					b != null;
					toSkip = b, b = ParentBody(b)) {

				DbgFmt("Plotting transfers around {0}", b.theName);

				int numBodies = b.orbitingBodies.Count;
				for (int i = 0; i < numBodies; ++i) {
					CelestialBody satellite = b.orbitingBodies[i];
					if (satellite != toSkip) {
						DbgFmt("Plotting transfer to {0}", satellite.theName);
						transfers.Add(new TransferModel(origin, satellite, vessel));
						DbgFmt("Finalized transfer to {0}", satellite.theName);
					}
				}
				DbgFmt("Exhausted transfers around {0}", b.theName);
			}
			DbgFmt("Shipping completed transfers");
		}
	}

}