using UnityEngine;

namespace MLTD.ML
{

    public class Numeric
    {
        public class Vector
        {
            public float[] data;
            public int size;

            public Vector(int size)
            {
                this.size = size;
                this.data = new float[size];
                // for(int i = 0; i <size; ++i){
                //     this.data[i] = 0f;
                // }
            }

            public float At(int index)
            {
                return this.data[index];
            }

            public void At(int index, float value)
            {
                this.data[index] = value;
            }

            public void UpdateAt(int index, float value)
            {
                this.data[index] += value;
            }

        }

        public class Matrix
        {
            public float[] data;
            public int nline;
            public int ncolumn;

            public Matrix(int nline, int ncol)
            {
                this.nline = nline;
                this.ncolumn = ncol;
                data = new float[nline * ncol];
            }

            public float At(int indexLine, int indexColumn)
            {
                // if(float.IsNaN(data[indexLine * ncolumn + indexColumn])) {
                //     Debug.Log("?? : " + data[indexLine * ncolumn + indexColumn]);
                // }
                return data[indexLine * ncolumn + indexColumn];
            }

            public void At(int indexLine, int indexColumn, float value)
            {
                data[indexLine * ncolumn + indexColumn] = value;
            }
            public void UpdateAt(int indexLine, int indexColumn, float value)
            {
                data[indexLine * ncolumn + indexColumn] += value;
            }

            public void Randomize()
            {
                float limit = Mathf.Sqrt(6f / (this.ncolumn + this.nline));
                // Debug.Log("limit : " + limit);
                for (int i = 0; i < nline * ncolumn; ++i)
                {
                    // float val = RandomGaussian(-limit, +limit);
                    float val = Random.Range(-limit, +limit);
                    this.data[i] =  val;
                    // Debug.Log("Random val : " + val);
                }
            }

            public Matrix Multiply(float value) {
                for (int i = 0; i < nline * ncolumn; ++i)
                {
                    this.data[i] *= value;
                }
                return this;
            }

            public Matrix Add(Matrix m) {
                for (int i = 0; i < nline * ncolumn; ++i)
                {
                    this.data[i] += m.data[i];
                }
                return this;
            }

        }

        public static Matrix MatMult(Matrix m1, Matrix m2)
        {
            Matrix result = new Matrix(m1.nline, m2.ncolumn);
            for (int line = 0; line < m1.nline; ++line)
            {
                for (int col = 0; col < m2.ncolumn; ++col)
                {
                    for (int k = 0; k < m1.ncolumn; ++k)
                    {
                        result.UpdateAt(line, col, m1.At(line, k) * m2.At(k, col));
                    }
                }
            }
            return result;
        }

        public static Vector MatMult(Matrix m, Vector v)
        {
            Vector result = new Vector(m.nline);
            for (int line = 0; line < m.nline; ++line)
            {
                for (int col = 0; col < m.ncolumn; ++col)
                {
                    //Debug.Log(m.nline + " ; " + m.ncolumn);
                    //var a = v.At(col);
                    result.UpdateAt(line, m.At(line, col) * v.At(col));
                }
            }
            return result;
        }

        public static Vector MatMultBias(Matrix m, Vector v)
            {
                Vector result = new Vector(m.nline);
                for (int line = 0; line < m.nline; ++line)
                {
                    for (int col = 0; col < m.ncolumn - 1; ++col)
                    {
                        // Debug.Log("" + m.At(line, col) + " * " + v.At(col) + " = " + m.At(line, col) * v.At(col));
                        float val = m.At(line, col) * v.At(col);
                        result.UpdateAt(line, val);
                    }
                    result.UpdateAt(line, m.At(line, m.ncolumn - 1) * 1f);
                    // Debug.Log("result at " + line + " = " + result.At(line));
                }
                return result;
            }

        public static Vector SoftMax(Vector v)
        {
            Vector result = new Vector(v.size);
            float total = 0f;
            for (int i = 0; i < v.size; ++i)
            {
                float tmp = Mathf.Exp(v.At(i));
                result.At(i, tmp);
                total += tmp;
            }

            for (int i = 0; i < v.size; ++i)
            {
                result.At(i, result.At(i) / total);
            }

            return result;
        }

        public static Vector Sigmoid(Vector v, float min = 0f, float max = 1f)
        {
            Vector result = new Vector(v.size);
            for (int i = 0; i < v.size; ++i)
            {
                // Debug.Log("v at " + i + " " + v.At(i) + " -> " + Mathf.Exp(-v.At(i)) + " ( " + min + ", " + max +") => " + (min + (max - min) / (1f + Mathf.Exp(-v.At(i)))));
                result.At(i, min + (max - min) / (1f + Mathf.Exp(-v.At(i))));
            }
            return result;
        }

        public static Vector Relu(Vector v)
        {
            Vector result = new Vector(v.size);
            for (int i = 0; i < v.size; ++i)
            {
                result.At(i, v.At(i) > 0 ? v.At(i) : 0f);
            }
            return result;
        }

        public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
        {
            float u, v, S;

            do
            {
                u = 2.0f * UnityEngine.Random.value - 1.0f;
                v = 2.0f * UnityEngine.Random.value - 1.0f;
                S = u * u + v * v;
            }
            while (S >= 1.0f);

            // Standard Normal Distribution
            float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

            // Normal Distribution centered between the min and max value
            // and clamped following the "three-sigma rule"
            float mean = (minValue + maxValue) / 2.0f;
            float sigma = (maxValue - mean) / 3.0f;
            return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
        }
    }

}
