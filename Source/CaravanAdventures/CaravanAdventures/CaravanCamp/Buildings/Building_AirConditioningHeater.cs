using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace CaravanAdventures.CaravanCamp.Buildings
{
    class Building_AirConditioningHeater : Building_TempControl
    {
        public override void TickRare()
        {
            if (this.compPowerTrader.PowerOn)
            {
                float ambientTemperature = base.AmbientTemperature;
                float num;
                if (ambientTemperature < this.compTempControl.targetTemperature)
                {
                    num = 1f;
                }
                else if (ambientTemperature > this.compTempControl.targetTemperature)
                {
                    num = -1f;
                }
                else
                {
                    num = 0;
                }
                float energyLimit = this.compTempControl.Props.energyPerSecond * num * 4.16666651f;
                float num2 = GenTemperature.ControlTemperatureTempChange(base.Position, base.Map, energyLimit, this.compTempControl.targetTemperature);
                bool flag = !Mathf.Approximately(num2, 0f);
                DLog.Message($"elmit: {energyLimit} num2: {num2} flag: {flag}");
                CompProperties_Power props = this.compPowerTrader.Props;
                var highPower = this.GetRoom().Temperature < 20 || this.GetRoom().Temperature > 22 ? true : false;
                if (flag)
                {
                    this.GetRoom().Temperature += num2;
                    this.compPowerTrader.PowerOutput = -props.basePowerConsumption;
                }
                else
                {
                    this.compPowerTrader.PowerOutput = -props.basePowerConsumption * this.compTempControl.Props.lowPowerConsumptionFactor;
                }

                this.compTempControl.operatingAtHighPower = flag;
            }
        }

    }
}
