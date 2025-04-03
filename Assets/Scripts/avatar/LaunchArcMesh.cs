using System.Collections.Generic;
using UnityEngine;

public class LaunchArcMesh : MonoBehaviour
{
    [SerializeField]
    private Transform _hand = null;
    [SerializeField]
    private PlayerUpdateAim _aim = null;
    [SerializeField]
    private float _arcAngle = 20f;
    [SerializeField]
    private float _delayToChangeCollider = 0.15f;

    Mesh mesh = null;

    float distance;
    float angle;
    int resolution;

    float g; // gravity
    float radianAngle;

    GameObject arcfather;
    pointfromhand pfh;
    GameObject camerarig = null;

    private Collider _lastCollider = null;
    private float _lastColliderTime = 0f;
    private Vector3 _lastColliderPos = Vector3.zero;

    private const int _arcSecurity = 64;
    private List<Vector3> _arcPosList = new List<Vector3>(_arcSecurity);

    private int _layersToTest = 0;
    private int _wallsLayer = 0;

    private void Awake()
    {
        resolution = gamesettings_ui.myself.hand_arc_resolution;
        mesh = GetComponent<MeshFilter>().mesh;
        g = Mathf.Abs(Physics.gravity.y);
        arcfather = gameObject.transform.parent.gameObject;
        pfh = arcfather.transform.parent.gameObject.GetComponent<pointfromhand>();

        GameObject play = gameObject;
        Player pl = play.GetComponent<Player>();
        while (pl == null)
        {
            play = play.transform.parent.gameObject;
            pl = play.GetComponent<Player>();
        }
        camerarig = play;

        _layersToTest = LayerMask.GetMask("Floor", "Walls");
        _wallsLayer = LayerMask.NameToLayer("Walls");
        RunCalculation();
    }

	private void OnEnable()
	{
        _lastCollider = null;
        _lastColliderTime = Time.time;
        if (pfh != null)
            pfh.teleportCollider = null;
    }

	void RunCalculation()
    {
        MakeArcMesh(CalculatArcArray());
    }

    private void Update()
    {
        gamesettings_ui gsUI = gamesettings_ui.myself;

        Quaternion q = _hand.rotation;
        Vector3 target = q * Vector3.forward * 10.0f;
        float ang = Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg;


        Vector3 v = arcfather.transform.eulerAngles;
        v.x = v.z = 0;
        v.y = ang;
        arcfather.transform.eulerAngles = v;

       
        float newAngle = -_hand.localEulerAngles.x - _aim.aimAngle + _arcAngle;
        while (newAngle > 180.0f)
            newAngle -= 360.0f;
        while (newAngle < -180.0f)
            newAngle += 360.0f;
        newAngle = Mathf.Clamp(newAngle * gsUI.hand_arc_angle_coef + gsUI.hand_arc_angle_offset, gsUI.hand_arc_angle_start, gsUI.hand_arc_angle_end);
        float ratio = Mathf.InverseLerp(gsUI.hand_arc_angle_start, gsUI.hand_arc_angle_end, angle);
        if (gsUI.hand_arc_distance_angle_evol != null)
		{
            ratio = gsUI.hand_arc_distance_angle_evol.Evaluate(ratio);
        }
        distance = Mathf.Lerp(gsUI.hand_arc_distance_min, gsUI.hand_arc_distance_max, ratio);
        angle = newAngle;

        RunCalculation();
    }

    Vector3[] CalculatArcArray()
    {
        Material mat = gameObject.GetComponent<Renderer>().material;
        mat.SetColor("_Color", Color.red);
        bool validTeleport = false;
        int security = _arcSecurity;

        radianAngle = Mathf.Deg2Rad * angle;

        _arcPosList.Clear();
        Vector3 startposition = gameObject.transform.position;
        Vector3 lastposition = startposition;
        int i = 1;
        _arcPosList.Add(Vector3.zero);
        while (true)
        {
            float t = (float)i / (float)resolution;
            Vector3 localArcPos = CalculateArcPoint(t, distance);
            Vector3 nw = transform.TransformPoint(localArcPos);
            if (RayCastCheck(lastposition, ref nw, out bool isWall))
            {
                if (NearAvatarCheck(lastposition) && !isWall)
                {
                    validTeleport = true;
                    mat.SetColor("_Color", Color.green);
                }
                break;
            }
            lastposition = nw;
            _arcPosList.Add(localArcPos);

            security--;
            if (security <= 0)
                break;
            i++;
        }

        if (_lastCollider != null && !validTeleport)
        {
            _lastColliderTime = Time.time;
            _lastCollider = null;
        }

        if (pfh.teleportCollider == null)
		{
            if (_lastCollider != null)
            {
                pfh.teleportCollider = _lastCollider;
                pfh.validteleportpos = _lastColliderPos;
            }
        }
        else 
		{
            if (Time.time - _lastColliderTime > _delayToChangeCollider)
            {
                pfh.teleportCollider = _lastCollider;
                pfh.validteleportpos = _lastColliderPos;
            }
        }
        pfh.validteleport = pfh.teleportCollider != null;

        return _arcPosList.ToArray();
    }

    Vector3 CalculateArcPoint(float t,float maxDistance)
    {
        float x = t * maxDistance;
        float y = 0;
        float v = distance * Mathf.Cos(radianAngle);
        float v2 = v * v;
        if (!Mathf.Approximately(v2, 0.0f))
            y = x * Mathf.Tan(radianAngle) - (g * x * x) / (2f * v2);
        return new Vector3(0,y,x);
    }


    // Populating renderer
    void MakeArcMesh(Vector3[] arcVerts)
    {
        mesh.Clear();
        int rresolution = arcVerts.Length - 1;
        if (rresolution < 1)
            return;
        Vector3[] vertices = new Vector3[(rresolution+1)*2];
        Vector2[] uvs = new Vector2[(rresolution+1) * 2];
        int[] triangles = new int[rresolution * 12];
        /*
        Vector2 uv1 = new Vector2(0, 0);
        Vector2 uv2 = new Vector2(gamesettings._myself.hand_arc_tile_u, 0);
        Vector2 uv3 = new Vector2(gamesettings._myself.hand_arc_tile_u, gamesettings._myself.hand_arc_tile_v);
        Vector2 uv4 = new Vector2(0, gamesettings._myself.hand_arc_tile_v);
*/
        float vstep = 0.98f / (float)rresolution;
        Vector2 uv1 = new Vector2(0, 0.01f);
        Vector2 uv2 = new Vector2(1, 0.01f);

        float meshwidth = gamesettings_ui.myself.hand_arc_startwidth;
        float deltawidth = (gamesettings_ui.myself.hand_arc_endwidth - gamesettings_ui.myself.hand_arc_startwidth) / (float)rresolution;

        for (int i = 0; i <= rresolution; i++)
        {
            vertices[i * 2] = new Vector3(meshwidth * 0.5f,arcVerts[i].y, arcVerts[i].z);
            vertices[i * 2 + 1] = new Vector3(meshwidth * (-0.5f), arcVerts[i].y, arcVerts[i].z);
            uvs[i * 2] = uv2;
            uvs[i * 2 + 1] = uv1;
            uv1.y += vstep;
            uv2.y += vstep;
            meshwidth += deltawidth;

            if (i != rresolution)
            {
                triangles[i * 12] = i * 2 + 0;
                triangles[i * 12 + 1] = triangles[i * 12 + 4] = i * 2 + 1;
                triangles[i * 12 + 2] = triangles[i * 12 + 3] = i * 2 + 2;
                triangles[i * 12 + 5] = i * 2 + 3;

                triangles[i * 12 + 6] = i * 2 + 0;
                triangles[i * 12 + 6 + 1] = triangles[i * 12 + 6 + 4] = i * 2 + 2;
                triangles[i * 12 + 6 + 2] = triangles[i * 12 + 6 + 3] = i * 2 + 1;
                triangles[i * 12 + 6 + 5] = i * 2 + 3;
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }




    public bool RayCastCheck(Vector3 oldcoord, ref Vector3 objpos, out bool isWall)
    {
        bool result = false;
        isWall = false;
        float distance = Vector3.Distance(oldcoord, objpos);
        Vector3 direction = (objpos - oldcoord) / distance;
        RaycastHit[] hits = Physics.RaycastAll(oldcoord, direction, distance, _layersToTest);
        if (hits.Length > 0)
        {
            RaycastHit washit = hits[0];
            float mindist = 10000000.0f;
            float minWallDist = mindist;
            foreach (RaycastHit hit in hits)
            {
                if (gameObject != hit.collider.gameObject)
                {
                    if (hit.collider.gameObject.layer == _wallsLayer)
                    {
                        if (hit.distance < minWallDist)
                        {
                            minWallDist = hit.distance;
                        }
                    }
                    else
                    {
                        if (hit.distance < mindist)
                        {
                            mindist = hit.distance;
                            washit = hit;
                        }
                    }
                }
            }

            objpos = washit.point;

            if (minWallDist < mindist)
            {
                isWall = true;
                return true;
            }

            if (Player.myplayer.forcedTeleportTarget != null)
            {
                if (Vector3.SqrMagnitude(Player.myplayer.forcedTeleportTarget.position - objpos) > 0.1f)
                    return false;
            }

			Vector3 newPos = washit.point - camerarig.transform.position;
            if (_lastCollider != washit.collider)
			{
                _lastColliderPos = newPos;
                _lastCollider = washit.collider;
                _lastColliderTime = Time.time;
            }
            else
			{
				float sqrDistNewPos = Vector3.SqrMagnitude(newPos - _lastColliderPos);
				float speed = Mathf.Lerp(2f, 20f, Mathf.InverseLerp(0.1f, 1f, sqrDistNewPos));
                _lastColliderPos = Vector3.Lerp(_lastColliderPos, newPos, Time.deltaTime * speed);
			}
            result = true;
        }

        return result;
    }

    public bool NearAvatarCheck(Vector3 pos)
    {
        float nearDist = gamesettings_ui.myself.hand_arc_disable_near_avatar_distance;
        float nearSqrDist = nearDist * nearDist;
        foreach (var avatar in Player.myplayer.avatars)
        {
            if (avatar.actornumber >= 0 && avatar.isVisible && !avatar.isInPause)
            {
                if (Vector3.SqrMagnitude(avatar.transform.position - pos) < nearSqrDist)
                    return false;
            }
        }
        return true;
    }

}
