using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Technie.PhysicsCreator
{
	public class HullMapping
	{
		public Hull sourceHull;
		public Collider generatedCollider;
		public HullPainterChild targetChild; // null if non-child
	}

	public class HullPainter : MonoBehaviour
	{
		public PaintingData paintingData;
		public HullData hullData;

	//	private Dictionary<Hull, Collider> hullMapping; // todo: change this to List/array so it can be serialised properly?
		private List<HullMapping> hullMapping;
		
		void OnDestroy()
		{
#if UNITY_EDITOR
			SceneView.RepaintAll();
#endif
		}

		public void CreateColliderComponents ()
		{
			CreateHullMapping ();

			foreach (Hull hull in paintingData.hulls)
			{
				UpdateCollider(hull);
			}
		}

		/** Remove all colliders, leave child objects and HullPainterChild components for later recreation */
		public void RemoveAllColliders ()
		{
			// Don't immediately refresh the hull mappings, as that could create new child objects and we're just trying to delete existing ones

			if (hullMapping != null)
			{
				// Destroy just the generated colliders
				foreach (HullMapping mapping in hullMapping)
				{
					DestroyImmediateWithUndo(mapping.generatedCollider);
				}

				// Delete all mappings that aren't child mappings (as they will have no collider now)
				for (int i=hullMapping.Count-1; i>=0; i--)
				{
					if (hullMapping[i].targetChild != null)
					{
						hullMapping.RemoveAt(i);
					}
				}
			}
		}

		/** Removes all generated components - colliders and child objects with child colliders and HullPainterChild on them */
		public void RemoveAllGenerated()
		{
			CreateHullMapping();

			foreach (HullMapping mapping in hullMapping)
			{
				DestroyImmediateWithUndo(mapping.generatedCollider);
				if (mapping.targetChild != null)
				{
					DestroyImmediateWithUndo(mapping.targetChild.gameObject);
				}
			}
		}

		private static bool IsDeletable(GameObject obj)
		{
			Component[] allComps = obj.GetComponents<Component>();

			int numIgnorable = 0;

			foreach (Component comp in allComps)
			{
				if (comp is Transform
					|| comp is Collider
					|| comp is HullPainter
					|| comp is HullPainterChild)
				{
					numIgnorable++;
				}
			}

			return allComps.Length == numIgnorable;
		}

		private static void DestroyImmediateWithUndo(Object obj)
		{
			if (obj == null)
				return;
#if UNITY_EDITOR
			Undo.DestroyObjectImmediate(obj);
#else
			GameObject.DestroyImmediate(obj);
#endif
		}

		private void CreateHullMapping()
		{
			if (hullMapping == null)
			{
				//	hullMapping = new Dictionary<Hull, Collider>();
				hullMapping = new List<HullMapping>();
			}

			// Remove any invalid entries from the hull mapping
			//	null entries are garbage and can be dropped
			//	null source hull means the hull has been deleted and this mapping is no longer relevant
			//	missing *both* generated collider *and* target child means there's no data to point at, so might as well remove it and regen from scratch
			for (int i = hullMapping.Count - 1; i >= 0; i--)
			{
				HullMapping mapping = hullMapping[i];
				if (mapping == null
					|| mapping.sourceHull == null
					|| (mapping.generatedCollider == null && mapping.targetChild == null))
				{
					hullMapping.RemoveAt(i);
				}
			}

			// Check to see if any existing mappings need updating (hull.type doesn't match Collider type, or child type no longer matches)

			foreach (Hull hull in paintingData.hulls)
			{
				if (IsMapped(hull))
				{
					// We already have a mapping for this, but is it still of the correct type?
					
					Collider value = FindExistingCollider(hullMapping, hull);

					bool isHullOk = (hull.type == HullType.ConvexHull && value is MeshCollider);
					bool isBoxOk = (hull.type == HullType.Box && value is BoxCollider);
					bool isSphereOk = (hull.type == HullType.Sphere && value is SphereCollider);
					bool isFaceOk = (hull.type == HullType.Face && value is MeshCollider);

					bool isColliderTypeOk = (isHullOk || isBoxOk || isSphereOk || isFaceOk);
					bool isChildTypeOk = value == null || ((hull.isChildCollider) == (value.transform.parent == this.transform));

					if (isColliderTypeOk && isChildTypeOk)
					{
						// All good
					}
					else
					{
						// Mismatch - hull.type doesn't match collider type
						// Delete the collider and remove the mapping
						// This hull will then be orphaned, and a new collider added back in accordingly
						DestroyImmediateWithUndo(value);
						RemoveMapping(hull);
					}
				}
			}

			// Connect orphans
			//
			// Find hulls without a Collider
			// Find Colliders without hulls
			// Try and map the two together

			// First find orphans - hull, colliders or childs that aren't already mapped

			List<Hull> orphanedHulls = new List<Hull>();
			List<Collider> orphanedColliders = new List<Collider>();
			List<HullPainterChild> orphanedChilds = new List<HullPainterChild>();

			foreach (Hull h in paintingData.hulls)
			{
				if (!IsMapped(h))
				{
					orphanedHulls.Add(h);
				}
			}
			
			foreach (Collider c in FindLocal<Collider>())
			{
				if (!IsMapped(c))
				{
					orphanedColliders.Add(c);
				}
			}

			foreach (HullPainterChild c in FindLocal<HullPainterChild>())
			{
				if (!IsMapped(c))
				{
					orphanedChilds.Add(c);
				}
			}

			// Try and connect orphaned hulls with orphaned colliders

			for (int i = orphanedHulls.Count - 1; i >= 0; i--)
			{
				Hull h = orphanedHulls[i];

				for (int j = orphanedColliders.Count - 1; j >= 0; j--)
				{
					Collider c = orphanedColliders[j];

					// Find the HullPainterChild adjacent to the collider (if a child collider)
					HullPainterChild child = null;
					if (c.transform.parent == this.transform)
					{
						child = c.gameObject.GetComponent<HullPainterChild>();
					}

					// todo needs better handling
					bool isMatchingChild = h.isChildCollider && c.transform.parent == this.transform;
					if (isMatchingChild)
					{
						BoxCollider boxCol = c as BoxCollider;
						SphereCollider sphereCol = c as SphereCollider;
						MeshCollider meshCol = c as MeshCollider;

						bool isMatchingBox = h.type == HullType.Box && c is BoxCollider && Approximately(h.collisionBox.center, boxCol.center) && Approximately(h.collisionBox.size, boxCol.size);
						bool isMatchingSphere = h.type == HullType.Sphere && c is SphereCollider && h.collisionSphere != null && Approximately(h.collisionSphere.center, sphereCol.center) && Approximately(h.collisionSphere.radius, sphereCol.radius);
						bool isMatchingConvexHull = h.type == HullType.ConvexHull && c is MeshCollider && meshCol.sharedMesh == h.collisionMesh;
						bool isMatchingFace = h.type == HullType.Face && c is MeshCollider && meshCol.sharedMesh == h.faceCollisionMesh;

						if (isMatchingBox || isMatchingSphere || isMatchingConvexHull || isMatchingFace)
						{
							// Found a pair, so add a mapping and remove the orphans
							AddMapping(h, c, child);

							// These are no longer orphaned, so remove them from these lists
							orphanedHulls.RemoveAt(i);
							orphanedColliders.RemoveAt(j);
							
							// Remove the no-longer orphaned child
							for (int k=0; k<orphanedChilds.Count; k++)
							{
								if (orphanedChilds[k] == child)
								{
									orphanedChilds.RemoveAt(k);
									break;
								}
							}

							break;
						}
					}
				}
			}

			
			// We've tried to connect hulls to existing colliders, now try and connect hulls to existing HullPainterChilds
			// These will be child without a collider (as otherwise they'd have be picked up earlier)
			for (int i = orphanedHulls.Count - 1; i >= 0; i--)
			{
				Hull h = orphanedHulls[i];

				if (!h.isChildCollider)
					continue;

				for (int j = orphanedChilds.Count - 1; j >= 0; j--)
				{
					HullPainterChild child = orphanedChilds[j];
					HullMapping mapping = FindMapping(child);

					if (mapping != null && mapping.sourceHull != null)
					{
						// Found a match for hull-mapping-child

						// Ensure this still has a collider
						if (mapping.generatedCollider == null)
						{
							// Recreate the collider of the correct type with the existing hull-mapping-child

							RecreateChildCollider(mapping);
						}

						orphanedHulls.RemoveAt(i);
						orphanedChilds.RemoveAt(j);
						break;
					}
				}
			}
			
			// Create colliders for any hull mapping children without colliders
			foreach (HullMapping mapping in hullMapping)
			{
				if (mapping.targetChild != null && mapping.generatedCollider == null)
					RecreateChildCollider(mapping);
			}

			// Create child components for child colliders without them
			foreach (HullMapping mapping in hullMapping)
			{
				if (mapping.targetChild == null && mapping.generatedCollider != null && mapping.generatedCollider.transform.parent == this.transform)
				{
					// Mapping has a child collider but no HullPainterChild
					// Recreate the child component
					HullPainterChild newChild = AddComponent<HullPainterChild>(mapping.generatedCollider.gameObject);
					newChild.parent = this;
					mapping.targetChild = newChild;
				}
			}

			// Create colliders for any left over hulls

			foreach (Hull h in orphanedHulls)
			{
				if (h.type == HullType.Box)
				{
					CreateCollider<BoxCollider>(h);
				}
				else if (h.type == HullType.Sphere)
				{
					CreateCollider<SphereCollider>(h);
				}
				else if (h.type == HullType.ConvexHull)
				{
					CreateCollider<MeshCollider>(h);
				}
				else if (h.type == HullType.Face)
				{
					CreateCollider<MeshCollider>(h);
				}
			}
			
			// Delete any left over colliders
			// TODO: This probably isn't properly undo-aware

			foreach (Collider c in orphanedColliders)
			{
				if (c.gameObject == this.gameObject)
				{
					DestroyImmediateWithUndo(c);
				}
				else
				{
					// Child collider - delete collider, HullPainterChild (if any) and GameObject (if empty)

					GameObject go = c.gameObject;
					DestroyImmediateWithUndo(c);
					DestroyImmediateWithUndo(go.GetComponent<HullPainterChild>());
					if (IsDeletable(go))
					{
						DestroyImmediateWithUndo(go);
					}
				}
			}

			// Delete any left over hull painter childs
			// TODO: This probably isn't undo-aware
			
			foreach (HullPainterChild child in orphanedChilds)
			{
				if (child == null)
					continue;

				// Delete child, collider (if any) and GameObject (if empty)
				GameObject go = child.gameObject;
				DestroyImmediateWithUndo(child);
				DestroyImmediateWithUndo(go.GetComponent<Collider>());
				if (IsDeletable(go))
				{
					DestroyImmediateWithUndo(go);
				}
			}

			// Sanity check - all hull mappings should have a collider of the right type now
		//	foreach (HullMapping mapping in hullMapping)
		//	{
		//		if (mapping.generatedCollider == null)
		//			Debug.LogWarning("Null collider for hull: " + mapping.sourceHull.name);
		//	}
		}

		private static bool Approximately(Vector3 lhs, Vector3 rhs)
		{
			return Mathf.Approximately (lhs.x, rhs.x) && Mathf.Approximately (lhs.y, rhs.y) && Mathf.Approximately (lhs.z, rhs.z);
		}
		private static bool Approximately(float lhs, float rhs)
		{
			return Mathf.Approximately(lhs, rhs);
		}

		private void CreateCollider<T>(Hull sourceHull) where T : Collider
		{
			if (sourceHull.isChildCollider)
			{
			//	GameObject newChild = new GameObject(sourceHull.name);
				GameObject newChild = CreateGameObject(sourceHull.name);
				newChild.transform.SetParent(this.transform, false);
				newChild.transform.localPosition = Vector3.zero;
				newChild.transform.localRotation = Quaternion.identity;
				newChild.transform.localScale = Vector3.one;

				HullPainterChild childPainter = AddComponent<HullPainterChild>(newChild);
				childPainter.parent = this;

				T col = AddComponent<T>(newChild);
				AddMapping(sourceHull, col, childPainter);
			}
			else
			{
				T col = AddComponent<T>(this.gameObject);
				AddMapping(sourceHull, col, null);
			}
		}

		private void RecreateChildCollider(HullMapping mapping)
		{
			if (mapping == null || mapping.sourceHull == null || !mapping.sourceHull.isChildCollider)
				return;

			if (mapping.sourceHull.type == HullType.Box)
			{
				RecreateChildCollider<BoxCollider>(mapping);
			}
			else if (mapping.sourceHull.type == HullType.Sphere)
			{
				RecreateChildCollider<SphereCollider>(mapping);
			}
			else if (mapping.sourceHull.type == HullType.ConvexHull)
			{
				RecreateChildCollider<MeshCollider>(mapping);
			}
			else if (mapping.sourceHull.type == HullType.Face)
			{
				RecreateChildCollider<MeshCollider>(mapping);
			}
		}

		private void RecreateChildCollider<T>(HullMapping mapping) where T : Collider
		{
			if (mapping.sourceHull == null || !mapping.sourceHull.isChildCollider)
				return;
			
			T col = AddComponent<T>(mapping.targetChild.gameObject);
			mapping.generatedCollider = col;
		}

		// Updates the existing collider for this hull
		// A collider of the correct type wil already exist, we just need to sync up the latest position/size/etc. data from the hull to the collider
		private void UpdateCollider(Hull hull)
		{
			Collider c = null;

			if (hull.type == HullType.Box)
			{
				BoxCollider boxCollider = FindExistingCollider(hullMapping, hull) as BoxCollider;
				boxCollider.center = hull.collisionBox.center;
				boxCollider.size = hull.collisionBox.size + (hull.enableInflation ? Vector3.one * hull.inflationAmount : Vector3.zero);
				c = boxCollider;
			}
			else if (hull.type == HullType.Sphere)
			{
				SphereCollider sphereCollider = FindExistingCollider(hullMapping, hull) as SphereCollider;
				sphereCollider.center = hull.collisionSphere.center;
				sphereCollider.radius = hull.collisionSphere.radius + (hull.enableInflation ? hull.inflationAmount : 0.0f);
				c = sphereCollider;
			}
			else if (hull.type == HullType.ConvexHull)
			{
				MeshCollider meshCollider = FindExistingCollider(hullMapping, hull) as MeshCollider;
				meshCollider.sharedMesh = hull.collisionMesh;
				meshCollider.convex = true;
#if !UNITY_2018_4_OR_NEWER
				meshCollider.inflateMesh = hull.enableInflation;
				meshCollider.skinWidth = hull.inflationAmount;
#endif
				c = meshCollider;
			}
			else if (hull.type == HullType.Face)
			{
				MeshCollider faceCollider = FindExistingCollider(hullMapping, hull) as MeshCollider;
				faceCollider.sharedMesh = hull.faceCollisionMesh;
				faceCollider.convex = true;
				c = faceCollider;
			}

			if (c != null)
			{
				c.material = hull.material;
				c.isTrigger = hull.isTrigger;

				// Sync the child object's name with the hull
				if (hull.isChildCollider)
				{
					c.gameObject.name = hull.name;
				}
			}
		}

		public void SetAllTypes (HullType newType)
		{
			foreach (Hull h in paintingData.hulls)
			{
				h.type = newType;
			}
		}

		public void SetAllMaterials (PhysicMaterial newMaterial)
		{
			foreach (Hull h in paintingData.hulls)
			{
				h.material = newMaterial;
			}
		}

		public void SetAllAsChild(bool isChild)
		{
			foreach (Hull h in paintingData.hulls)
			{
				h.isChildCollider = isChild;
			}
		}

		public void SetAllAsTrigger(bool isTrigger)
		{
			foreach (Hull h in paintingData.hulls)
			{
				h.isTrigger = isTrigger;
			}
		}

		private List<T> FindLocal<T>() where T : Component
		{
			List<T> localComps = new List<T>();

			localComps.AddRange(this.gameObject.GetComponents<T>());

			for (int i=0; i<transform.childCount; i++)
			{
				localComps.AddRange(transform.GetChild(i).GetComponents<T>());
			}

			return localComps;
		}

		private bool IsMapped(Hull hull)
		{
			if (hullMapping == null)
				return false;

			foreach (HullMapping map in hullMapping)
			{
				if (map.sourceHull == hull)
					return true;
			}
			return false;
		}

		private bool IsMapped(Collider col)
		{
			if (hullMapping == null)
				return false;

			foreach (HullMapping map in hullMapping)
			{
				if (map.generatedCollider == col)
					return true;
			}
			return false;
		}

		private bool IsMapped(HullPainterChild child)
		{
			if (hullMapping == null)
				return false;

			foreach (HullMapping map in hullMapping)
			{
				if (map.targetChild == child)
					return true;
			}
			return false;
		}

		private void AddMapping(Hull hull, Collider col, HullPainterChild painterChild)
		{
			HullMapping newMapping = new HullMapping()
			{
				sourceHull = hull,
				generatedCollider = col,
				targetChild = painterChild
			};

			this.hullMapping.Add(newMapping);
		}

		private void RemoveMapping(Hull hull)
		{
			for (int i=0; i<hullMapping.Count; i++)
			{
				if (hullMapping[i].sourceHull == hull)
				{
					hullMapping.RemoveAt(i);
					return;
				}
			}
		}

		private HullMapping FindMapping(HullPainterChild child)
		{
			if (hullMapping == null)
				return null;

			foreach (HullMapping h in hullMapping)
			{
				if (h.targetChild == child)
					return h;
			}
			return null;
		}

		public Hull FindSourceHull(HullPainterChild child)
		{
			if (hullMapping == null)
			{
				// TODO: Hull mapping should be serialised, when it is remove this message as it'll only exist to catch un-upgraded assets
			//	Debug.LogError("No hull mapping present!");
				return null;
			}

			foreach (HullMapping h in hullMapping)
			{
				if (h.targetChild == child)
					return h.sourceHull;
			}
			return null;
		}

		private static Collider FindExistingCollider(List<HullMapping> mappings, Hull hull)
		{
			foreach (HullMapping map in mappings)
			{
				if (map.sourceHull == hull)
				{
					return map.generatedCollider;
				}
			}
			return null;
		}

		private static GameObject CreateGameObject(string goName)
		{
			GameObject go = new GameObject(goName);
#if UNITY_EDITOR
			Undo.RegisterCreatedObjectUndo(go, "Created "+goName);
#endif
			return go;
		}

		private static T AddComponent<T>(GameObject targetObj) where T : Component
		{
#if UNITY_EDITOR
			return (T)Undo.AddComponent(targetObj, typeof(T));
#else
			return targetObj.AddComponent<T>();
#endif
		}
	}

} // namespace Technie.PhysicsCreator

