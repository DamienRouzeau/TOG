//#define DEBUG_STATS

using System.Collections.Generic;
using UnityEngine;
using RRLib;

namespace RRStats
{
    
    internal class RRListenerStats
    {
        internal Dictionary<string, double> m_stats;

        internal RRListenerStats()
        {
            m_stats = new Dictionary<string, double>();
        }

        internal void IncrementStat(string statId, double value)
        {
            if( m_stats.ContainsKey(statId))
            {
                m_stats[statId] += value;
            }
            else
            {
                m_stats.Add(statId, value);
            }
        }

        internal void SetStat(string statId, double value)
        {
            if (m_stats.ContainsKey(statId))
            {
                m_stats[statId] = value;
            }
            else
            {
                m_stats.Add(statId, value);
            }
        }

        internal double GetStat(string statId)
        {
            if (!m_stats.ContainsKey(statId))
            {
                return 0;
            }
            return m_stats[statId];
        }

        internal Dictionary<string, double> GetStats()
        {
            return m_stats;
        }
    }

    public class RRStatsManager : RRSingletonMonoBehaviour<RRStatsManager>
    {
        private Dictionary<string, RRListenerStats> m_listenersStats;

        #region public method
        /// <summary>
        /// AddListener, to register new listener
        /// </summary>
        /// <param name="listenerId"> id of our object</param>
        public void AddListener( string listenerId )
        {
#if DEBUG_STATS
            Debug.Log($"[STATS] AddListener {listenerId}");
#endif
            if ( m_listenersStats.ContainsKey(listenerId))
            {
                Debug.LogError("Listener Already register!");
                return;
            }
            m_listenersStats.Add(listenerId, new RRListenerStats());
        }

        /// <summary>
        /// IncrementStat, to increment a stat
        /// </summary>
        /// <param name="listenerId"> id of our object</param>
        /// <param name="statId"> id of our stat</param>
        /// <param name="value"> value to increment</param>
        public void IncrementStat(string listenerId, string statId, double value )
        {
#if DEBUG_STATS
            Debug.Log($"[STATS] IncrementStat {listenerId} {statId} {value}");
#endif
            if (!m_listenersStats.ContainsKey(listenerId))
            {
                AddListener(listenerId);
            }
            m_listenersStats[listenerId].IncrementStat(statId, value);
        }

		public void ResetStats()
		{
            if (m_listenersStats != null)
                m_listenersStats.Clear();
        }

		/// <summary>
		/// Setup a stat to a determinate value
		/// </summary>
		/// <param name="listenerId"> id of our object</param>
		/// <param name="statId"> id of our stat</param>
		/// <param name="value"> value to set</param>
		public void SetStat(string listenerId, string statId, double value)
        {
#if DEBUG_STATS
            Debug.Log($"[STATS] SetStat {listenerId} {statId} {value}");
#endif
            if (!m_listenersStats.ContainsKey(listenerId))
            {
                AddListener(listenerId);
            }
            m_listenersStats[listenerId].SetStat(statId, value);
        }

        public double GetStat(string listenerId, string statId)
        {
            if (!m_listenersStats.ContainsKey(listenerId))
            {
                return 0;
            }
            return m_listenersStats[listenerId].GetStat(statId);
        }

        public Dictionary<string,double> GetStats( string listenerId )
        {
            if (!m_listenersStats.ContainsKey(listenerId))
            {
                return null;
            }
            return m_listenersStats[listenerId].GetStats();
        }

        public string GetBestListenerForStat( string statId )
        {
            return null;
        }

        public string GetBestListenerForStatInList(string statId, List<string> listeners )
        {
            double max = 0;
            string best = null;

            for( int i=0; i<listeners.Count; i++ )
            {
                if( m_listenersStats.ContainsKey(listeners[i]))
                {
                    double current = m_listenersStats[listeners[i]].GetStat(statId);
                    if( current > max )
                    {
                        max = current;
                        best = listeners[i];
                    }

                }
            }
            return best;
        }


        #endregion

        #region private function

        private void Awake()
        {
            m_listenersStats = new Dictionary<string, RRListenerStats>();
        }
        #endregion
    }
}
