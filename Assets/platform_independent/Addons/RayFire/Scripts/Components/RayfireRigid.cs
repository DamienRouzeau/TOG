using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

// Stop coroutines at destroy
// Unyielding range

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Rigid")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-rigid-component/")]
    public class RayfireRigid : MonoBehaviour
    {
        public enum InitType
        {
            ByMethod = 0,
            AtStart  = 1
        }
        
        [Space (2)]
        public InitType initialization = InitType.ByMethod;
        [HideInInspector] public bool initialized = false;
        
        [Header ("  Main")]
        [Space (2)]
        
        [Tooltip ("Defines behaviour of object during simulation.")]
        public SimType simulationType = SimType.Dynamic;
        [Space (1)]
        public ObjectType objectType = ObjectType.Mesh;
        [Space (1)]
        public DemolitionType demolitionType = DemolitionType.None;
        
        [Header ("  Simulation")]
        [Space (2)]
        
        public RFPhysic physics = new RFPhysic();
        public RFActivation activation = new RFActivation();
        
        [Header ("  Demolition")]
        [Space (2)]
        
        public RFLimitations limitations = new RFLimitations();
        public RFDemolitionMesh meshDemolition = new RFDemolitionMesh();
        public RFDemolitionCluster clusterDemolition = new RFDemolitionCluster();
        public RFReferenceDemolition referenceDemolition = new RFReferenceDemolition();
        public RFSurface    materials  = new RFSurface();
        public RFDamage     damage     = new RFDamage();
        public RFFade       fading     = new RFFade();

        [Header ("  Info")]

        // Hidden
        [HideInInspector] public List<RFDictionary> subIds;
        [HideInInspector] public Vector3[] pivots;
        [HideInInspector] public RFMesh[] rfMeshes;
        [HideInInspector] public List<RayfireRigid> fragments;
        [HideInInspector] public Vector3 contactPoint;
        [HideInInspector] public Quaternion cacheRotation; // NOTE. Should be public, otherwise rotation error on demolition.
        [HideInInspector] public Bounds bound;
        [HideInInspector] public Transform transForm;
        [HideInInspector] public Transform rootChild;
        [HideInInspector] public Transform rootParent;
        [HideInInspector] public Mesh[] meshes;
        [HideInInspector] public MeshFilter meshFilter;
        [HideInInspector] public MeshRenderer meshRenderer;
        [HideInInspector] public SkinnedMeshRenderer skinnedMeshRend;
        [HideInInspector] public RayfireDebris scrDebris;
        [HideInInspector] public RayfireDust scrDust;
        
        // Events
        public RFDemolitionEvent demolitionEvent = new RFDemolitionEvent();
        public RFActivationEvent activationEvent = new RFActivationEvent();

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        // Awake
        void Awake()
        {
            if (initialization == InitType.AtStart)
                AwakeMethods();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (initialization == InitType.AtStart)
                StartMethods();
        }
        
        // Awake ops
        void AwakeMethods()
        {
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();

            // Init mesh root.
            if (SetRootMesh() == true)
                return;
            
            // Set components for mesh / skinned mesh / clusters
            SetComponentsBasic();
            
            // Check for user mistakes
            RFLimitations.Checks(this);
            
            // Set components for mesh / skinned mesh / clusters
            SetComponentsPhysics();
            
            // Precache meshes at awake
            AwakePrecache();

            // Prefragment object at awake
            AwakePrefragment();
        }
        
        // Start ops
        void StartMethods()
        {
            // Excluded from simulation
            if (physics.exclude == true)
                return;

            // Set Start variables
            SetObjectType();

            // Start all coroutines
            StartCoroutines();

            // Object initialized
            initialized = true;
        }

        // Initialize
        public void Initialize()
        {
            if (initialized == false)
            {
                AwakeMethods();
                StartMethods();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Basic
        /// /////////////////////////////////////////////////////////

        // Init mesh root. Copy Rigid component for children with mesh
        bool SetRootMesh()
        {
            if (objectType == ObjectType.MeshRoot)
            {
                // Stop if already initiated
                if (limitations.demolished == true || physics.exclude == true)
                    return true;
                
                // Get children
                List<Transform> children = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                    children.Add (transform.GetChild (i));

                // Add Rigid to child with mesh
                fragments = new List<RayfireRigid>();
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].GetComponent<MeshFilter>() != null)
                    {
                        // Turn off to copy script data and then init
                        children[i].gameObject.SetActive (false);

                        // Get rigid
                        RayfireRigid childRigid = children[i].gameObject.GetComponent<RayfireRigid>();
                        if (childRigid == null)
                            childRigid = children[i].gameObject.AddComponent<RayfireRigid>();
                        childRigid.initialization = InitType.AtStart;
                        
                        fragments.Add (childRigid);
                        CopyPropertiesTo (childRigid);

                        // Turn on
                        children[i].gameObject.SetActive (true);
                    }
                }

                // TODO Setup as clusters root children with transform only

                // Turn off demolition and physics
                demolitionType = DemolitionType.None;
                physics.exclude = true;
                return true;
            }

            return false;
        }
        
        // Define basic components
        void SetComponentsBasic()
        {
            // Set shatter component
            meshDemolition.scrShatter = meshDemolition.useShatter == true 
                ? GetComponent<RayfireShatter>() 
                : null;
            
            // Other
            transForm       = GetComponent<Transform>();
            meshFilter      = GetComponent<MeshFilter>();
            meshRenderer    = GetComponent<MeshRenderer>();
            skinnedMeshRend    = GetComponent<SkinnedMeshRenderer>();
            scrDebris       = GetComponent<RayfireDebris>();
            scrDust         = GetComponent<RayfireDust>();
        }
        
        // Define components
        void SetComponentsPhysics()
        {
            // Excluded from simulation
            if (physics.exclude == true)
                return;
            
            // Physics components
            physics.rigidBody = GetComponent<Rigidbody>();
            physics.meshCollider = GetComponent<Collider>();
            
            // Add missing mesh renderer
            if (meshFilter != null && meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            // Mesh check
            if (objectType == ObjectType.Mesh)
            {
                // Set collider
                RFPhysic.SetMeshCollider (this);
                
                // Rigid body
                if (physics.rigidBody == null)
                    physics.rigidBody = gameObject.AddComponent<Rigidbody>();
            }

            // Cluster check TODO EXPOSE IN UI !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (objectType == ObjectType.NestedCluster || objectType == ObjectType.ConnectedCluster)
            {
                // No children mesh for clustering
                bool clusteringState = RFDemolitionCluster.Clusterize (this);
                if (clusteringState == false)
                {
                    physics.exclude = true;
                    Debug.Log ("RayFire Rigid: " + name + " has no children with mesh. Object Excluded from simulation.",
                        gameObject);
                    return;
                }
                
                // Rigid body
                if (physics.rigidBody == null)
                    physics.rigidBody = gameObject.AddComponent<Rigidbody>();
            }
            
            // Collision detection TODO set in UI
            physics.rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Set variables
        /// /////////////////////////////////////////////////////////
        
        // Set Start variables
        void SetObjectType ()
        {
            if (objectType == ObjectType.Mesh ||
                objectType == ObjectType.NestedCluster ||
                objectType == ObjectType.ConnectedCluster)
            {
                // Reset rigid data
                ResetRigid();

                // Set physics properties
                SetPhysics();
            }
        }

        // Reset rigid data
        void ResetRigid()
        {
            // Reset damage and other props like it is new
            damage.Reset();

            // Reset contact info
            clusterDemolition.damageRadius = 0f;
            contactPoint = Vector3.zero;

            // Set birth time
            limitations.birthTime = Time.time + Random.Range (0f, 0.3f);

            // Birth position for activation check
            physics.birthPos = transForm.position;
            
            // Set bound and size
            SetBound();
        }

        // Set physics properties
        void SetPhysics()
        {
            // Excluded from sim
            if (physics.exclude == true)
                return;
            
            // MeshCollider physic material preset. Set new or take from parent 
            RFPhysic.SetColliderMaterial (this);

            // Set physical simulation type. Important. Should after collider material define
            RFPhysic.SetSimulationType (this);

            // Convex collider meshCollider. After SetSimulation Type to turn off convex for kinematic
            RFPhysic.SetColliderConvex (this);

            // Set density. After collider defined
            physics.SetDensity (physics.rigidBody);

            // Set drag properties
            physics.SetDrag (physics.rigidBody);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Start all coroutines
        void StartCoroutines()
        {
            // Check for demolition state every frame
            if (demolitionType != DemolitionType.None)
                StartCoroutine (DemolishableCor());
            
            // Stop if already has running coroutines
            // StopAllCoroutines();
            
            // Prevent physics cors
            if (physics.exclude == true)
                return;
            
            // Cache physics data for fragments 
            StartCoroutine (physics.PhysicsDataCor(this));
            
            // Activation by velocity\offset coroutines
            if (simulationType == SimType.Inactive || simulationType == SimType.Kinematic)
            {
                if (activation.byVelocity > 0)
                    StartCoroutine (activation.ActivationVelocityCor(this));
                if (activation.byOffset > 0)
                    StartCoroutine (activation.ActivationOffsetCor(this));
            }
            
            // Init inactive every frame update coroutine
            if (simulationType == SimType.Inactive)
                StartCoroutine (activation.InactiveCor(this));
            
            // Init infinite fall check if dynamic and sleeping
            physics.InitFallCheck(this);
        }
        
        // Cache velocity for fragments 
        IEnumerator DemolishableCor()
        {
            while (demolitionType != DemolitionType.None)
            {
                // Max depth reached
                if (limitations.depth > 0 && limitations.currentDepth >= limitations.depth)
                    demolitionType = DemolitionType.None;

                // Init demolition
                if (limitations.demolitionShould == true)
                    DemolishObject();
                
                // Check for slicing planes and init slicing
                else if (limitations.sliceByBlade == true && limitations.slicePlanes.Count > 1)
                    SliceObjectByPlanes();
                
                yield return null;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition types
        /// /////////////////////////////////////////////////////////
        
        // Precache meshes at awake
        void AwakePrecache()
        {
            if (demolitionType == DemolitionType.AwakePrecache && objectType == ObjectType.Mesh)
                CacheInstant();
        }
        
        // Predefine fragments
        void AwakePrefragment()
        {
            if (demolitionType == DemolitionType.AwakePrefragment && objectType == ObjectType.Mesh)
            {
                // Cache meshes
                CacheInstant();

                // Predefine fragments
                Prefragment();
            }
        }
        
        // Precache meshes for prefab in editor
        public void PrefabPrecache()
        {
            if (demolitionType == DemolitionType.ManualPrefabPrecache && objectType == ObjectType.Mesh)
            {
                // Set components for mesh / skinned mesh / clusters
                SetComponentsBasic();
                
                // Cache meshes
                CacheInstant();

                // Convert meshes to RFmeshes
                if (HasMeshes == true)
                {
                    rfMeshes = new RFMesh[meshes.Length];
                    for (int i = 0; i < meshes.Length; i++)
                        rfMeshes[i] = new RFMesh (meshes[i], meshDemolition.compressPrefab);
                }
                meshes = null;
            }
        }

        // Precache meshes in editor
        public void ManualPrecache()
        {
            if (demolitionType == DemolitionType.ManualPrecache && objectType == ObjectType.Mesh)
            {
                // Set components
                SetComponentsBasic();
                
                // Set components
                SetComponentsPhysics();
                
                // Cache meshes
                CacheInstant();
            }
            else if (demolitionType == DemolitionType.ManualPrecache && objectType != ObjectType.Mesh)
                Debug.Log ("RayFire Rigid: " + name + " Object Type is not Mesh. Set to Mesh type to Precache.", gameObject);
        }
        
        // Precache meshes in editor
        public void ManualPrefragment()
        {
            if (demolitionType == DemolitionType.ManualPrefragment && objectType == ObjectType.Mesh)
            {
                // Set components
                SetComponentsBasic();
                
                // Set components
                SetComponentsPhysics();
                
                // Cache meshes
                CacheInstant();

                // Predefine fragments
                Prefragment();
            }
            else if (demolitionType == DemolitionType.ManualPrefragment && objectType != ObjectType.Mesh)
                Debug.Log ("RayFire Rigid: " + name + " Object Type is not Mesh. Set to Mesh type to Prefragment.", gameObject);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Caching
        /// /////////////////////////////////////////////////////////
        
        // Start cache fragment meshes. Instant or runtime
        void CacheRuntime()
        {
            if (meshDemolition.runtimeCaching.type == CachingType.Disable)
                CacheInstant();
            else
                CacheFrames();
        }
        
        // Instant caching into meshes
        void CacheInstant()
        {
            DeleteCache();

            // Input mesh, setup
            if (RFFragment.PrepareCacheMeshes (this) == false)
                return;
            
            RFFragment.CacheMeshesInst (ref meshes, ref pivots, ref subIds, this);
        }
        
        // Caching into meshes over several frames
        void CacheFrames()
        {
            DeleteCache();
            StartCoroutine (meshDemolition.RuntimeCachingCor(this));
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        // Collision check
        void OnCollisionEnter (Collision collision)
        {
            // TODO check if it is better to check state or collisions str
            
            // Demolish object check
            if (DemolitionState() == false)
                return;
            
            // Check if collision demolition passed
            if (CollisionDemolition (collision) == true)
                limitations.demolitionShould = true;
        }
        
        // Check if collision demolition passed
        bool CollisionDemolition (Collision collision)
        {
            // Collision with kinematic object
            if (collision.rigidbody != null && collision.rigidbody.isKinematic == true)
                if (collision.impulse.magnitude > physics.Solidity * limitations.solidity * RayfireMan.inst.globalSolidity * 7f)
                {
                    // TODO USE GET CONTACTS INSTEAD
                    contactPoint = collision.contacts[0].point;
                    //contactCollider = collision.contacts[0].thisCollider;
                    return true;
                }

            // Collision force checks. TODO get biggest collision
            for (int i = 0; i < collision.contacts.Length; i++)
            {
                // Set contact point
                contactPoint = collision.contacts[i].point;

                // { TODO COLLISION VELOCITY MULTIPLIER
                //     Debug.Log (collision.contacts[i].otherCollider.attachedRigidbody.velocity.magnitude);
                //     RayfireRigid colrig = collision.contacts[i].otherCollider.gameObject.GetComponent<RayfireRigid>();
                //     Debug.Log (colrig.physics.velocity.magnitude);
                //     colrig.rigidBody.velocity = colrig.physics.velocity*3;
                // }
                
                // Demolish if collision high enough
                if (collision.relativeVelocity.magnitude > physics.Solidity * limitations.solidity * RayfireMan.inst.globalSolidity)
                    return true;
                
                // Collect damage by collision
                if (damage.enable == true && damage.collectCollisions == true)
                {
                    if (ApplyDamage (collision.relativeVelocity.magnitude, contactPoint, 0f) == true)
                        return true;
                }
            }

            return false;
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

         // Demolition available state
        public bool State ()
        {
            // Object already demolished
            if (limitations.demolished == true)
                return false;

            // Object already passed demolition state and demolishing is in progress
            if (meshDemolition.runtimeCaching.inProgress == true)
                return false;
            
            // Bad mesh check
            if (meshDemolition.badMesh > RayfireMan.inst.advancedDemolitionProperties.badMeshTry)
                return false;

            // Max amount check
            if (RayfireMan.MaxAmountCheck == false)
                return false;

            // Depth level check
            if (limitations.depth > 0 && limitations.currentDepth >= limitations.depth)
                return false;

            // Min Size check. Min Size should be considered and size is less than
            if (limitations.bboxSize < limitations.size)
                return false;

            // Safe frame
            if (Time.time - limitations.birthTime < limitations.time)
                return false;

            return true;
        }
        
        // Check if object should be demolished
        bool DemolitionState ()
        {
            // No demolition allowed
            if (demolitionType == DemolitionType.None)
                return false;
            
            // Non destructible material
            if (physics.Destructible == false)
                return false;

            // Demolition available check
            if (State() == false)
                return false;

            // Per frame time check
            if (RayfireMan.inst.timeQuota > 0 && RayfireMan.inst.maxTimeThisFrame > RayfireMan.inst.timeQuota)
                return false;

            return true;
        }
        
        // Demolish object
        public void DemolishObject()
        {
            // Initialize if not
            if (initialized == false)
                Initialize();

            // Timestamp
            float t1 = Time.realtimeSinceStartup;
            
            // Restore position and rotation to prevent high collision offset
            transForm.position = physics.position;
            transForm.rotation = physics.rotation;

            // Demolish mesh or cluster to reference
            if (DemolishReference() == false)
                return;

            // Demolish mesh and create fragments. Stop if runtime caching or no meshes/fragments were created
            if (DemolishMesh() == false)
                return;
            
            // Demolish cluster to children nodes 
            DemolishCluster();

            // Check fragments and proceed
            if (limitations.demolished == false)
            {
                demolitionType = DemolitionType.None;
                Debug.Log ("DemolishObject error: No Fragments", gameObject);
                return;
            }

            // Connectivity check
            activation.CheckConnectivity();
            
            // Fragments initialisation
            InitFragments();
            
            // Sum total demolition time
            RayfireMan.inst.maxTimeThisFrame += Time.realtimeSinceStartup - t1;
            
            // Init particles
            RFParticles.InitDemolitionParticles(this);

            // Event
            demolitionEvent.InvokeLocalEvent (this);
            RFDemolitionEvent.InvokeGlobalEvent (this);
            
            // Destroy demolished object
            if (limitations.demolished == true)
                RayfireMan.DestroyFragment (gameObject, rootParent);
        }

        // Demolish object to reference
        bool DemolishReference()
        {
            if (demolitionType == DemolitionType.ReferenceDemolition)
            {
                // Get instance
                GameObject referenceGo = referenceDemolition.GetReference();
                
                // Has reference
                if (referenceGo != null)
                {
                    // Instantiate turned off reference 
                    referenceGo.SetActive (false);
                    GameObject fragRoot = Instantiate (referenceGo);
                    referenceGo.SetActive (true);
                    fragRoot.name = referenceGo.name;

                    // Set tm
                    rootChild                  = fragRoot.transform;
                    rootChild.position         = transForm.position;
                    rootChild.rotation         = transForm.rotation;
                    rootChild.transform.parent = RayfireMan.inst.transForm;

                    // Clear list for fragments
                    fragments.Clear();

                    // Check root for rigid props
                    RayfireRigid rootScr = fragRoot.gameObject.GetComponent<RayfireRigid>();

                    // Reference Root has not rigid. Add to
                    if (rootScr == null && referenceDemolition.addRigid == true)
                    {
                        // Add rigid and copy
                        rootScr = fragRoot.gameObject.AddComponent<RayfireRigid>();
                        rootScr.initialization = InitType.AtStart;
                        
                        CopyPropertiesTo (rootScr);

                        // Single mesh TODO improve
                        if (fragRoot.transform.childCount == 0)
                        {
                            rootScr.objectType = ObjectType.Mesh;
                        }

                        // Multiple meshes
                        if (fragRoot.transform.childCount > 0)
                        {
                            rootScr.objectType = ObjectType.MeshRoot;
                        }
                    }

                    // Activate and init rigid
                    rootChild.gameObject.SetActive (true);

                    // Reference has rigid
                    if (rootScr != null)
                    {
                        // Create rigid for root children
                        if (rootScr.objectType == ObjectType.MeshRoot)
                        {
                            foreach (var frag in rootScr.fragments)
                                frag.limitations.currentDepth++;
                            fragments.AddRange (rootScr.fragments);
                            Destroy (rootScr);
                        }

                        // Get ref rigid
                        else if (rootScr.objectType == ObjectType.Mesh ||
                                 rootScr.objectType == ObjectType.SkinnedMesh)
                        {
                            rootScr.meshDemolition.runtimeCaching.type = CachingType.Disable;
                            rootScr.DemolishMesh();
                            fragments.AddRange (rootScr.fragments);
                            RayfireMan.DestroyFragment (rootScr.gameObject, rootScr.rootParent, 1f);
                        }

                        // Get ref rigid
                        else if (rootScr.objectType == ObjectType.NestedCluster ||
                                 rootScr.objectType == ObjectType.ConnectedCluster)
                        {
                            rootScr.ResetRigid();
                            rootScr.contactPoint = contactPoint;
                            rootScr.DemolishCluster();
                            rootScr.physics.exclude = true;
                            fragments.AddRange (rootScr.fragments);
                            RayfireMan.DestroyFragment (rootScr.gameObject, rootScr.rootParent, 1f);
                        }

                        // Has rigid by has No fragments. Stop demolition
                        if (HasFragments == false)
                        {
                            demolitionType = DemolitionType.None;
                            return false;
                        }
                    }
                }

                // Has no rigid, has No fragments, but demolished
                limitations.demolished = true;
            }

            return true;
        }

        // Demolish single mesh to fragments
        bool DemolishMesh()
        {
            // Object demolition
            if (objectType == ObjectType.Mesh || objectType == ObjectType.SkinnedMesh)
            {
                // Skip if reference
                if (demolitionType == DemolitionType.ReferenceDemolition)
                    return true;
                
                // Object was prefragmented. Proceed with demolition
                if (demolitionType == DemolitionType.AwakePrefragment || 
                    demolitionType == DemolitionType.ManualPrefragment)
                {
                    // Check fragments
                    if (HasFragments == true)
                    {
                        // Check for null
                        if (rootChild == null || transForm == null)
                        {
                            Debug.Log ("Transform null error", gameObject);
                            return false;
                        }
                        
                        // Set tm 
                        rootChild.position = transForm.position;
                        rootChild.rotation = transForm.rotation;
                        rootChild.transform.parent = RayfireMan.inst.transForm;

                        // Copy properties if were changed
                        for (int i = 0; i < fragments.Count; i++)
                        {
                            CopyPropertiesTo (fragments[i]);
                            fragments[i].initialization = InitType.AtStart;
                        }

                        // Activate root and fragments
                        rootChild.gameObject.SetActive (true);
                        
                        limitations.demolished = true;
                        return true;
                    }
                }

                // Prefab was precached. Convert serialized meshes to unity meshes
                if (demolitionType == DemolitionType.ManualPrefabPrecache)
                {
                    if (HasRfMeshes == true)
                        RFMesh.ConvertRfMeshes (this);
                }

                // Cache meshes in runtime
                if (demolitionType == DemolitionType.Runtime)
                {
                    if (HasMeshes == false)
                    {
                        // Cache meshes
                        CacheRuntime();

                        // Caching in progress. Stop demolition
                        if (meshDemolition.runtimeCaching.inProgress == true)
                            return false;
                    }
                }

                // Object has cache, Create fragments
                if (demolitionType == DemolitionType.AwakePrecache || 
                    demolitionType == DemolitionType.ManualPrecache ||
                    demolitionType == DemolitionType.ManualPrefabPrecache ||
                    demolitionType == DemolitionType.Runtime)
                {
                    // Check meshes
                    if (HasMeshes == true)
                    {
                        // Create fragments
                        fragments = CreateFragments();

                        // Check fragments
                        if (HasFragments == true)
                        {
                            limitations.demolished = true;
                            return true;
                        }
                    }
                }

                return false;
            }

            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Clusters
        /// /////////////////////////////////////////////////////////
        
        // Demolish cluster to children nodes
        void DemolishCluster()
        {
            // Skip if not runtime
            if (demolitionType != DemolitionType.Runtime)
                return;
            
            // Cluster demolition
            if (objectType == ObjectType.NestedCluster)
                clusterDemolition.DemolishClusterNested (this);
            else if (objectType == ObjectType.ConnectedCluster)
                clusterDemolition.DemolishClusterConnected (this);
        }

        // Generate colliders
        public void GenerateColliders()
        {
            transForm = GetComponent<Transform>();
            physics.colliderType = RFColliderType.Mesh;
            RFDemolitionCluster.Clusterize (this);
            physics.colliderType = RFColliderType.None;
        }
        
        // Delete all colliders
        public void DeleteColliders()
        {
            Collider[] cols = gameObject.GetComponents<Collider>();
            for (int i = 0; i < cols.Length; i++)
                DestroyImmediate (cols[i]);
            physics.colliderType = RFColliderType.Mesh;
            clusterDemolition.cluster = null;
        }
                
        /// /////////////////////////////////////////////////////////
        /// Fragments
        /// /////////////////////////////////////////////////////////
        
        // Create fragments by mesh and pivots array
        List<RayfireRigid> CreateFragments()
        {
            // Fragments list
            List<RayfireRigid> scrArray = new List<RayfireRigid>();

            // Stop if has no any meshes
            if (meshes == null)
                return scrArray;
            
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Create root object and parent
            RFLimitations.CreateRoot (this);
            
            // Vars 
            int    baseLayer = meshDemolition.GetLayer(this);
            string baseTag   = gameObject.tag;
            string baseName  = gameObject.name + "_fr_";

            // Save original rotation
            // Quaternion originalRotation = rootChild.transform.rotation;
            
            // Set rotation to precache rotation
            if (demolitionType == DemolitionType.AwakePrecache ||
                demolitionType == DemolitionType.ManualPrecache ||
                demolitionType == DemolitionType.ManualPrefabPrecache)
                rootChild.transform.rotation = cacheRotation;

            // Get original mats
            Material[] mats = skinnedMeshRend != null
                ? skinnedMeshRend.sharedMaterials
                : meshRenderer.sharedMaterials;

            // Create fragment objects
            for (int i = 0; i < meshes.Length; ++i)
            {
                // Get object from pool or create
                RayfireRigid rfScr = RayfireMan.inst == null
                    ? RayfireMan.CreateRigidInstance()
                    : RayfireMan.inst.GetPoolObject();

                // Setup
                rfScr.transform.position    = transForm.position + pivots[i];
                rfScr.transform.parent      = rootChild;
                rfScr.name                  = baseName + i;
                rfScr.gameObject.tag        = baseTag;
                rfScr.gameObject.layer      = baseLayer;
                rfScr.meshFilter.sharedMesh = meshes[i];
                rfScr.rootParent            = rootChild;

                // Copy properties from parent to fragment node
                CopyPropertiesTo (rfScr);

                // Set collider
                RFPhysic.SetFragmentMeshCollider (rfScr, meshes[i]);
                
                // Shadow casting
                if (RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > 0 && 
                    RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > meshes[i].bounds.size.magnitude)
                    rfScr.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                
                // Turn on
                rfScr.gameObject.SetActive (true);

                // Set multymaterial
                RFSurface.SetMaterial (subIds, mats, materials, rfScr.meshRenderer, i, meshes.Length);

                // Update depth level and amount
                rfScr.limitations.currentDepth = limitations.currentDepth + 1;
                rfScr.meshDemolition.amount = (int)(rfScr.meshDemolition.amount * rfScr.meshDemolition.depthFade);
                if (rfScr.meshDemolition.amount < 2)
                    rfScr.meshDemolition.amount = 2;

                // Add in array
                scrArray.Add (rfScr);
            }

            // Fix transform for precached fragments
            if (demolitionType == DemolitionType.AwakePrecache ||
                demolitionType == DemolitionType.ManualPrecache ||
                demolitionType == DemolitionType.ManualPrefabPrecache)
                rootChild.rotation = transForm.rotation;

            // Fix runtime caching rotation difference. Get rotation difference and add to root
            if (demolitionType == DemolitionType.Runtime && meshDemolition.runtimeCaching.type != CachingType.Disable)
            {
                Quaternion cacheRotationDif = transForm.rotation * Quaternion.Inverse (meshDemolition.cacheRotationStart);
                rootChild.rotation = cacheRotationDif * rootChild.rotation;
            }

            // Empty lists
            DeleteCache();

            return scrArray;
        }

        // Copy rigid properties from parent to fragments
        public void CopyPropertiesTo (RayfireRigid toScr)
        {
            // Object type
            toScr.objectType = objectType;

            // Set mesh type if copied from mesh root
            if (objectType == ObjectType.MeshRoot || objectType == ObjectType.SkinnedMesh)
                toScr.objectType = ObjectType.Mesh;

            // Sim type
            toScr.simulationType = simulationType;
            
            // Demolition type
            toScr.demolitionType = demolitionType;
            if (objectType != ObjectType.MeshRoot)
                if (demolitionType != DemolitionType.None)
                    toScr.demolitionType = DemolitionType.Runtime;
            
            // Copy classes
            toScr.physics.CopyFrom (physics);
            toScr.activation.CopyFrom (activation);
            toScr.limitations.CopyFrom (limitations);
            toScr.meshDemolition.CopyFrom (meshDemolition);
            toScr.clusterDemolition.CopyFrom (clusterDemolition);
            
            // Copy reference demolition props
            if (objectType == ObjectType.MeshRoot)
                toScr.referenceDemolition.CopyFrom (referenceDemolition);
            
            toScr.materials.CopyFrom (materials);
            toScr.damage.CopyFrom (damage);
            toScr.fading.CopyFrom (fading);
            
            // Copy debris
            if (scrDebris != null)
            {
                if (toScr.scrDebris == null)
                    toScr.scrDebris = toScr.gameObject.AddComponent<RayfireDebris>();
                toScr.scrDebris.debris.CopyFrom (scrDebris.debris);
            }
            
            // Copy dust
            if (scrDust != null)
            {
                if (toScr.scrDust == null)
                    toScr.scrDust = toScr.gameObject.AddComponent<RayfireDust>();
                toScr.scrDust.dust.CopyFrom (scrDust.dust);
            }
                                
        }
        
        // Fragments initialisation
        void InitFragments()
        {
            // No fragments
            if (HasFragments == false)
                return;
            
            // Set bound and size
            for (int i = 0; i < fragments.Count; i++)
                fragments[i].SetBound();
            
            // Current velocity
            if (meshDemolition.runtimeCaching.wasUsed == true && meshDemolition.runtimeCaching.skipFirstDemolition == false)
            {
                for (int i = 0; i < fragments.Count; i++)
                    fragments[i].physics.rigidBody.velocity = physics.rigidBody.GetPointVelocity (fragments[i].transForm.position) * physics.collMult;
            }

            // Previous frame velocity
            else
            {
                Vector3 baseVelocity = physics.velocity * physics.collMult;
                for (int i = 0; i < fragments.Count; i++)
                    fragments[i].physics.rigidBody.velocity = baseVelocity;
            }
           
            // TODO set current frame for cluster demol types
            
            // Sum total new fragments amount
            RayfireMan.inst.advancedDemolitionProperties.currentAmount += fragments.Count;

            // Fading. move to fragment
            fading.FadingParent (this, fragments);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Manual methods
        /// /////////////////////////////////////////////////////////
        
        // Predefine fragments
        void Prefragment()
        {
            // Delete existing
            DeleteFragments();

            // Create fragments from cache
            fragments = CreateFragments();
                
            // Stop
            if (fragments == null)
            {
                demolitionType = DemolitionType.None;
                return;
            }
                
            // No fragments after fragmentation. Disable demolition
            if (fragments.Count == 0)
            {
                demolitionType = DemolitionType.None;
                return;
            }

            // Set physics properties
            foreach (var scr in fragments)
            {
                scr.SetComponentsBasic();
                scr.SetComponentsPhysics();
                scr.SetObjectType();
            }
            
            // Deactivate fragments root
            if (rootChild != null)
                rootChild.gameObject.SetActive (false);
        }

        // Delete fragments
        public void DeleteFragments()
        {
            // Destroy root
            if (rootChild != null)
            {
                if (Application.isPlaying == true)
                    Destroy (rootChild.gameObject);
                else
                    DestroyImmediate (rootChild.gameObject);

                // Clear ref
                rootChild = null;
            }

            // Clear array
            fragments = null;
        }

        // Clear cache info
        public void DeleteCache()
        {
            meshes           = null;
            pivots           = null;
            rfMeshes         = null;
            subIds = new List<RFDictionary>();
        }

        /// /////////////////////////////////////////////////////////
        /// Blade
        /// /////////////////////////////////////////////////////////

        // Add new slice plane
        public void AddSlicePlane (Vector3[] slicePlane)
        {
            // Not even amount of slice data
            if (slicePlane.Length % 2 == 1)
                return;

            // Add slice plane data
            limitations.slicePlanes.AddRange (slicePlane);
        }
        
        // Slice object
        void SliceObjectByPlanes()
        {
            // Empty lists
            DeleteCache();
            DeleteFragments();
    
            // SLice
            RFFragment.SliceMeshes (ref meshes, ref pivots, ref subIds, this, limitations.slicePlanes);

            // Remove plane info 
            limitations.slicePlanes.Clear();

            // Stop
            if (HasMeshes == false)
                return;

            // Get fragments
            fragments = CreateSlices();

            // Fragments initialisation
            InitFragments();

            // Event
            demolitionEvent.InvokeLocalEvent (this);
            RFDemolitionEvent.InvokeGlobalEvent (this);

            // Destroy original
            RayfireMan.DestroyFragment (gameObject, rootParent);
        }

        // Create slices by mesh and pivots array
        List<RayfireRigid> CreateSlices()
        {
            // Create root object
            RFLimitations.CreateRoot (this);

            // Clear array for new fragments
            List<RayfireRigid> scrArray = new List<RayfireRigid>();

            // Vars 
            int    baseLayer = meshDemolition.GetLayer(this);
            string baseTag   = gameObject.tag;
            string baseName  = gameObject.name + "_sl_";

            // Create fragment objects
            for (int i = 0; i < meshes.Length; ++i)
            {
                // Get object from pool or create
                RayfireRigid rfScr = RayfireMan.inst.GetPoolObject();

                // Setup
                rfScr.transform.position         = transForm.position + pivots[i];
                rfScr.transform.parent           = rootChild;
                rfScr.name                       = baseName + i;
                rfScr.gameObject.tag             = baseTag;
                rfScr.gameObject.layer           = baseLayer;
                rfScr.meshFilter.sharedMesh      = meshes[i];
                rfScr.meshFilter.sharedMesh.name = baseName + i;
                rfScr.rootParent                 = rootChild;

                // Copy properties from parent to fragment node
                CopyPropertiesTo (rfScr);

                // Shadow casting
                if (RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > 0 && 
                    RayfireMan.inst.advancedDemolitionProperties.sizeThreshold > meshes[i].bounds.size.magnitude)
                    rfScr.meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

                // Turn on
                rfScr.gameObject.SetActive (true);

                // Set multymaterial
                RFSurface.SetMaterial (subIds, meshRenderer.sharedMaterials, materials, rfScr.meshRenderer, i, meshes.Length);

                // Inherit same current depth level
                rfScr.limitations.currentDepth = limitations.currentDepth + 1;

                // Set collider mesh
                MeshCollider mc = rfScr.physics.meshCollider as MeshCollider;
                if (mc != null)
                {
                    mc.sharedMesh = meshes[i];
                    mc.name       = meshes[i].name;
                }

                // Add in array
                scrArray.Add (rfScr);
            }

            // Empty lists
            DeleteCache();

            return scrArray;
        }

        /// /////////////////////////////////////////////////////////
        /// Damage
        /// /////////////////////////////////////////////////////////

        // Apply damage
        public bool ApplyDamage (float damageValue, Vector3 damagePoint, float damageRadius = 0f)
        {
            // Initialize if not
            if (initialized == false)
                Initialize();
            
            // Already demolished or should be
            if (limitations.demolished == true || limitations.demolitionShould == true)
                return false;
            
            // Apply damage and get demolition state
            bool demolitionState = damage.ApplyDamage (damageValue);
            
            
            
            // Set demolition info
            if (demolitionState == true)
            {
                // Demolition available check
                if (DemolitionState() == false)
                    return false;

                // Set damage position
                contactPoint  = damagePoint;
                clusterDemolition.damageRadius = damageRadius;

                // Demolish object
                limitations.demolitionShould = true;

                // Demolish
                DemolishObject();

                // Was demolished
                if (limitations.demolished == true)
                    return true;
            }
            
            // Check for activation
            if (activation.byDamage > 0 && damage.currentDamage > activation.byDamage)
                Activate();
            
            return false;
        }

        /// /////////////////////////////////////////////////////////
        /// Inactive
        /// /////////////////////////////////////////////////////////
        
        // Activate inactive object
        public void Activate()
        {
            // Initialize if not
            if (initialized == false)
                Initialize();
            
            // Turn convex if kinematik activation
            if (simulationType == SimType.Kinematic)
            {
                MeshCollider meshCollider = physics.meshCollider as MeshCollider;
                if (meshCollider != null)
                    meshCollider.convex = true;

                // Swap with animated object
                if (physics.recorder == true)
                {
                    // Set dynamic before copy
                    simulationType        = SimType.Dynamic;
                    physics.rigidBody.isKinematic = false;
                    physics.rigidBody.useGravity  = true;
                    
                    // Create copy
                    GameObject inst = Instantiate (gameObject);
                    inst.transform.position = transForm.position;
                    inst.transform.rotation = transForm.rotation;

                    // Save velocity
                    Rigidbody rBody = inst.GetComponent<Rigidbody>();
                    if (rBody != null)
                    { 
                        rBody.velocity        = physics.rigidBody.velocity;
                        rBody.angularVelocity = physics.rigidBody.angularVelocity; 
                    }
                    
                    // Activate and init rigid
                    gameObject.SetActive (false);
                }
            }

            // Connectivity check
            activation.CheckConnectivity();
         
            // Set props
            simulationType        = SimType.Dynamic;
            physics.rigidBody.isKinematic = false;
            physics.rigidBody.useGravity  = true;

            activation.TurnOff();

            // Init infinite fall check
            physics.InitFallCheck(this);

            // Init particles on activation
            RFParticles.InitActivationParticles(this);

            // Event
            activationEvent.InvokeLocalEvent (this);
            RFActivationEvent.InvokeGlobalEvent (this);

            // TODO add initial velocity and rotation if still
            //rigidBody.velocity = 
        }

        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////
        
        // Set bound and size
        void SetBound()
        {
            if (objectType == ObjectType.Mesh)
                bound = meshRenderer.bounds;
            else if (objectType == ObjectType.Mesh)
                bound = skinnedMeshRend.bounds;
            else if (objectType == ObjectType.NestedCluster || objectType == ObjectType.ConnectedCluster)
                bound = RFCluster.GetChildrenBound (transForm);
            limitations.bboxSize = bound.size.magnitude;
        }
        
        // Destroy
        public void DestroyCollider(Collider col) { Destroy (col); }
        public void DestroyObject(GameObject go) { Destroy (go); }
        public void DestroyRigid(RayfireRigid rigid) { Destroy (rigid); }
        public void DestroyRb(Rigidbody rb) { Destroy (rb); }
        
        // Instantiate
        public Mesh InstantiateMesh(Mesh mesh) { return Instantiate (mesh); }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Fragments/Meshes/RFMeshes check
        public bool HasFragments { get { return fragments != null && fragments.Count > 0; } }
        public bool HasMeshes { get { return meshes != null && meshes.Length > 0; } }
        public bool HasRfMeshes { get { return rfMeshes != null && rfMeshes.Length > 0; } }
    }
}