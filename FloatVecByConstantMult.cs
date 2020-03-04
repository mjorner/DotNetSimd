using System;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;

namespace TestSIMD {
    public static class FloatVecByConstantMult {
        private const MethodImplOptions MaxOpt =
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;
        public static readonly float Constant = (float) Math.PI;

        [MethodImpl(MaxOpt)]
        public static float LinqFloatSum(float[] arr) {
            return arr.Sum(x => x * Constant);
        }

        [MethodImpl(MaxOpt)]
        public static float LinqFloatAggr(float[] arr) {
            return arr.Aggregate(0.0F, (acc, x) => acc + x * Constant);
        }

        [MethodImpl(MaxOpt)]
        public static float NaiveForEachFloatSum(float[] arr) {
            float sum = 0.0F;
            foreach (float d in arr) {
                sum += d * Constant;
            }
            return sum;
        }

        [MethodImpl(MaxOpt)]
        public static float NaiveForFloatSum(float[] arr) {
            float sum = 0.0F;
            for (int i = 0; i < arr.Length; i++) {
                sum += arr[i] * Constant;
            }
            return sum;
        }

        [MethodImpl(MaxOpt)]
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

        [MethodImpl(MaxOpt)]
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
            for (int i = (len - remain); i < len; i++) {
                sum += arr[i] * Constant;
            }

            return sum;
        }

        [MethodImpl(MaxOpt)]
        public static unsafe float SimdExplicitFloatSumAvx2(float[] arr) {
            float sum;
            int lanes = Vector256<float>.Count;

            fixed(float * pArr = arr) {
                Vector256<float> vresult = Vector256<float>.Zero;

                int i = 0;
                int lastBlockIndex = arr.Length - (arr.Length % lanes);

                while (i < lastBlockIndex) {
                    Vector256<float> vv = Avx2.LoadVector256(pArr + i);
                    vv = Avx2.Multiply(vv, Vector256.Create(FloatVecByConstantMult.Constant));
                    vresult = Avx2.Add(vresult, vv);
                    i += lanes;
                }

                sum = 0.0F;
                float * temp = stackalloc float[lanes];
                Avx2.Store(temp, vresult);
                for (int j = 0; j < lanes; j++) {
                    sum += temp[j];
                }

                while (i < arr.Length) {
                    sum += pArr[i] * Constant;
                    i += 1;
                }
            }
            return sum;
        }
    }
}