
namespace MLTD.ML{

    public abstract class ADataType {
        public int size = 1;
        public abstract Numeric.Vector Forward(Numeric.Vector data);
    }

    public class Real : ADataType {
        public override Numeric.Vector Forward(Numeric.Vector data){
            return data;
        }
    }

    public class RealPositive : ADataType {
        public override Numeric.Vector Forward(Numeric.Vector data) {
            for(int i = 0; i < data.size; ++i) {
                var tmp = data.At(i);
                data.At(i,  tmp > 0f ? tmp : 0f);
            }
            return data;
        }
    }

    public class RealNegative : ADataType {
        public override Numeric.Vector Forward(Numeric.Vector data) {
            for(int i = 0; i < data.size; ++i) {
                var tmp = data.At(i);
                data.At(i,  tmp < 0f? tmp : 0f);
            }
            return data;
        }
    }

    public class Range : ADataType {
        public float min;
        public float max;
        public Range(float min, float max) {
            this.min = min;
            this.max = max;
        }
        public override Numeric.Vector Forward(Numeric.Vector data){
            return Numeric.Sigmoid(data, this.min, this.max);
        }
    }

    public class Boolean : ADataType {
        public override Numeric.Vector Forward(Numeric.Vector data){
            for(int i = 0; i < data.size; ++i) {
                data.At(i,  data.At(i) < 0f ? 0f : 1f);
            }
            return data;
        }
    }

    public class Qualitative : ADataType {
        public Qualitative(int nClasses){
            this.size = nClasses;
        }
        public override Numeric.Vector Forward(Numeric.Vector data){
            return Numeric.SoftMax(data);
        }
    }

}
