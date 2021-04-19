﻿using System;
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
            data.Messages = new bool[nbMessages][];
            for (int i = 0; i < nbMessages; i++)
            {
                data.Messages[i] = new bool[msgSize];
            }
            data.CanUseSkill = false;
            data.SkillTimer = 0f;
            data.SkillTimerMaxDuration = 0f;
            data.LeaderPosition = Vector2.one;

            if (settings.EnableMemory)
            {
                data.Memory = new Tuple<RaycastOutput, Vector2>[settings.MemorySize];
                for (int i = 0; i < settings.MemorySize; i++)
                    data.Memory[i] = new Tuple<RaycastOutput, Vector2>(RaycastOutput.NONE, Vector2.zero);
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
            foreach (var msg in input.Messages)
            {
                List<float> m = new List<float>();
                for (int i = 0; i < msg.Length; i++) m.Add(msg[i] ? 1f : 0f);
                data.AddRange(m);
            }
            data.Add(input.CanUseSkill ? 1f : 0f);
            data.Add(input.SkillTimer / input.SkillTimerMaxDuration);
            data.Add(leaderPos.x - myPos.x);
            data.Add(leaderPos.y - myPos.y);
            if (settings.EnableMemory)
            {
                foreach (var elem in input.Memory)
                {
                    var max = Enum.GetValues(typeof(RaycastOutput)).Cast<int>().Max() + 1;
                    List<float> elems = new List<float>();
                    for (int i = 0; i < max * 2; i++) elems.Add(0f);
                    elems[(int)elem.Item1 * 2] = elem.Item2.x;
                    elems[(int)elem.Item1 * 2 + 1] = elem.Item2.y;
                    data.AddRange(elems);
                }
            }
            return data.ToArray();
        }

        // Neural network float array to output structure
        private static OutputData FloatArrayToOutput(float[] output)
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

        // Call to the neural network
        public static OutputData Decide(AISettings settings, InputData input, NN neuralNet)
        {
            var iFloat = InputToFloatArray(settings, input);

            return FloatArrayToOutput(neuralNet.Forward(iFloat));
        }
    }
}
