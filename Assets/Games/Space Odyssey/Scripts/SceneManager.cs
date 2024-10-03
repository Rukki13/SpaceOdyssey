using Photon.Pun;
using SpaceOdyssey.Frames;
using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey
{
    public class SceneManager : MonoBehaviourPun
    {
        private int seed;

        void Start()
        {
            GameManager.onGameStart += OnGameStart;
        }

        private void OnGameStart()
        {
            if (PhotonNetwork.IsMasterClient || GameManager.isAgainstBot)
            {
                seed = Random.Range(0, 999999);
                if (!GameManager.isAgainstBot)
                {
                    photonView.RPC("SyncSeed", RpcTarget.All, seed);
                }
                else
                {
                    SyncSeed(seed);
                }
            }
        }
        [PunRPC]
        private void SyncSeed(int seed)
        {
            PlanetGenerator.I.Initialize(seed);
            FrameLogic.I.isPlaying = true;
        }
    }
}
