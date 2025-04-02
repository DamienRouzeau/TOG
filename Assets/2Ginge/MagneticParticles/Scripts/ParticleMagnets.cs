using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ParticleMagnets : MonoBehaviour {

    [System.Serializable]
    class FVector3
    {
        public float x, y, z;
    }

    [System.Serializable]
    public class Magnet
    {
        [HideInInspector]
        public string name = "Magnet";

        
        public enum MagnetType { Repulse, Attract};
        public enum MagnetEffects { Position = 0, Direction = 1, BoundedPosition = 3, BoundedDirection = 4, PositionalNoise = 5, DirectionalNoise = 6};
        
        public MagnetType magnetType;
        private MagnetType currentMagnetType;

        public MagnetEffects magnetEffectType;
        private MagnetEffects currentMagnetEffect;

        public Transform magnetTransform;
        public float scale = 1, distance = 1;
        //using delegates in order to avoid many switch statements per particle;
        public delegate void EffectorDelegate(ref Vector3 p_pos, ref Vector3 p_vel);
        public EffectorDelegate currentDelegate;

        private int repulse_attract = 1;
        public bool visualize = false;
        [HideInInspector]
        public bool localSpace = false;
        public bool enabledToggle = true;
        private Vector3 _pos, tmp, _dir;
        private FVector3 f1;
        private float val;
        private float delta = 0;

        public void SetFrame(Transform _localTrans)
        {

            //this.name = "Magnet";
            delta = Mathf.Min(Time.deltaTime, Time.fixedDeltaTime);
            if (localSpace)
            {
                _pos = _localTrans.InverseTransformPoint(magnetTransform.position);
            }
            else
            {
                _pos = magnetTransform.position;
            }
        }

        

        void FastSub(ref Vector3 a, ref Vector3 b)
        {
            tmp.x = a.x - b.x;
            tmp.y = a.y - b.y;
            tmp.z = a.z - b.z;
        }

        void FastMul(ref Vector3 a, float s)
        {
            tmp.x = a.x * s;
            tmp.y = a.y * s;
            tmp.z = a.z * s;

        }

        void FastDiv(ref Vector3 a, float s)
        {
            if (s == 0)
            {
                tmp = a;
            }
            else
            {
                tmp.x = a.x / s;
                tmp.y = a.y / s;
                tmp.z = a.z / s;
            }
        }
        void BoundedPosDelegate(ref Vector3 p_pos, ref Vector3 p_vel)
        {
            _dir = p_pos;
            FastSub(ref _pos, ref _dir);
            _dir = tmp;
            val = _dir.sqrMagnitude;
            if (distance * distance - val <= 0)
                return;
            val = (distance - Mathf.Sqrt(val));
            //if (value <= 0)
            //    return;
            val = Mathf.Clamp(val, 0.0f, 1.0f) * delta * repulse_attract * scale;
            FastMul(ref _dir, val);
            p_pos += tmp;
        }
        void PositionalNoiseDelegate(ref Vector3 p_pos, ref Vector3 p_vel)
        {
            //tmp = p.position;
            //FastSub(ref _pos, ref tmp);
            //_dir = tmp;
            //float value = Mathf.Clamp((distance - dir.magnitude), 0.0f, 1.0f) * scale;
            //if (value == 0)
            _dir = Vector3.Lerp(p_vel.normalized,Random.insideUnitSphere,Mathf.Clamp01(distance));
            //    return;
            val = delta * repulse_attract * scale;
            FastMul(ref _dir, val);
            p_pos += tmp;
        }
        void DirectionalNoiseDelegate(ref Vector3 p_pos, ref Vector3 p_vel)
        {
            //tmp = p.position;
            //FastSub(ref _pos, ref tmp);
            //Vector3 dir = tmp;
            //float value = Mathf.Clamp((distance - dir.magnitude), 0.0f, 1.0f) * scale;
            //if (value == 0)
            _dir = Vector3.Lerp(p_vel.normalized, Random.insideUnitSphere, Mathf.Clamp01(distance));
            //    return;
            val = delta * repulse_attract * scale;
            FastMul(ref _dir, val);
            p_vel += tmp;
        }
        void BoundedDirDelegate(ref Vector3 p_pos, ref Vector3 p_vel)
        {
            _dir = p_pos;
            FastSub(ref _pos, ref _dir);
            _dir = tmp;
            val = (distance - _dir.magnitude);
            if (val <= 0)
                return;
            val = Mathf.Clamp(val, 0.0f, 1.0f) * delta * repulse_attract * scale;
            FastMul(ref _dir, val);
            p_vel += tmp;
        }
        void PosDelegate(ref Vector3 p_pos, ref Vector3 p_vel)
        {
            _dir = p_pos;
            FastSub(ref _pos, ref _dir);
            _dir = tmp;

            val = (_dir.sqrMagnitude * distance);

            FastDiv(ref _dir, val);
            _dir = tmp;
            FastMul(ref _dir, delta * scale * repulse_attract);

            p_pos += tmp;
        }
        void DirDelegate(ref Vector3 p_pos, ref Vector3 p_vel)
        {
            //this adds the direction to the velocity;
            _dir = p_pos;
            FastSub(ref _pos, ref _dir);
            _dir = tmp;
            val = (_dir.sqrMagnitude * distance);

            FastDiv(ref _dir, val);
            _dir = tmp;
            FastMul(ref _dir, delta * scale * repulse_attract);
            p_vel += tmp;
        }
        //not very useful.
        void QDelegate(ref Vector3 p_pos, ref Vector3 p_vel)
        {
            //this one rotates the direction to or from
            Vector3 dir = (_pos - p_pos);
            Vector3 pDir = p_vel;
            
            if (Mathf.Abs(Vector3.Dot(pDir.normalized, dir.normalized)) != 0)
            {
                tmp = Vector3.RotateTowards(dir.normalized, pDir.normalized * -1, scale * (distance / dir.magnitude), float.MaxValue);
                //Vector3 c = Vector3.Cross(dir.normalized, pDir.normalized).normalized;
                //Quaternion nQ = Quaternion.RotateTowards(Quaternion.LookRotation(pDir.normalized, c), Quaternion.LookRotation(dir.normalized, c), Time.deltaTime * repulse_attract);
                //p.velocity += nQ * Vector3.forward * scale * (distance/dir.magnitude) * Time.deltaTime * repulse_attract;
                p_vel = tmp.normalized * pDir.magnitude;
            }
            else
            {

                //just rotate it any direction. as all directions are equal.
            }

        }
        public void SetDelegates()
        {
            switch(magnetType)
            {
                case (MagnetType.Attract):
                    repulse_attract = 1;
                    break;
                case (MagnetType.Repulse):
                    repulse_attract = -1;
                    break;
                default:
                    repulse_attract = 1;
                    break;
            }
            switch (magnetEffectType)
            {
                case (MagnetEffects.Direction):
                    currentDelegate = DirDelegate;
                    break;
                case (MagnetEffects.Position):
                    currentDelegate = PosDelegate;
                    break;
                case (MagnetEffects.BoundedPosition):
                    currentDelegate = BoundedPosDelegate;
                    break;
                case (MagnetEffects.BoundedDirection):
                    currentDelegate = BoundedDirDelegate;
                    break;
                case (MagnetEffects.PositionalNoise):
                    currentDelegate = PositionalNoiseDelegate;
                    break;
                case (MagnetEffects.DirectionalNoise):
                    currentDelegate = DirectionalNoiseDelegate;
                    break;
                default:
                    currentDelegate = PosDelegate;
                    break;
            }
            currentMagnetEffect = magnetEffectType;
            currentMagnetType = magnetType;
        }
        public void ApplyEffectors(ref Vector3 p_pos, ref Vector3 p_vel)
        {
            
            //check if the current type of the function is the current type selcted;
            if(currentMagnetEffect == magnetEffectType && magnetType == currentMagnetType)
            {
                //apply logic
                currentDelegate(ref p_pos, ref p_vel);
            }
            else
            {
                //rebuild, apply logic
                SetDelegates();
                currentDelegate(ref p_pos, ref p_vel);
            }
        }
    }

    // Use this for initialization
    
    public Magnet[] magnets;
    private ParticleSystem system;
    [SerializeField]
    ParticleSystem.Particle[] particles;

    void Start () {
        //looks like the issue could be here.
        if (system == null)
        {
            AttemptStart();
        }
    }
    void AttemptStart()
    {

        system = GetComponent<ParticleSystem>();
        if (magnets != null)
            for (int i = 0; i < magnets.Length; ++i)
            {
                magnets[i].SetDelegates();
            }
    }
    void OnDrawGizmos()
    {
        
        if (magnets != null)
        {

            for (int i = 0; i < magnets.Length; ++i)
            {
                magnets[i].name = "Magnet";
                if (magnets[i].visualize)
                {
                    Gizmos.DrawSphere(magnets[i].magnetTransform.position, magnets[i].distance);
                }
                if (magnets[i].enabledToggle)
                {
                    if (magnets[i].magnetType == Magnet.MagnetType.Repulse)
                    {
                        Gizmos.DrawIcon(magnets[i].magnetTransform.position, "MagnetGizmoBlue_128x128.png", true);
                    }
                    else
                    {
                        Gizmos.DrawIcon(magnets[i].magnetTransform.position, "MagnetGizmoRed_128x128.png", true);
                    }
                }
            }
        }
    }



    // Update is called once per frame
    void Update () {
        

	    if (system.isStopped)
	    {
	        if (particles != null)
	            particles = null;
            return;
        }
	    if (system.isPaused)
	    {
	        return;
	    }
        if(system.particleCount == 0)
        {
            return;
        }
        if (particles == null)
        {
            particles = new ParticleSystem.Particle[system.particleCount];
        }
        //lets try something a bit differently
        /*
        if (particles.Length != system.particleCount)
        {
            particles = new ParticleSystem.Particle[system.particleCount];
        }
        */
        //super stable
	    if (particles.Length != system.main.maxParticles)
	    {
            particles = new ParticleSystem.Particle[system.main.maxParticles];
        }
	    int max = system.main.maxParticles;
        system.GetParticles(particles);
        //check for dead particles
        List<ParticleSystem.Particle> goodParticles = new List<ParticleSystem.Particle>();
        for(int i = 0; i < particles.Length; ++i)
        {
            if(particles[i].remainingLifetime > 0)
            {
                goodParticles.Add(particles[i]);
            }
        }
        particles = goodParticles.ToArray();

        bool localSpace = false;
        localSpace = (system.main.simulationSpace == ParticleSystemSimulationSpace.Local) ? true : false;

	    if (magnets != null)
	    {
            //if there is a disabled magnet, lets construct a new array as if there are 20k particles, it is an additional 20k condition checks.
            int numberOfEnabledMagnets = 0;
            for(int k = 0; k < magnets.Length; ++k)
            {
                if (magnets[k].enabledToggle)
                    numberOfEnabledMagnets += 1;
                magnets[k].localSpace = localSpace;
            }
            Magnet[] tmpMagnets = new Magnet[numberOfEnabledMagnets];
            for (int ind = 0, k = 0; k < magnets.Length; ++k)
            {
                if (magnets[k].enabledToggle)
                {
                    tmpMagnets[ind] = magnets[k];
                    ind += 1;
                }
            }
            //per frame setup
            for (int k = 0; k < tmpMagnets.Length; ++k)
            {
                if (tmpMagnets[k].currentDelegate == null)
                {
                    tmpMagnets[k].SetDelegates();

                }
                tmpMagnets[k].SetFrame(transform);
            }
            Vector3 pPos;
	        Vector3 pVel;
            for (int i = 0; i < particles.Length; ++i)
            {
                //set pos and vel here, only accessed once;
                pPos = particles[i].position;
                pVel = particles[i].velocity;
                for (int j = 0; j < tmpMagnets.Length; ++j)
	            {
                    //if (magnets[j].magnetTransform != null)
                    //{
                    //
                    //}


                    //Vector3 dir = (transform.position - particles[i].position);
                    ////particles[i].position += dir/(dir.magnitude * distMagnitude) * Time.deltaTime * particleSpeed;
                    //particles[i].velocity += dir / (dir.magnitude * distMagnitude) * Time.deltaTime * particleSpeed;
                    //if (i > max)
                    //    break;



                    tmpMagnets[j].ApplyEffectors(ref pPos, ref pVel);
	            }
                //apply pos and vel here, only accessed once;
                particles[i].position = pPos;
                particles[i].velocity = pVel;
            }

        }

	    system.SetParticles(particles, particles.Length);
        
	}
}
