using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MLTD.ML
{

    public class Numeric{
        public struct Vector {
            public float[] data;
            public int size;

            public Vector(int size){
                this.size = size;
                data = new float[size];
            }

            public float At(int index){
                return data[index];
            }

            public void At(int index, float value){
                data[index] = value;
            }

            public void UpdateAt(int index, float value){
                data[index] += value;
            }

        }

        public class Matrix {
            public float[] data;
            public int nline;
            public int ncolumn;

            public Matrix(int nline, int ncol) {
                this.nline = nline;
                this.ncolumn = ncol;
                data = new float[nline * ncol];
            }

            public float At(int indexLine, int indexColumn) {
                return data[indexLine * ncolumn + indexColumn];
            }

            public void At(int indexLine, int indexColumn, float value) {
                data[indexLine * ncolumn + indexColumn] = value;
            }
            public void UpdateAt(int indexLine, int indexColumn, float value) {
                data[indexLine * ncolumn + indexColumn] += value;
            }
        }

        public static Matrix MatMult(Matrix m1, Matrix m2) {
            Matrix result = new Matrix(m1.nline, m2.ncolumn);
            for(int line = 0; line < m1.nline; ++line) {
                for(int col=0; col < m2.ncolumn; ++col) {
                    for(int k = 0; k < m1.ncolumn; ++k) {
                        result.UpdateAt(line, col, m1.At(line, k) * m2.At(k, col));
                    }
                }
            }
            return result;
        }

        public static Vector MatMult(Matrix m, Vector v) {
            Vector result = new Vector(m.nline);
            for(int line = 0; line < m.nline; ++line) {
                for(int col = 0; col < m.ncolumn; ++col) {
                    result.UpdateAt(line, m.At(line, col) * v.At(col));
                }
            }
            return result;
        }

        public static Vector SoftMax(Vector v){
            Vector result = new Vector(v.size);
            float total = 0f;
            for(int i = 0; i < v.size; ++i){
                float tmp = Mathf.Exp( v.At(i) );
                result.At(i, tmp);
                total += tmp;
            }

            for(int i = 0; i < v.size; ++i){
                result.At(i, result.At(i) / total);
            }

            return result;
        }

        public static Vector Relu(Vector v){
            Vector result = new Vector(v.size);
            for(int i = 0; i < v.size; ++i){
                result.At(i, v.At(i) > 0 ? v.At(i) : 0f);
            }
            return result;
        }
    }


    public class NN
    {
        private int inputSize;
        private int outputSize;
        private List<int> hiddenLayers;
        private List<Numeric.Matrix> weigths;


        public NN(int inputSize, int outputSize, List<int> hiddenLayers){

        }

        public float[] Forward(float[] data){

            foreach(var matrix in weigths){

            }
            return new float[0];
        }
    }


}