using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLTD.ML
{
    public class Decision
    {
        // Get the size of a request to the neural network
        public static int GetFloatArraySize(AISettings settings, int nbMessages, int msgSize)
        {
            InputData data = new InputData();

            data.WorldSize = Vector2.one;
            data.Position = Vector2.one;
            data.Direction = 1f;
            data.Speed = 1f;
            data.RaycastInfos = new Tuple<RaycastOutput, float>[settings.VisionAngles.Length];
            for (int i = 0; i < settings.VisionAngles.Length; i++)
                data.RaycastInfos[i] = new Tuple<RaycastOutput, float>(RaycastOutput.NONE, 0f);
            data.RaycastMaxSize = settings.VisionAngles.Length;
            data.CanUseSkill = false;
            data.SkillTimer = 0f;
            data.SkillTimerMaxDuration = 0f;

            if (settings.EnableLeadership)
            {
                data.Messages = new bool[nbMessages][];
                for (int i = 0; i < nbMessages; i++)
                {
                    data.Messages[i] = new bool[msgSize];
                }
                data.LeaderPosition = Vector2.one;
            }

            if (settings.EnableMemory)
            {
                data.Memory = new float[settings.MemorySize];
            }

            return InputToFloatArray(settings, data).Length;
        }

        // Input structure to float array for the neural network
        public static float[] InputToFloatArray(AISettings settings, InputData input)
        {
            List<float> data = new List<float>();
            var myPos = new Vector2(input.Position.x / input.WorldSize.x, input.Position.y / input.WorldSize.y);
            var leaderPos = new Vector2(input.LeaderPosition.x / input.WorldSize.x, input.LeaderPosition.y / input.WorldSize.y);
            data.Add(myPos.x);
            data.Add(myPos.y);
            data.Add(input.Direction);
            data.Add(input.Speed);
            foreach (var elem in input.RaycastInfos)
            {
                var max = Enum.GetValues(typeof(RaycastOutput)).Cast<int>().Max() + 1;
                List<float> elems = new List<float>();
                for (int i = 0; i < max; i++) elems.Add(0f);
                elems[(int)elem.Item1] = elem.Item2 / input.RaycastMaxSize;
                data.AddRange(elems);
            }
            data.Add(input.CanUseSkill ? 1f : 0f);
            data.Add(input.SkillTimer / input.SkillTimerMaxDuration);

            if (settings.EnableLeadership)
            {
                foreach (var msg in input.Messages)
                {
                    List<float> m = new List<float>();
                    for (int i = 0; i < msg.Length; i++) m.Add(msg[i] ? 1f : 0f);
                    data.AddRange(m);
                }
                data.Add(leaderPos.x - myPos.x);
                data.Add(leaderPos.y - myPos.y);
            }

            if (settings.EnableMemory)
            {
                foreach (var elem in input.Memory)
                {
                    data.Add(elem);
                }
            }
            return data.ToArray();
        }

        // Neural network float array to output structure
        public static OutputData FloatArrayToOutput(AISettings settings, float[] output)
        {
            var o = new OutputData();
            o.Direction = output[0];
            o.Speed = output[1];
            o.SkillState = output[2] > .5f;
            List<bool> msg = new List<bool>();
            for (int i = 3; i < 13; i++) msg.Add(output[i] < .5f);
            o.Message = msg.ToArray();
            if (settings.EnableMemory)
            {
                List<float> memory = new List<float>();
                for (int i = 13; i < 13 + settings.MemorySize; i++) memory.Add(output[i]);
            }
            return o;
        }

        // Call to the neural network
        public static float[] Decide(AISettings settings, InputData input, NN neuralNet)
        {
            var iFloat = InputToFloatArray(settings, input);

            return neuralNet.Forward(iFloat);
        }
    }
}
