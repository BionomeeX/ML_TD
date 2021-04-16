using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace MLTD.ML
{
    public class Decision
    {
        private float[] InputToFloatArray(InputData input)
        {
            List<float> data = new List<float>();
            data.Add(input.Position.x / input.WorldSize.x);
            data.Add(input.Position.y / input.WorldSize.y);
            return data.ToArray();
        }

        public OutputData Decide(InputData input)
        {
            // TODO
            return new OutputData();
        }
    }
}
