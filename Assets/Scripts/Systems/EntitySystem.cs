
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace TFCB
{
    public class EntitySystem : SimulationSystem
    {
        public static event EventHandler<OnCitizenEventArgs> OnCreateCitizen;
        private List<Citizen> _citizenList;

        public override void Init()
        {
            SetupEvents();

            CreateCitizens();
        }

        private void SetupEvents()
        {
            SimulationManager.OnTick += Tick;
        }

        private void CreateCitizens()
        {
            _citizenList = new List<Citizen>(EntityInfo.TotalCitizens);

            for (int i = 0; i < EntityInfo.TotalCitizens; i++)
            {
                int2? position = SimulationManager.Instance.MapSystem.GetOpenPosition();
                if (position == null)
                {
                    break; // TODO: what do we do if this happens?
                }
                Citizen newCitizen = new Citizen
                {
                    Direction = Utils.RandomEnumValue<Direction>(),
                    Nation = Utils.RandomEnumValue<Nation>(),
                    Position = (int2)SimulationManager.Instance.MapSystem.GetOpenPosition(),
                };

                _citizenList.Add(newCitizen);

                OnCreateCitizen?.Invoke(this, new OnCitizenEventArgs { Citizen = newCitizen });
            }
        }

        protected override void Tick(object sender, OnTickArgs eventArgs)
        {
            if (_citizenList == null || _citizenList.Count <= 0)
            {
                return;
            }

            foreach (Citizen citizen in _citizenList)
            {
                citizen.Tick();
            }
        }

        private void TearDownEvents()
        {
            SimulationManager.OnTick -= Tick;
        }

        public override void Quit()
        {
            TearDownEvents();
        }
    }
}