using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class ObjectivePath : Objectives
    {
        public class PathObjData : ObjData
        {
            public Vector2 v;

            public PathObjData( Vector2 vInitial )
            {
                v = vInitial;
            }

            public override void UpdateData(ObjData other)
            {
                v = ((PathObjData)other).v;
            }

            public override ObjData Copy()
            {
                PathObjData pathObjData = new PathObjData(v);
                return pathObjData;
            }
        }


        [SerializeField]
        PathHall[] m_halls = null;
        [SerializeField]
        float goalValue = 1f;

        RRLib.RRRndArray m_hallSelector;

        internal override void Setup()
        {
            base.Setup();
            m_hallSelector = new RRLib.RRRndArray((uint)m_halls.Length);
        }


        internal override void AddPlayer(RRObjectivePlayer player)
        {
            Debug.Assert(m_halls != null && m_halls.Length > 0, "You have no halls set in your Objective Path");

            base.AddPlayer(player);
            // get given laterality
            ObjData playerData = null;
            if (m_playersValues.TryGetValue(player.playerId, out playerData))
            {
                PathObjData pathObjData = (PathObjData)playerData;
                PathFollower pathFollower = player.GetComponent<PathFollower>();
                if( pathFollower!=null )
                {
                    int nPathId = pathFollower.preferredPath ;
                    PathHall pathHall = null;
                    if( nPathId<0 || m_hallSelector.HasValueBeenChoosen( (uint)nPathId ) )
                    {
                        pathHall = m_halls[m_hallSelector.ChooseValue()];
                    }
                    else
                    {
                        m_hallSelector.SetValueAsChoosen((uint)nPathId);
                        pathHall = m_halls[nPathId];
                    }
                    pathFollower.Setup( player.playerId, pathHall, pathObjData.v.y);
                }
            }
        }

        internal override void GetInitialAndTargetValue(out ObjData initial, out ObjData target)
        {
            initial = new PathObjData(new Vector2(0f, 0f));
            target = new PathObjData(new Vector2(goalValue, 0f));
        }

        internal override bool CheckObjectiveComplete(int playerId, ObjData data)
        {
            PathObjData pathObjData = (PathObjData)data;
            return (pathObjData.v.x >= goalValue);
        }


        internal override void AddPlayerWithInitialValues(int nPlayerId)
        {
            Vector2 v = new Vector2(0f, 0f);
            PathObjData pathObjData = new PathObjData(v);
            m_playersValues.Add(nPlayerId, pathObjData);
            RegisterPlayerInstant(nPlayerId, pathObjData);
        }
    }
}
