using System;
using System.Collections.Generic;
using System.Linq;

namespace MLTD.ML
{
    public class Decision
    {
        private float[] InputToFloatArray(InputData input)
        {
            List<float> data = new List<float>();
            data.Add(input.Position.x / input.WorldSize.x);
            data.Add(input.Position.y / input.WorldSize.y);
            data.Add(input.Direction);
            data.Add(input.Speed);
            foreach (var elem in input.RaycastInfos)
            {
                var max = Enum.GetValues(typeof(RaycastOutput)).Cast<int>().Max() + 1;
                List<float> elems = new List<float>();
                for (int i = 0; i < max; i++) elems.Add(0f);
                elems[(int)elem.Item1] = elem.Item2 / input.RaycastMaxSize;
            }
            foreach (var msg in input.Messages)
            {
                List<float> m = new List<float>();
                for (int i = 0; i < msg.Length; i++) m.Add(msg[i] ? 1f : 0f);
            }
            data.Add(input.CanUseSkill ? 1f : 0f);
            data.Add(input.SkillTimer / input.SkillTimerMaxDuration);
            return data.ToArray();
        }

        private OutputData FloatArrayToOutput(float[] output)
        {
            var o = new OutputData();
            o.Direction = output[0];
            o.Speed = output[1];
            o.SkillState = output[2] > .5f;
            List<bool> msg = new List<bool>();
            for (int i = 3; i < output.Length; i++) msg.Add(output[i] < .5f);
            o.Message = msg.ToArray();
            return o;
        }

        public OutputData Decide(InputData input)
        {
            var iFloat = InputToFloatArray(input);
            // TODO
            return FloatArrayToOutput(iFloat);
        }
    }
}
