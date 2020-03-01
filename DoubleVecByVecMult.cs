using System;
using System.Numerics;

namespace TestSIMD {
    public static class DoubleVecByVecMult {
        public static double[] NaiveVecByVecMult(double[] arr, double[] arr2) {
            double[] res = new double[arr.Length];
            for (int i = 0; i < arr.Length; i++) {
                res[i] = arr[i] * arr2[i];
            }
            return res;
        }

        public unsafe static double[] UnsafeNaiveVecByVecMult(double[] arr, double[] arr2) {
            int len = arr.Length;
            double[] res = new double[len];
            fixed(double * rbase = res, abase = arr, a2base = arr2) {
                for (int i = 0; i < len; i++) {
                    rbase[i] = abase[i] * a2base[i];
                }
            }
            return res;
        }

        public static double[] SimdVecMult2(double[] lhs, double[] rhs) {
            var lanes = Vector<double>.Count;
            var res = new double[lhs.Length];
            var i = 0;
            for (i = 0; i <= lhs.Length - lanes; i += lanes) {
                var va = new Vector<double>(lhs, i);
                var vb = new Vector<double>(rhs, i);
                (va * vb).CopyTo(res, i);
            }

            for (; i < lhs.Length; ++i) {
                res[i] = lhs[i] * rhs[i];
            }

            return res;
        }

        public static double[] SimdExplicitVecMult(double[] arr, double[] arr2) {
            int len = arr.Length;
            int lanes = Vector<double>.Count;
            int remain = len % lanes;
            double[] res = new double[arr.Length];

            if (len >= lanes) {
                int i = 0;
                while (i < len - remain) {
                    Vector<double> va = new Vector<double>(arr, i);
                    Vector<double> va2 = new Vector<double>(arr2, i);
                    va = va * va2;
                    va.CopyTo(res, i);
                    i += lanes;
                }
            }

            for (int i = (len - remain); i < len; i++) {
                res[i] = arr[i] * arr2[i];
            }
            return res;
        }
    }
}