using System.Collections.Generic;

namespace MLTD.ML
{

    public class GeneticAlgorithm
    {

        // generate from a pool of NN
        public static List<NN> GeneratePool(List<(NN network, float score)> agents, int ngenerated)
        {

            List<NN> nns = new List<NN>(ngenerated);
            int top = 10;

            float smallscale = 0.05f;
            float highscale = 0.5f;

            top = agents.Count > top ? top : agents.Count - 1;

            // We sort by score

            agents.Sort(delegate
            ((NN network, float score) a, (NN network, float score) b)
            {
                return b.score.CompareTo(a.score);
            });

            // 80 % of ngenerated will be made with best choice
            int nbest = (int)(ngenerated * 0.8);

            for (int i = 0; i < nbest; ++i)
            {
                // select 2 in the top ??
                int index1 = (int)UnityEngine.Random.Range(0, top + 1);
                int index2 = (int)UnityEngine.Random.Range(0, top + 1);
                if (index2 == index1)
                {
                    index2++;
                }

                float alpha = UnityEngine.Random.Range(0f, 1f);

                NN newNN1 = new NN(agents[index1].network);
                NN newNN2 = new NN(agents[index2].network);
                for (int w = 0; w < newNN1.weights.Count; ++w)
                {
                    // weights = alpha * w1 + (1 - alpha) * w2
                    newNN1.weights[w].Multiply(alpha).Add(newNN2.weights[w].Multiply(1f - alpha));

                    // we reuse w2 for random noise
                    newNN2.weights[w].Randomize();
                    newNN2.weights[w].Multiply(smallscale);

                    // w1 += small random noise
                    newNN1.weights[w].Add(newNN2.weights[w]);

                    newNN1.weights[w].ScaleByLine();
                }

                nns.Add(newNN1);
            }


            // 20 % will be at random with large noise
            for (int i = 0; i < ngenerated - nbest; ++i)
            {
                int index1 = (int)UnityEngine.Random.Range(0, agents.Count - 1);
                int index2 = (int)UnityEngine.Random.Range(0, agents.Count);
                if (index2 == index1)
                {
                    index2++;
                }

                float alpha = UnityEngine.Random.Range(0f, 1f);

                NN newNN1 = new NN(agents[index1].network);
                NN newNN2 = new NN(agents[index2].network);
                for (int w = 0; w < newNN1.weights.Count; ++w)
                {
                    // weights = alpha * w1 + (1 - alpha) * w2
                    newNN1.weights[w].Multiply(alpha).Add(newNN2.weights[w].Multiply(1f - alpha));

                    // we reuse w2 for random noise
                    newNN2.weights[w].Randomize();
                    newNN2.weights[w].Multiply(highscale);

                    // w1 += small random noise
                    newNN1.weights[w].Add(newNN2.weights[w]);

                    newNN1.weights[w].ScaleByLine();
                }

                nns.Add(newNN1);
            }

            return nns;
        }
    }



}
