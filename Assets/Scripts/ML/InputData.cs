using System;
using UnityEngine;

namespace MLTD.ML
{
    public struct InputData
    {
        public Vector2 WorldSize;
        public Vector2 Position; // Between 0 and 1
        public float Direction; // Direction between -1 and 1
        public float Speed; // Speed between -1 and 1

        public Tuple<RaycastOutput, float>[] RaycastInfos; // For each raycast, what the AI see and its distance
        public float RaycastMaxSize;            

        public bool[][] Messages; // 5 messages that are a combinaison of 10 characters that are a message

        public bool CanUseSkill;
        public float SkillTimer;
        public float SkillTimerMaxDuration;

        public Vector2 LeaderPosition;
    }
}
