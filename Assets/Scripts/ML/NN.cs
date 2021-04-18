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

            this.weights.Add(new Numeric.Matrix(this.hiddenLayers[0], this.inputSize));

            for (int i = 0; i < this.weights.Count - 2; ++i)
            {
                this.weights.Add(new Numeric.Matrix(this.hiddenLayers[i + 1], this.hiddenLayers[i]));
            }

            for (int i = 0; i < outputs.Count; ++i)
            {
                this.weights.Add(new Numeric.Matrix(this.outputs[i].size, this.hiddenLayers[this.hiddenLayers.Count - 1]));
            }

            foreach (var weight in this.weights)
            {
                weight.Randomize();
            }

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

            Debug.Log("NN Forward called with : " + data[0]);

            Numeric.Vector result = new Numeric.Vector(data.Length);
            result.data = data;

            for (int i = 0; i < hiddenLayers.Count; ++i)
            {
                result = Numeric.Relu(Numeric.MatMult(weights[i], result));
            }

            List<Numeric.Vector> output = new List<Numeric.Vector>();
            for (int i = 0; i < outputs.Count; ++i)
            {
                output.Add(outputs[i].Forward(Numeric.MatMult(weights[hiddenLayers.Count + i], result)));
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

            for(int i = 0; i < foutput.Length; ++i){
                Debug.Log("foutput " + i + " " + foutput[i]);
            }

            return foutput;
        }
    }


}
