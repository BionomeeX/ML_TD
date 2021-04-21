using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MLTD.ML
{
    public class NN
    {
        public int inputSize;
        public List<ADataType> outputs;
        public List<int> hiddenLayers;
        public List<Numeric.Matrix> weights;

        public bool Save(string filename)
        {
            using FileStream file = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);
            using BinaryWriter writer = new BinaryWriter(file);

            writer.Write(this.inputSize);

            writer.Write(hiddenLayers.Count);
            for (int i = 0; i < hiddenLayers.Count; ++i)
            {
                writer.Write(hiddenLayers[i]);
            }

            writer.Write(outputs.Count);
            for (int i = 0; i < outputs.Count; ++i)
            {
                outputs[i].Write(writer);
            }

            writer.Write(weights.Count);
            for (int i = 0; i < weights.Count; ++i)
            {
                weights[i].Write(writer);
            }

            return true;
        }

        public static NN read(string filename)
        {
            using FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new BinaryReader(file);

            int inputsize = reader.ReadInt32();

            int hlcount = reader.ReadInt32();
            List<int> hiddenlayers = new List<int>(hlcount);
            for (int i = 0; i < hlcount; ++i)
            {
                hiddenlayers.Add(reader.ReadInt32());
            }

            int outputcount = reader.ReadInt32();
            List<ADataType> outputs = new List<ADataType>(outputcount);
            for (int i = 0; i < outputcount; ++i)
            {
                outputs.Add(DataTypeFactory.Read(reader));
            }

            int weightscount = reader.ReadInt32();
            List<Numeric.Matrix> weigths = new List<Numeric.Matrix>(weightscount);
            for (int i = 0; i < weightscount; ++i)
            {
                weigths.Add(Numeric.Matrix.Read(reader));
            }

            NN nn = new NN(inputsize, outputs, hiddenlayers);
            nn.weights = weigths;

            return nn;
        }

        public NN(int inputSize, List<ADataType> outputs, List<int> hiddenLayers)
        {
            this.inputSize = inputSize;
            this.outputs = outputs;
            this.hiddenLayers = hiddenLayers;

            this.weights = new List<Numeric.Matrix>(this.hiddenLayers.Count + outputs.Count);

            this.weights.Add(new Numeric.Matrix(this.hiddenLayers[0], this.inputSize + 1));

            for (int i = 0; i < this.hiddenLayers.Count - 1; ++i)
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

        }

        public NN(NN other)
        {
            this.inputSize = other.inputSize;
            this.outputs = new List<ADataType>(other.outputs);
            this.hiddenLayers = new List<int>(other.hiddenLayers);
            this.weights = new List<Numeric.Matrix>(other.weights);
        }

        public static NN RandomLike(NN other)
        {
            NN newNN = new NN(other);
            foreach (var w in newNN.weights)
            {
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

        public void BackPropagate(List<float[]> data)
        {
            // for each weight we initialize a zero gradient

            //for each element of the input data
            //  we go Forward, but we keep the intermediates outputs

            //

        }

        public List<Numeric.Vector> ForwardTraining(float[] data)
        {
            List<Numeric.Vector> result = new List<Numeric.Vector>(1 + hiddenLayers.Count * 2 + outputs.Count * 2);

            Numeric.Vector input = new Numeric.Vector(data.Length);
            input.data = data;

            result.Add(input);

            for (int i = 0; i < hiddenLayers.Count; ++i)
            {
                result.Add(Numeric.MatMultBias(weights[i], result[result.Count - 1]));
                result.Add(Numeric.Sigmoid(result[result.Count - 1]));
            }

            int idx = result.Count - 1;

            for(int i = 0; i < outputs.Count; ++i){
                result.Add(
                    Numeric.MatMultBias(weights[hiddenLayers.Count + i], result[idx])
                );
                result.Add(
                    outputs[i].Forward(result[result.Count - 1])
                );
            }

            result.RemoveAt(0);

            return result;
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
