using System.Collections.Generic;
using System.Linq;


namespace MLTD.ML
{

    public class GeneticAlgorithm
    {

        public static NN GenerateFromNNPair(NN a, NN b, float scale){
            float alpha = UnityEngine.Random.Range(0f, 1f);

            NN newNN1 = new NN(a);
            NN newNN2 = new NN(b);

            for (int w = 0; w < newNN1.weights.Count; ++w)
            {
                // weights = alpha * w1 + (1 - alpha) * w2
                newNN1.weights[w].Multiply(alpha).Add(newNN2.weights[w].Multiply(1f - alpha));

                // we reuse w2 for random noise
                newNN2.weights[w].Randomize();
                newNN2.weights[w].ScaleByLine();
                newNN2.weights[w].Multiply(scale);

                // w1 += small random noise
                newNN1.weights[w].Add(newNN2.weights[w]);

                newNN1.weights[w].ScaleByLine();
            }

            return newNN1;
        }

        public static List<NN> GenerateFromOneList(List<(NN network, float score)> lNN, int ngen, float scale) {
            List<NN> result = new List<NN>(ngen);

            for (int i = 0; i < ngen; ++i)
            {
                int index1 = (int)UnityEngine.Random.Range(0, lNN.Count);
                int index2 = (int)UnityEngine.Random.Range(0, lNN.Count - 1);
                if (index2 == index1)
                {
                    index2++;
                }

                result.Add(GenerateFromNNPair(lNN[index1].network, lNN[index2].network, scale));
            }
            return result;
        }

        public static List<NN> SelectFromOneList(List<(NN network, float score)> lNN, int ngen) {
            List<NN> result = new List<NN>(ngen);

            for (int i = 0; i < ngen; ++i)
            {
                int index = (int)UnityEngine.Random.Range(0, lNN.Count);
                result.Add(lNN[index].network);
            }
            return result;
        }


        public static List<NN> GenerateFromTwoList(List<(NN network, float score)> lNN1, List<(NN network, float score)> lNN2, int ngen, float scale) {
            List<NN> result = new List<NN>(ngen);

            for (int i = 0; i < ngen; ++i)
            {
                int index1 = (int)UnityEngine.Random.Range(0, lNN1.Count);
                int index2 = (int)UnityEngine.Random.Range(0, lNN2.Count);
                result.Add(GenerateFromNNPair(lNN1[index1].network, lNN2[index2].network, scale));
            }
            return result;
        }

        public static List<NN> GenerateBestFromOneList(List<(NN network, float score)> lNN, int ngen, float scale) {
            List<NN> result = new List<NN>(ngen);

            for (int i = 0; i < ngen; ++i)
            {
                NN newNN = new NN(lNN[0].network);
                NN noiseNN = NN.RandomLike(lNN[0].network);

                for(int w = 0; w < noiseNN.weights.Count; ++w){
                    noiseNN.weights[w].ScaleByLine();
                    noiseNN.weights[w].Multiply(scale);
                    newNN.weights[w].Add(noiseNN.weights[w]);

                    newNN.weights[w].ScaleByLine();
                }


                result.Add(newNN);
            }

            return result;
        }

        // generate from a pool of NN
        public static List<NN> GeneratePool(List<(NN network, float score)> agents_LG, List<(NN network, float score)> agents_BoA, int ngenerated)
        {

            List<NN> nns = new List<NN>(ngenerated);
            // int top = 10;

            // float smallscale = 0.05f;

            // top = agents_LG.Count > top ? top : agents_LG.Count - 1;

            // // We sort by score
            // agents_LG.Sort(delegate
            // ((NN network, float score) a, (NN network, float score) b)
            // {
            //     return b.score.CompareTo(a.score);
            // });

            // List<(NN network, float score)> tops = agents_LG.Take(top).ToList();


            // 20 % of ngenerated will be made with best choice
            int n80percent = (int)(ngenerated * 0.8);

            // nns.AddRange(SelectFromOneList(agents_BoA, n20percent));
            // nns.AddRange(SelectFromOneList(tops, n20percent));
            // nns.AddRange(GenerateFromOneList(tops, n20percent, smallscale));
            // nns.AddRange(GenerateFromTwoList(agents_BoA, tops, n20percent, smallscale));
            nns.AddRange(GenerateBestFromOneList(agents_BoA, n80percent, 0.001f));

            for(int i = 0; i < ngenerated - n80percent; ++i){
                nns.Add(NN.RandomLike(agents_BoA[0].network));
            }

            return nns;
        }
    }



}
