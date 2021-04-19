using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLTD.ML
{
    public class NN
    {
        public int inputSize;
        public List<ADataType> outputs;
        public List<int> hiddenLayers;
        public List<Numeric.Matrix> weights;

        public NN(int inputSize, List<ADataType> outputs, List<int> hiddenLayers)
        {
            this.inputSize = inputSize;
            this.outputs = outputs;
            this.hiddenLayers = hiddenLayers;

            this.weights = new List<Numeric.Matrix>(this.hiddenLayers.Count + outputs.Count);

            this.weights.Add(new Numeric.Matrix(this.hiddenLayers[0], this.inputSize + 1));

            for (int i = 0; i < this.weights.Count - 2; ++i)
            {
                this.weights.Add(new Numeric.Matrix(this.hiddenLayers[i + 1], this.hiddenLayers[i] + 1));
            }

            for (int i = 0; i < outputs.Count; ++i)
            {
                this.weights.Add(new Numeric.Matrix(this.outputs[i].size, this.hiddenLayers[this.hiddenLayers.Count - 1] + 1));
            }

            foreach (var weight in this.weights)
            {
                weight.Randomize();
            }

            // foreach(var w in this.weights){
            //     for(int line = 0; line < w.nline; ++line){
            //         string l = "";
            //         for(int col = 0; col < w.ncolumn; ++col){
            //             l += w.At(line, col) + "  ";
            //         }
            //         Debug.Log(l);
            //     }
            //     Debug.Log("");
            // }

        }

        public NN(NN other){
            this.inputSize = other.inputSize;
            this.outputs = new List<ADataType>(other.outputs);
            this.hiddenLayers = new List<int>(other.hiddenLayers);
            this.weights = new List<Numeric.Matrix>(other.weights);
        }

        public static NN RandomLike(NN other) {
            NN newNN = new NN(other);
            foreach(var w in newNN.weights){
                w.Randomize();
                w.ScaleByLine();
            }
            return newNN;
        }

        public int OutputSize()
        {
            int size = 0;
            foreach (var dtype in outputs)
            {
                size += dtype.size;
            }
            return size;
        }

        public float[] Forward(float[] data)
        {

            // Debug.Log("NN Forward called with datasize : " + data.Length);

            Numeric.Vector result = new Numeric.Vector(data.Length);
            result.data = data;

            // string str = "";
            // for(int i=0; i <data.Length; ++i){
            //     str += data[i] + "  ";
            // }

            // Debug.Log("data : " + str);

            for (int i = 0; i < hiddenLayers.Count; ++i)
            {
                result = Numeric.Sigmoid(Numeric.MatMultBias(weights[i], result));
            }

            List<Numeric.Vector> output = new List<Numeric.Vector>();
            for (int i = 0; i < outputs.Count; ++i)
            {
                output.Add(outputs[i].Forward(Numeric.MatMultBias(weights[hiddenLayers.Count + i], result)));
            }

            float[] foutput = new float[OutputSize()];
            int count = 0;
            foreach (var datum in output)
            {
                for (int i = 0; i < datum.size; ++i)
                {
                    foutput[count] = datum.At(i);
                    ++count;
                }
            }

            // for(int i = 0; i < foutput.Length; ++i){
            //     Debug.Log("foutput " + i + " " + foutput[i]);
            // }

            return foutput;
        }
    }


}
