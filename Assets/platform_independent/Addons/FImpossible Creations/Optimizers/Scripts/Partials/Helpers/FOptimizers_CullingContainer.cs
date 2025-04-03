using System;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class which is managing CullingGroup bounding spheres and sending culling groups' events to optimizer components
    /// Objects need to have the same distance values to be able to be saved in container
    /// </summary>
    //[Serializable] DEBUG
    public class FOptimizers_CullingContainer
    {
        public const int MaxSlots = 1000;
        public int ID { get; private set; }
        public bool HaveFreeSlots { get { return highestIndex < MaxSlots - 1; } }

        internal bool Destroying = false;

        public CullingGroup CullingGroup { get; private set; }
        public FOptimizer_Base[] Optimizers { get; private set; }
        public BoundingSphere[] CullingSpheres { get; private set; }

        private float[] distanceLevels;
        private int boundingCount;
        private int highestIndex;
        private int lastRemovedIndex;


        public FOptimizers_CullingContainer()
        {
            Optimizers = new FOptimizer_Base[MaxSlots];
        }


        /// <summary>
        /// Initializing container with distances, defining ID and preparing CullingGroup to work
        /// </summary>
        public void InitializeContainer(int id, float[] distances, Camera targetCamera)
        {
            ID = id;

            distanceLevels = new float[distances.Length + 2];
            distanceLevels[0] = 0.001f; // I'm disappointed I have to use additional distance to allow detect first frame culling event catch everything

            for (int i = 1; i < distances.Length + 1; i++)
                distanceLevels[i] = distances[i - 1];

            // Additional distance level to be able detecting frustum ranges, instead of frustum with distance ranges combined
            distanceLevels[distanceLevels.Length - 1] = distances[distances.Length - 1] * 1.5f;

            CullingGroup = new CullingGroup { targetCamera = targetCamera };

            CullingSpheres = new BoundingSphere[MaxSlots];

            CullingGroup.SetBoundingSpheres(CullingSpheres);
            boundingCount = 0;
            highestIndex = -1;
            lastRemovedIndex = -1;
            CullingGroup.SetBoundingSphereCount(boundingCount);

            CullingGroup.onStateChanged = CullingGroupStateChanged;

            CullingGroup.SetBoundingDistances(distanceLevels);
            CullingGroup.SetDistanceReferencePoint(targetCamera.transform);
        }


        /// <summary>
        /// Setting new main camera
        /// </summary>
        public void SetNewCamera(Camera cam)
        {
            CullingGroup.targetCamera = cam;
            CullingGroup.SetDistanceReferencePoint(cam.transform);
        }


        /// <summary>
        /// Returns true if list have free slots
        /// </summary>
        public bool AddOptimizer(FOptimizer_Base optimizer)
        {
            if (!HaveFreeSlots) return false;

            int nextId = highestIndex+1;

            CullingSpheres[nextId].position = optimizer.GetReferencePosition();
            CullingSpheres[nextId].radius = optimizer.DetectionRadius * FOptimizer_Base.GetScaler(optimizer.transform);
            Optimizers[nextId] = optimizer;

            optimizer.AssignToContainer(this, nextId, ref CullingSpheres[nextId]);

            highestIndex++;
            boundingCount++;
            CullingGroup.SetBoundingSphereCount(boundingCount);

            return true;
        }


        /// <summary>
        /// Remove optimizer from container and freeing slot for another optimizer
        /// </summary>
        public void RemoveOptimizer(FOptimizer_Base optimizer)
        {
            if (Optimizers == null) return;

#if UNITY_EDITOR
            if (FOptimizers_Manager.AppIsQuitting) return;
#endif

            lastRemovedIndex = optimizer.ContainerSphereId;
            Optimizers[lastRemovedIndex] = null;
            MoveStackOptimizerToFreeSlot();
        }


        private void MoveStackOptimizerToFreeSlot()
        {
            FOptimizer_Base optimizerToMove = Optimizers[highestIndex];
            Optimizers[highestIndex] = null;
            highestIndex--;
            boundingCount--;

            if (optimizerToMove == null) return;
            int freeSlot = lastRemovedIndex;
            lastRemovedIndex = highestIndex + 1;

            CullingSpheres[freeSlot].position = optimizerToMove.GetReferencePosition();
            CullingSpheres[freeSlot].radius = optimizerToMove.DetectionRadius * FOptimizer_Base.GetScaler(optimizerToMove.transform);
            Optimizers[freeSlot] = optimizerToMove;

            optimizerToMove.AssignToContainer(this, freeSlot, ref CullingSpheres[freeSlot]);
        }


        /// <summary>
        /// Culling state changed for one culling sphere from container
        /// </summary>
        private void CullingGroupStateChanged(CullingGroupEvent cullingEvent)
        {
            if (Optimizers[cullingEvent.index] != null)
            {
                Optimizers[cullingEvent.index].CullingGroupStateChanged(cullingEvent);
            }
        }


        /// <summary>
        /// Cleaning culling group from memory
        /// </summary>
        public void Dispose()
        {
            CullingGroup.Dispose();
            CullingGroup = null;
            Optimizers = null;
        }


        /// <summary>
        /// Generating id for distance set
        /// </summary>
        public static int GetId(float[] distances)
        {
            int id = distances.Length * 179;
            for (int i = 0; i < distances.Length; i++)
            {
                id += (int)distances[i] / 2;
            }

            return id;
        }
    }
}
