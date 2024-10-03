using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Turns
{
    public class TurnManager : MonoBehaviourPun
    {
        public static TurnManager I;

        public bool myTurn;

        private void Awake()
        {
            I = this;
        }

        private void Start()
        {
            GameManager.onGameStart += OnGameStart;
        }

        private void OnGameStart()
        {
            if (PhotonNetwork.IsMasterClient)
                SyncSetTurnLogic(PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [PunRPC]
        private void SetTurnLogic(int actorNbr)
        {
            if (actorNbr == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                myTurn = true;
            }
            else
                myTurn = false;
        }
        private void SyncSetTurnLogic(int actorNbr)
        {
            if (GameManager.isAgainstBot)
            {
                SetTurnLogic(actorNbr);
            }
            else
            {
                photonView.RPC("SetTurnLogic", RpcTarget.All, actorNbr);
            }
        }

        public void OnFinishedTurn()
        {
            if (myTurn)
            {
                SyncSetTurnLogic(PhotonManager.enemyPlayer.ActorNumber);
            }
            else
            {
                SyncSetTurnLogic(PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }
}
