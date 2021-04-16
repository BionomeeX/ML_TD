using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLTD.ML
{
    public class NN
    {
        public int inputSize;
        public int outputSize;
        public List<int> hiddenLayers;
        public List<Numeric.Matrix> weights;

        public NN(int inputSize, int outputSize, List<int> hiddenLayers){
            this.inputSize = inputSize;
            this.outputSize = outputSize;
            this.hiddenLayers = hiddenLayers;

            this.weights = new List<Numeric.Matrix>(this.hiddenLayers.Count + 1);

            this.weights.Add(new Numeric.Matrix(this.hiddenLayers[0], this.inputSize));

            for(int i = 0; i < this.weights.Count - 2; ++i){
                this.weights.Add(new Numeric.Matrix(this.hiddenLayers[i + 1], this.hiddenLayers[i]));
            }

            this.weights.Add(new Numeric.Matrix(outputSize, this.hiddenLayers[this.hiddenLayers.Count - 1]));

            foreach(var weight in this.weights){
                weight.Randomize();
            }

        }

        public float[] Forward(float[] data){
            Numeric.Vector result = new Numeric.Vector(data.Length);
            result.data = data;
            for(int i = 0; i < weights.Count - 1; ++i){
                result = Numeric.Relu( Numeric.MatMult(weights[i], result) );
            }

            result = Numeric.MatMult(weights[weights.Count - 1], result);

            return result.data;
        }
    }


}
