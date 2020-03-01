using System;
using System.Numerics;

namespace TestSIMD {
    public class DoubleVecByVecSumMult {
        public static double SimdExplicitSumVecByVecMult(double[] arr, double[] arr2) {
            int len = arr.Length;
            int lanes = Vector<double>.Count;
            int remain = len % lanes;
            Vector<double> vsum = Vector<double>.Zero;
            if (len >= lanes) {
                int i = 0;
                while (i < len - remain) {
                    Vector<double> va = new Vector<double>(new ReadOnlySpan<double>(arr, i, lanes));
                    Vector<double> va2 = new Vector<double>(new ReadOnlySpan<double>(arr2, i, lanes));
                    vsum += va * va2;
                    i += lanes;
                }
            }

            double sum = 0;
            for (int i = 0; i < lanes; i++) {
                sum += vsum[i];
            }
            for (int i = (len - remain); i < len; i++) {
                sum += arr[i] * arr2[i];
            }
            return sum;
        }

        public static double NaiveSumVecByVecMult(double[] arr, double[] arr2) {
            double sum = 0.0;
            for (int i = 0; i < arr.Length; i++) {
                sum += arr[i] * arr2[i];
            }
            return sum;
        }
    }
}