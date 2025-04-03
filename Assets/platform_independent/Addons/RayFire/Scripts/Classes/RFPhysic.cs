using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{ 
    [Serializable]
    public class RFPhysic
    {
        [Header("  Physic Material")]
        [Space(1)]
        
        [Tooltip("Material preset with predefined density, friction, elasticity and solidity. Can be edited in Rayfire Man component.")]
        public MaterialType materialType;
        
        [Space(1)]
        [Tooltip("Allows to define own Physic Material.")]
        public PhysicMaterial material;
        
        [Header("  Mass")]
        [Space(1)]
        
        public MassType massBy;
        [Space(1)]
        [Range(0, 100f)] public float mass;
        
        [Header ("  Collider")]
        [Space(1)]
        
        public RFColliderType colliderType;

        //public CollisionDetectionMode collisionDetection;
        
        // Hidden
        [HideInInspector] public bool recorder;
        [HideInInspector] public bool exclude;
        [HideInInspector] public Quaternion rotation;
        [HideInInspector] public Vector3 position;
        [HideInInspector] public Vector3 velocity;
        [HideInInspector] public float collMult;
        [HideInInspector] public Vector3 birthPos;
        [HideInInspector] public Collider meshCollider;
        [HideInInspector] public List<Collider> clusterColliders;
        [HideInInspector] public Rigidbody rigidBody;
                
        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFPhysic()
        {
            colliderType = RFColliderType.Mesh;
            materialType = MaterialType.Concrete;
            material = null;
            massBy = MassType.MaterialDensity;
            mass = 0;
            //collisionDetection = CollisionDetectionMode.ContinuousDynamic;
            
            // Hidden
            recorder = false;
            exclude = false;
            rotation = Quaternion.identity;
            position = Vector3.zero;
            velocity = Vector3.zero;
            collMult = 0.8f;
        }

        // Copy from
        public void CopyFrom(RFPhysic physics)
        {
            colliderType = physics.colliderType;
            materialType = physics.materialType;
            material = physics.material;
            massBy = physics.massBy;
            mass = physics.mass;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Simulation Type
        /// /////////////////////////////////////////////////////////
        
        // Set simulation type properties
        public static void SetSimulationType(RayfireRigid scr)
        {
            // Dynamic
            if (scr.simulationType == SimType.Dynamic)
                SetDynamic(scr);

            // Sleeping 
            else if (scr.simulationType == SimType.Sleeping)
                SetSleeping(scr);

            // Inactive
            else if (scr.simulationType == SimType.Inactive)
                SetInactive(scr);

            // Kinematic
            else if (scr.simulationType == SimType.Kinematic)
                SetKinematic(scr);

            // Static
            else if (scr.simulationType == SimType.Static)
                SetStatic(scr);
        }

        // Set as dynamic
        static void SetDynamic(RayfireRigid scr)
        {
            // Set dynamic rigid body properties
            scr.physics.rigidBody.isKinematic = false;
            scr.physics.rigidBody.useGravity = true;

            // Turn of all activation properties
            scr.activation.TurnOff();
        }

        // Set as sleeping
        static void SetSleeping(RayfireRigid scr)
        {
            // Set dynamic rigid body properties
            scr.physics.rigidBody.isKinematic = false;
            scr.physics.rigidBody.useGravity = true;

            // Turn of all activation properties
            scr.activation.TurnOff();

            // Set sleep
            scr.physics.rigidBody.Sleep();
        }

        // Set as inactive
        static void SetInactive(RayfireRigid scr)
        {
            scr.physics.rigidBody.isKinematic = false;
            scr.physics.rigidBody.useGravity = false;

            // Set sleep
            scr.physics.rigidBody.Sleep();
        }

        // Set as Kinematic
        static void SetKinematic(RayfireRigid scr)
        {
            scr.physics.rigidBody.isKinematic = true;
            scr.physics.rigidBody.useGravity = false;
        }

        // Set as Static
        static void SetStatic(RayfireRigid scr)
        {
            // Set Static rigid body properties
            scr.physics.rigidBody.isKinematic = true;
            scr.physics.rigidBody.useGravity = false;
            scr.physics.rigidBody.mass = 0f;

            // Turn of all activation properties
            scr.activation.TurnOff();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rigid body
        /// /////////////////////////////////////////////////////////
        
        // Set density. After collider defined.
        public void SetDensity(Rigidbody rb)
        {
            // Do not set mass for kinematik
            if (rb.isKinematic == true)
                return;
                
            // Default mass from inspector
            float m = mass;

            // TODO  fragments inherit density, distribute mass from mesh root to all fragments

            
            // Get mass by density
            if (massBy == MassType.MaterialDensity)
            {
                rb.SetDensity(RayfireMan.inst.materialPresets.Density(materialType));
                m = rb.mass;
            }
            
            // Check for minimum mass
            if (RayfireMan.inst.minimumMass > 0)
                if (m < RayfireMan.inst.minimumMass)
                    m = RayfireMan.inst.minimumMass;
            
            // Check for maximum mass
            if (RayfireMan.inst.maximumMass > 0)
                if (m > RayfireMan.inst.maximumMass)
                    m = RayfireMan.inst.maximumMass;
            
            // Update mass in inspector
            rb.mass = m;
        }
        
        // Set drag
        public void SetDrag(Rigidbody rb)
        {
            // Set drag properties
            rb.drag        = (RayfireMan.inst.materialPresets.Drag(materialType));
            rb.angularDrag = (RayfireMan.inst.materialPresets.AngularDrag(materialType));
        }

        /// /////////////////////////////////////////////////////////
        /// Collider
        /// /////////////////////////////////////////////////////////
        
        // Set fragments collider
        public static void SetFragmentMeshCollider(RayfireRigid scr, Mesh mesh)
        {
            // Custom collider
            scr.physics.colliderType = scr.meshDemolition.properties.colliderType;
            if (scr.meshDemolition.properties.sizeFilter > 0)
                if (mesh.bounds.size.magnitude < scr.meshDemolition.properties.sizeFilter)
                    scr.physics.colliderType = RFColliderType.None;
            
            // Skip collider
            SetMeshCollider (scr, mesh);
        }
        
        // Set fragments collider
        public static void SetMeshCollider (RayfireRigid scr, Mesh mesh = null)
        {
            // Skip collider
            if (scr.physics.colliderType == RFColliderType.None)
                return;
            
            // No collider. Add own // TODO set non convex shape for collider
            if (scr.physics.meshCollider == null)
            {
                // Mesh collider
                if (scr.physics.colliderType == RFColliderType.Mesh)
                {
                    // Add Mesh collider
                    MeshCollider mCol = scr.gameObject.AddComponent<MeshCollider>();
                    
                    // Set mesh
                    if (mesh != null)
                        mCol.sharedMesh = mesh;

                    // Set convex for dynamic types // TODO convex for kinematik
                    if (scr.simulationType == SimType.Dynamic ||
                        scr.simulationType == SimType.Inactive ||
                        scr.simulationType == SimType.Sleeping)
                        mCol.convex = true;
                    scr.physics.meshCollider = mCol;
                }
                    
                // Box collider
                else if (scr.physics.colliderType == RFColliderType.Box)
                {
                    BoxCollider mCol = scr.gameObject.AddComponent<BoxCollider>();
                    scr.physics.meshCollider = mCol;
                }
                        
                // Sphere collider
                else if (scr.physics.colliderType == RFColliderType.Sphere)
                {
                    SphereCollider mCol = scr.gameObject.AddComponent<SphereCollider>();
                    scr.physics.meshCollider = mCol;
                }
            }
        }
        
        // Create mesh colliders for every input mesh TODO input cluster to control all nest roots for correct colliders
        public static void SetClusterColliders (RayfireRigid scr, MeshFilter[] childMeshes)
        {
            //float t1 = Time.realtimeSinceStartup;

            // Check children for mesh or cluster root until all children will not be checked
            Mesh tempMesh;
            scr.physics.clusterColliders = new List<Collider>();
            for (int i = 0; i < childMeshes.Length; i++)
            {
                // Skip
                if (childMeshes[i].sharedMesh == null)
                    continue;

                // Offset mesh for collider
                List<Vector3> vertices = new List<Vector3>();
                childMeshes[i].sharedMesh.GetVertices (vertices);
                for (int v = 0; v < vertices.Count; v++)
                    vertices[v] = scr.transform.InverseTransformPoint (childMeshes[i].transform.TransformPoint (vertices[v]));
                
                // Set new mesh data
                tempMesh = new Mesh();
                tempMesh.name = childMeshes[i].sharedMesh.name;
                tempMesh.SetVertices (vertices);
                tempMesh.triangles = childMeshes[i].sharedMesh.triangles;

                // Set up new collider based on child mesh
                MeshCollider meshCol = scr.gameObject.AddComponent<MeshCollider>();
                meshCol.convex = true;
                meshCol.sharedMesh = tempMesh;

                // Collect colliders
                scr.physics.clusterColliders.Add (meshCol);
            }

            //Debug.Log (Time.realtimeSinceStartup - t1);
        }
        
        // Set collider material
        public static void SetColliderMaterial(RayfireRigid scr)
        {
            // Set physics material if not defined by user
            if (scr.physics.material == null)
                scr.physics.material = scr.physics.PhysMaterial;
            
            // Set mesh collider material
            if (scr.physics.meshCollider != null)
            {
                scr.physics.meshCollider.sharedMaterial = scr.physics.material;
                return;
            }
            
            // Set cluster colliders material
            if (scr.physics.clusterColliders != null)
                if (scr.physics.clusterColliders.Count > 0)
                    foreach (var col in scr.physics.clusterColliders)
                        col.sharedMaterial = scr.physics.material;
        }
        
        // Set collider convex state
        public static void SetColliderConvex(RayfireRigid scr)
        {
            if (scr.physics.meshCollider != null)
            {
                // Not Mesh collider
                if (scr.physics.meshCollider is MeshCollider == false)
                    return;
                
                // Turn on convex for non kinematik
                MeshCollider mCol = (MeshCollider)scr.physics.meshCollider;
                if (scr.physics.rigidBody.isKinematic == false)
                    mCol.convex = true;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Cache physics data for fragments 
        public IEnumerator PhysicsDataCor (RayfireRigid scr)
        {
            while (exclude == false)
            {
                velocity = scr.physics.rigidBody.velocity;
                // TODO angularVelocity = rigidBody.angularVelocity; rigidBody.GetPointVelocity () set rotation to fragments
                position = scr.transForm.position;
                rotation = scr.transForm.rotation;
                yield return null;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Fall check
        /// /////////////////////////////////////////////////////////
        
        // Init infinite fall check
        public void InitFallCheck (RayfireRigid scr)
        {
            if (RayfireMan.inst.destroy == true)
                if (scr.simulationType == SimType.Dynamic || scr.simulationType == SimType.Sleeping || scr.simulationType == SimType.Inactive)
                    scr.StartCoroutine (FallCheckCor(scr));
        }
        
        // Exclude from simulation, move under ground, destroy
        IEnumerator FallCheckCor (RayfireRigid scr)
        {
            // Wait random time
            yield return new WaitForSeconds (Random.Range (0f, 5f));

            // Check fall every 10 seconds
            while (RayfireMan.inst.destroy == true)
            {
                // Wait 10 second and check
                yield return new WaitForSeconds (10f);

                // Object not sleeping
                if (scr.physics.rigidBody.IsSleeping() == false)
                {
                    // Check distance between birth and current
                    if (birthPos.y - scr.transForm.position.y > RayfireMan.inst.distance)
                    {
                        // Check for prefragmented objects
                        scr.DeleteFragments();

                        // Destroy actual fragment
                        RayfireMan.DestroyFragment (scr.gameObject, scr.rootParent);
                    }
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Get Destructible state
        public bool Destructible
        {
            get { return RayfireMan.inst.materialPresets.Destructible(materialType); }
        }

        // Get physic material
        public int Solidity
        {
            get { return RayfireMan.inst.materialPresets.Solidity(materialType); }
        }

        // Get physic material
        PhysicMaterial PhysMaterial
        {
            get
            {
                // Return predefine material
                if (material != null)
                    return material;

                // Crete new material
                return RFMaterialPresets.Material(materialType);
            }
        }
    }
}