using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace TestSIMD {
    public static class FloatVecByVecSumMult {
        public static float SimdExplicitSumVecMult(float[] arr, float[] arr2) {
            int len = arr.Length;
            int lanes = Vector<float>.Count;
            int remain = len % lanes;
            Vector<float> vsum = Vector<float>.Zero;
            if (len >= lanes) {
                int i = 0;
                while (i < len - remain) {
                    Vector<float> va = new Vector<float>(new ReadOnlySpan<float>(arr, i, lanes));
                    Vector<float> va2 = new Vector<float>(new ReadOnlySpan<float>(arr2, i, lanes));
                    vsum += va * va2;
                    i += lanes;
                }
            }

            float sum = 0.0F;
            for (int i = 0; i < lanes; i++) {
                sum += vsum[i];
            }
            for (int i = (len - remain); i < len; i++) {
                sum += arr[i] * arr2[i];
            }
            return sum;
        }

        public static unsafe float SimdExplicitSumVecMultAvx2(float[] source, float[] constants) {
            float result;
            int lanes = Vector256<float>.Count;

            fixed(float * pSource = source, pConstant = constants) {
                Vector256<float> vresult = Vector256<float>.Zero;

                int i = 0;
                int lastBlockIndex = source.Length - (source.Length % lanes);

                while (i < lastBlockIndex) {
                    Vector256<float> vv = Avx2.LoadVector256(pSource + i);
                    Vector256<float> vv2 = Avx2.LoadVector256(pConstant + i);
                    vv = Avx2.Multiply(vv, vv2);
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

        public static float BareMetalSumVecMult(float[] arr, float[] arr2) {
            float sum = 0.0F;
            for (int i = 0; i < arr.Length; i++) {
                sum += arr[i] * arr2[i];
            }
            return sum;
        }
    }
}