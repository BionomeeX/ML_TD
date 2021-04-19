using System.IO;
namespace MLTD.ML{

    public abstract class ADataType {
        public int size = 1;
        public abstract Numeric.Vector Forward(Numeric.Vector data);
        public abstract void Write(BinaryWriter writer);
    }

    public enum DataTypeType {
        REAL, REALPOS, REALNEG, RANGE, BOOLEAN, QUALI,
    }

    public class DataTypeFactory {
        private DataTypeFactory() {}
        public static ADataType Read(BinaryReader reader) {
            // read the type
            int dataTypeType = reader.ReadInt32();

            // read the size
            int dataTypeSize = reader.ReadInt32();

            switch((DataTypeType)dataTypeType){
                case DataTypeType.REAL:
                    return new Real();
                case DataTypeType.REALPOS:
                    return new RealPositive();
                case DataTypeType.REALNEG:
                    return new RealNegative();
                case DataTypeType.RANGE:
                    float min = reader.ReadSingle();
                    float max = reader.ReadSingle();
                    return new Range(min, max);
                case DataTypeType.BOOLEAN:
                    return new Boolean();
                case DataTypeType.QUALI:
                    return new Qualitative(dataTypeSize);
                default:
                    return new Real();
            }
        }
    }

    public class Real : ADataType {
        public override Numeric.Vector Forward(Numeric.Vector data){
            return data;
        }
        public override void Write(BinaryWriter writer){
            writer.Write((int)DataTypeType.REAL);
            writer.Write(size);
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
        public override void Write(BinaryWriter writer){
            writer.Write((int)DataTypeType.REALPOS);
            writer.Write(size);
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
        public override void Write(BinaryWriter writer){
            writer.Write((int)DataTypeType.REALNEG);
            writer.Write(size);
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
        public override void Write(BinaryWriter writer){
            writer.Write((int)DataTypeType.RANGE);
            writer.Write(size);
            writer.Write(min);
            writer.Write(max);
        }
    }

    public class Boolean : ADataType {
        public override Numeric.Vector Forward(Numeric.Vector data){
            for(int i = 0; i < data.size; ++i) {
                data.At(i,  data.At(i) < 0f ? 0f : 1f);
            }
            return data;
        }
        public override void Write(BinaryWriter writer){
            writer.Write((int)DataTypeType.BOOLEAN);
            writer.Write(size);
        }
    }

    public class Qualitative : ADataType {
        public Qualitative(int nClasses){
            this.size = nClasses;
        }
        public override Numeric.Vector Forward(Numeric.Vector data){
            return Numeric.SoftMax(data);
        }
        public override void Write(BinaryWriter writer){
            writer.Write((int)DataTypeType.QUALI);
            writer.Write(size);
        }
    }

}
