using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLTD.ML {

    public class GeneticAlgorithm {

        // generate from a pool of NN
        public static List<NN> GeneratePool(List<(NN network, float score)> agents, int ngenerated){
            List<NN> nns = new List<NN>(ngenerated);

            // We sort by score

            agents.Sort(delegate
            ((NN network, float score) a, (NN network, float score) b){
                return a.score.CompareTo(b.score);
            });

            // 80 % of ngenerated will be made with best choice
            int best = (int)(ngenerated * 0.8);

            // 20 % will be at random with large noise


            return nns;
        }
    }



}
