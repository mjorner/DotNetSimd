using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace TestSIMD {
    public static class DoubleVecByConstantMult {
        public static readonly double Constant = Math.PI;
        private const MethodImplOptions MaxOpt =
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;

        [MethodImpl(MaxOpt)]
        public static double LinqSum(double[] arr) {
            return arr.Sum(x => x * Constant);
        }

        [MethodImpl(MaxOpt)]
        public static double LinqAggr(double[] arr) {
            return arr.Aggregate(0.0, (acc, x) => acc + x * Constant);
        }

        [MethodImpl(MaxOpt)]
        public static double NaiveForEachSum(double[] arr) {
            double sum = 0.0;
            foreach (double d in arr) {
                sum += d * Constant;
            }
            return sum;
        }

        [MethodImpl(MaxOpt)]
        public static double NaiveForSum(double[] arr) {
            double sum = 0.0;
            for (int i = 0; i < arr.Length; i++) {
                sum += arr[i] * Constant;
            }
            return sum;
        }

        [MethodImpl(MaxOpt)]
        public unsafe static double UnsafeNaiveForSum(double[] arr) {
            double sum = 0.0;
            int len = arr.Length;
            fixed(double * arrbase = arr) {
                for (int i = 0; i < len; i++) {
                    sum += arrbase[i] * Constant;
                }
            }
            return sum;
        }

        [MethodImpl(MaxOpt)]
        public static double SimdExplicitSum(double[] arr) {
            int len = arr.Length;
            int lanes = Vector<double>.Count;
            int remain = len % lanes;

            Vector<double> vsum = Vector<double>.Zero;

            for (int i = 0; i < len - remain; i += Vector<double>.Count) {
                var value = new Vector<double>(arr, i);
                vsum += value * Constant;
            }

            double sum = 0.0;
            for (int i = 0; i < lanes; i++) {
                sum += vsum[i];
            }
            for (int i = (len - remain); i < len; i++) {
                sum += arr[i] * Constant;
            }
            return sum;
        }
    }
}