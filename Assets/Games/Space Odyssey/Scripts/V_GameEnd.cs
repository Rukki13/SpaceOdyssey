using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Whack_a_Mole.UserInterface
{
    public class V_GameEnd : MonoBehaviour
    {
        [SerializeField] private GameObject v_gameEnd;
        [SerializeField] private GameObject v_won;
        [SerializeField] private GameObject v_lost;

        private void Start()
        {
            GameManager.onGameEnd += OnGameEnded;
        }

        public void OnGameEnded(bool won)
        {
            Debug.Log("result");
            v_gameEnd.SetActive(true);
            v_won.SetActive(won);
            v_lost.SetActive(!won);
        }
    }
}
