using System;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace TestSIMD {
    public static class FloatVecByConstantMult {
        public static readonly float Constant = (float)Math.PI;

        public static float LinqFloatSum(float[] arr) {
            return arr.Sum(x => x * Constant);
        }

        public static float LinqFloatAggr(float[] arr) {
            return arr.Aggregate(0.0F, (acc, x) => acc + x * Constant);
        }

        public static float NaiveForEachFloatSum(float[] arr) {
            float sum = 0.0F;
            foreach (float d in arr) {
                sum += d * Constant;
            }
            return sum;
        }

        public static float NaiveForFloatSum(float[] arr) {
            float sum = 0.0F;
            for (int i = 0; i < arr.Length; i++) {
                sum += arr[i] * Constant;
            }
            return sum;
        }

        public unsafe static float UnsafeNaiveForFloatSum(float[] arr) {
            float sum = 0.0F;
            int len = arr.Length;
            fixed(float * arrbase = arr) {
                for (int i = 0; i < len; i++) {
                    sum += arrbase[i] * Constant;
                }
            }
            return sum;
        }

        private static float SumSpan(ReadOnlySpan<float> span) {
            float sum = 0.0F;
            foreach (float value in span) {
                sum += value * Constant;
            }
            return sum;
        }

        public static float SimdExplicitFloatSum(float[] arr) {
            int len = arr.Length;
            int lanes = Vector<float>.Count;
            int remain = len % lanes;

            Vector<float> vsum = Vector<float>.Zero;

            for (int i = 0; i < len - remain; i += Vector<float>.Count) {
                var value = new Vector<float>(arr, i);
                vsum += value * Constant;
            }

            float sum = 0;
            for (int i = 0; i < lanes; i++) {
                sum += vsum[i];
            }
            float remainder = SumSpan(new ReadOnlySpan<float>(arr, (len - remain), remain));
            return sum + remainder;
        }

        public static unsafe float SimdExplicitFloatSumAvx2(float[] source) {
            float result;
            int lanes = Vector256<float>.Count;

            fixed(float * pSource = source) {
                Vector256<float> vresult = Vector256<float>.Zero;

                int i = 0;
                int lastBlockIndex = source.Length - (source.Length % lanes);

                while (i < lastBlockIndex) {
                    Vector256<float> vv = Avx2.LoadVector256(pSource + i);
                    vv = Avx2.Multiply(vv, Vector256.Create(FloatVecByConstantMult.Constant));
                    vresult = Avx2.Add(vresult, vv);
                    i += lanes;
                }

                result = 0.0F;
                float* temp = stackalloc float[lanes];
                Avx2.Store(temp, vresult);
                for (int j = 0; j < lanes; j++) {
                    result += temp[j];
                }

                while (i < source.Length) {
                    result += pSource[i] * FloatVecByConstantMult.Constant;
                    i += 1;
                }
            }
            return result;
        }
    }
}