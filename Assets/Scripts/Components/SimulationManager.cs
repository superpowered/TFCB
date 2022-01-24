using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TFCB
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }

        public static event EventHandler<OnTickArgs> OnTick;

        public MapSystem MapSystem { get; private set; }

        private int _tick;
        private float _tickTimer;

        private void Awake()
        {
            EnforceSingleInstance();

            MapSystem = new MapSystem();

            _tick = 0;
            _tickTimer = 0;
        }

        public void Start()
        {
            MapSystem.Init();
        }

        private void EnforceSingleInstance()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void Update()
        {
            _tickTimer += Time.deltaTime;

            if (_tickTimer >= SimulationInfo.TickDuration)
            {
                _tick++;
                _tickTimer -= SimulationInfo.TickDuration;

                OnTick?.Invoke(this, new OnTickArgs { Tick = _tick });
            }
        }

        private void OnDisable()
        {
            MapSystem.Quit();
        }
    }
}
