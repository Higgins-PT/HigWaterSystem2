using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
namespace HigWaterSystem2
{
    public class OceanWaveformBase : HigSingleInstance<OceanWaveformBase>
    {
        public List<SimTexBase> simTexBases = new List<SimTexBase>();
        public override void ResetSystem()
        {
            
        }
        public override void DestroySystem()
        {

        }

    }
}
