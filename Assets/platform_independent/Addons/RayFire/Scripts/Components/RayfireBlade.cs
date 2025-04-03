using UnityEngine;

namespace RayFire
{
    [AddComponentMenu ("RayFire/Rayfire Blade")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-blade-component/")]
    public class RayfireBlade : MonoBehaviour
    {

        // Cut Type
        public enum CutType
        {
            Enter     = 0,
            Exit      = 1,
            EnterExit = 2
        }

        // Variables
        public CutType   cutType   = CutType.Exit;
        public PlaneType sliceType = PlaneType.XY;

        // Hidden
        [HideInInspector] public Vector3[] enterPlane = null;
        [HideInInspector] public Vector3[] exitPlane = null;
        [HideInInspector] public Collider colLider = null;
        [HideInInspector] public int mask = -1;
        [HideInInspector] public string tagFilter = "Untagged";

        // Event
        public RFSliceEvent sliceEvent = new RFSliceEvent();

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Set components
            DefineComponents();
        }

        // Check for trigger
        void OnTriggerEnter (Collider col)
        {
            // Cut target
            if (cutType == CutType.Enter)
            {
                // Slice
                Slice (col.gameObject, GetSlicePlane());
            }

            // Remember enter plane
            else if (cutType == CutType.EnterExit)
            {
                // Set enter plane
                enterPlane = GetSlicePlane();
            }
        }

        // Exit trigger
        void OnTriggerExit (Collider col)
        {
            // Cut target
            if (cutType == CutType.Exit)
            {
                // Slice
                Slice (col.gameObject, GetSlicePlane());
            }

            // Remember exit plane and calculate average plane
            else if (cutType == CutType.EnterExit)
            {
                // Get exit plane
                exitPlane = GetSlicePlane();

                // Get slice plane by enter plane and exit plane
                Vector3[] slicePlane = new Vector3[2];
                slicePlane[0] = (enterPlane[0] + exitPlane[0]) / 2f;
                slicePlane[1] = (enterPlane[1] + exitPlane[1]) / 2f;

                // Slice
                Slice (col.gameObject, slicePlane);
            }
        }

        // Slice collider by blade
        void Slice (GameObject targetObject, Vector3[] slicePlane)
        {
            // Check tag
            if (tagFilter != "Untagged" && targetObject.tag != tagFilter)
                return;

            // Check layer
            if (LayerCheck (targetObject.layer) == false)
                return;

            // Get RayFire script
            RayfireRigid rfScr = targetObject.GetComponent<RayfireRigid>();

            // No Rayfire Rigid script
            if (rfScr == null)
                return;

            // No demolition allowed
            if (rfScr.demolitionType == DemolitionType.None)
                return;

            // Object can't be cut
            if (rfScr.limitations.sliceByBlade == false)
                return;

            // Object available
            if (rfScr.limitations.available == false)
                return;

            // Global demolition state check
            if (rfScr.State() == false)
                return;

            // Slice object
            rfScr.AddSlicePlane (slicePlane);

            // Event
            sliceEvent.InvokeLocalEvent (this);
            RFSliceEvent.InvokeGlobalEvent (this);
        }

        // Get two points or slice
        Vector3[] GetSlicePlane()
        {
            // Get position and normal
            Vector3[] points = new Vector3[2];
            points[0] = transform.position;

            // Slice plane direction
            if (sliceType == PlaneType.XY)
                points[1] = transform.forward;
            else if (sliceType == PlaneType.XZ)
                points[1] = transform.up;
            else if (sliceType == PlaneType.YZ)
                points[1] = transform.right;

            return points;
        }

        // Check if object layer is in layer mask
        bool LayerCheck (int layerId)
        {
            //// Layer mask check
            //LayerMask layerMask = new LayerMask();
            //layerMask.value = mask;
            //if (LayerCheck(projectile.rb.gameObject.layer, layerMask) == true)
            //    Debug.Log("In mask " + projectile.rb.name);
            //else
            //    Debug.Log("Not In mask " + projectile.rb.name);
            return mask == (mask | (1 << layerId));
        }

        // Define components
        void DefineComponents()
        {
            // Check collider
            colLider = GetComponent<Collider>();

            // No collider. Add own
            if (colLider == null)
            {
                colLider = gameObject.AddComponent<MeshCollider>();
                ((MeshCollider)colLider).convex = true;
            }

            colLider.isTrigger = true;
        }
    }
}
