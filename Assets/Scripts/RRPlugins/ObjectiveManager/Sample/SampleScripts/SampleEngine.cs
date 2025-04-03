using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RRObjective
{
    public class SampleEngine : MonoBehaviour
    {
        [SerializeField]
        SampleHud sampleHud = null;
        [SerializeField]
        GameObject[] boatsPrefabs = null;
        [SerializeField]
        int boatsCount = 2;

        // Start is called before the first frame update
        void Start()
        {
            sampleHud.Init();
            RRObjectiveManager.instance.onAddLoseDlg = OnLose;
            RRObjectiveManager.instance.onAddWinDlg = OnWin;

            //init Boats
            for( int i=0; i<boatsCount; i++ )
            {
                GameObject.Instantiate(boatsPrefabs[i % boatsPrefabs.Length]);
            }
        }

        // Update is called once per frame
        void Update()
        {
            float fElapsedTime = Time.deltaTime;
            RRObjectiveManager.instance.UpdateObjective("Time", -1, new ObjectiveFloat.FloatObjData(-fElapsedTime));

            if( Input.GetKeyDown( KeyCode.Escape ))
            {
                if( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "sampleCircle" )
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("sampleLine");
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("sampleCircle" );
                }
            }
        }

        private void OnLose( int nPlayerId )
        {
            RRObjectiveManager.instance.Pause();
            sampleHud.SetFinalText("You Lose");
        }

        private void OnWin(int nPlayerId)
        {
            RRObjectiveManager.instance.Pause();
            sampleHud.SetFinalText("Player " + nPlayerId +" Win !!" );
        }
    }
}
