using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime;
using System.Runtime.Intrinsics.X86;

namespace TestSIMD {
    public class Program {

        private const int VecSize = 999;//999;
        private const int Iterations = 999999;//9;
        public static void Main(string[] args) {
            new Program().Run();
        }

        public void Run() {
            bool enabled = Vector.IsHardwareAccelerated;
            bool serverGC = GCSettings.IsServerGC;
            bool sse2Available = Sse2.IsSupported;
            bool avx2Available = Avx2.IsSupported;

            ConsoleColor baseColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("CPU " + (enabled? "supports": "does not support") + " vectorization");
            if (enabled) {
                int lanes = Vector<double>.Count;
                Console.WriteLine($"With {lanes} paralell double registers(lanes)");
                lanes = Vector<float>.Count;
                Console.WriteLine($"With {lanes} paralell float registers(lanes)");
                Console.WriteLine($"SSE2: {(sse2Available?"available":"not available")}, AVX2: {(avx2Available?"available":"not available")}");
            }
            Console.WriteLine($"GC mode: {(serverGC?"server":"workstation")}");

            PrintDescription("RES=SUM(ARR*CONST) (DOUBLE)", baseColor);
            TestSumSpeed(DoubleVecByConstantMult.SimdExplicitSum, GetDoubleArray(VecSize));
            TestSumSpeed(DoubleVecByConstantMult.NaiveForEachSum, GetDoubleArray(VecSize));
            TestSumSpeed(DoubleVecByConstantMult.NaiveForSum, GetDoubleArray(VecSize));
            TestSumSpeed(DoubleVecByConstantMult.UnsafeNaiveForSum, GetDoubleArray(VecSize));
            TestSumSpeed(DoubleVecByConstantMult.LinqSum, GetDoubleArray(VecSize));
            TestSumSpeed(DoubleVecByConstantMult.LinqAggr, GetDoubleArray(VecSize));

            PrintDescription("RES=SUM(ARR)=ARR*ARR (DOUBLE)", baseColor);
            TestArrayMultSpeed(DoubleVecByVecMult.SimdExplicitVecMult, GetDoubleArray(VecSize), GetConstantDoubleArray(VecSize));
            TestArrayMultSpeed(DoubleVecByVecMult.SimdVecMult2, GetDoubleArray(VecSize), GetConstantDoubleArray(VecSize));
            TestArrayMultSpeed(DoubleVecByVecMult.NaiveVecByVecMult, GetDoubleArray(VecSize), GetConstantDoubleArray(VecSize));
            TestArrayMultSpeed(DoubleVecByVecMult.UnsafeNaiveVecByVecMult, GetDoubleArray(VecSize), GetConstantDoubleArray(VecSize));

            PrintDescription("RES=SUM(ARR*ARR) (DOUBLE)", baseColor);
            TestArraySumMultSpeed(DoubleVecByVecSumMult.SimdExplicitSumVecByVecMult, GetDoubleArray(VecSize), GetConstantDoubleArray(VecSize));
            TestArraySumMultSpeed(DoubleVecByVecSumMult.NaiveSumVecByVecMult, GetDoubleArray(VecSize), GetConstantDoubleArray(VecSize));

            PrintDescription("RES=SUM(ARR*CONST) (FLOAT)", baseColor);
            TestSumSpeed(FloatVecByConstantMult.SimdExplicitFloatSum, GetFloatArray(VecSize));
            if (avx2Available) {TestSumSpeed(FloatVecByConstantMult.SimdExplicitFloatSumAvx2, GetFloatArray(VecSize));}
            TestSumSpeed(FloatVecByConstantMult.NaiveForEachFloatSum, GetFloatArray(VecSize));
            TestSumSpeed(FloatVecByConstantMult.NaiveForFloatSum, GetFloatArray(VecSize));
            TestSumSpeed(FloatVecByConstantMult.UnsafeNaiveForFloatSum, GetFloatArray(VecSize));
            TestSumSpeed(FloatVecByConstantMult.LinqFloatSum, GetFloatArray(VecSize));
            TestSumSpeed(FloatVecByConstantMult.LinqFloatAggr, GetFloatArray(VecSize));

            PrintDescription("RES=SUM(ARR*ARR) (FLOAT)", baseColor);
            TestArraySumMultSpeed(FloatVecByVecSumMult.SimdExplicitSumVecMult, GetFloatArray(VecSize), GetConstantFloatArray(VecSize));
            TestArraySumMultSpeed(FloatVecByVecSumMult.NaiveVecByVecFloatMult, GetFloatArray(VecSize), GetConstantFloatArray(VecSize));
            if (avx2Available) { TestArraySumMultSpeed(FloatVecByVecSumMult.SimdExplicitSumVecMultAvx2, GetFloatArray(VecSize), GetConstantFloatArray(VecSize)); }
        }

        private static void PrintDescription(string desc, ConsoleColor baseColor) {
            Console.ForegroundColor = ConsoleColor.Blue;
            string rightPadding = "".PadRight(50 - desc.Length, '-');
            Console.WriteLine($"--------{desc}{rightPadding}");
            Console.ForegroundColor = baseColor;
        }

        private void TestSumSpeed(Func<double[], double> func, double[] arr) {
            string funcMethodName = func.Method.Name;
            System.GC.Collect();
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            double res = 0;
            for (int i = 0; i < Iterations; i++) {
                res = func(arr);
            }
            sw.Stop();
            Console.WriteLine($"{funcMethodName.PadRight(30)} {(sw.ElapsedMilliseconds+"ms").PadRight(10)} {res}");
        }

        private void TestSumSpeed(Func<float[], float> func, float[] arr) {
            string funcMethodName = func.Method.Name;
            System.GC.Collect();
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            double res = 0;
            for (int i = 0; i < Iterations; i++) {
                res = func(arr);
            }
            sw.Stop();
            Console.WriteLine($"{funcMethodName.PadRight(30)} {(sw.ElapsedMilliseconds+"ms").PadRight(10)} {res}");
        }

        private void TestArrayMultSpeed(Func<double[], double[], double[]> func, double[] arr, double[] arr2) {
            string funcMethodName = func.Method.Name;
            System.GC.Collect();
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            double[] vec = new double[0];
            for (int i = 0; i < Iterations; i++) {
                vec = func(arr, arr2);
            }
            sw.Stop();
            double res = vec.Sum();
            Console.WriteLine($"{funcMethodName.PadRight(30)} {(sw.ElapsedMilliseconds+"ms").PadRight(10)} {res}");
        }

        private void TestArraySumMultSpeed(Func<double[], double[], double> func, double[] arr, double[] arr2) {
            string funcMethodName = func.Method.Name;
            System.GC.Collect();
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            double res = 0;
            for (int i = 0; i < Iterations; i++) {
                res = func(arr, arr2);
            }
            sw.Stop();
            Console.WriteLine($"{funcMethodName.PadRight(30)} {(sw.ElapsedMilliseconds+"ms").PadRight(10)} {res}");
        }

        private void TestArraySumMultSpeed(Func<float[], float[], float> func, float[] arr, float[] arr2) {
            string funcMethodName = func.Method.Name;
            System.GC.Collect();
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            float res = 0.0F;
            for (int i = 0; i < Iterations; i++) {
                res = func(arr, arr2);
            }
            sw.Stop();
            Console.WriteLine($"{funcMethodName.PadRight(30)} {(sw.ElapsedMilliseconds+"ms").PadRight(10)} {res}");
        }

        private static double[] GetDoubleArray(int len) {
            double[] arr = new double[len];
            int rem = 0;
            for (int i = 1; i < len + 1; i++) {
                arr[i - 1] = (double) i - rem;
                if (i % 10 == 0) {
                    rem = i;
                }
            }
            return arr;
        }

        private static float[] GetFloatArray(int len) {
            float[] arr = new float[len];
            int rem = 0;
            for (int i = 1; i < len + 1; i++) {
                arr[i - 1] = (float) i - rem;
                if (i % 10 == 0) {
                    rem = i;
                }
            }
            return arr;
        }

        private static double[] GetConstantDoubleArray(int len) {
            double[] arr = new double[len];
            for (int i = 0; i < len; i++) {
                arr[i] = DoubleVecByConstantMult.Constant;
            }
            return arr;
        }

        private static float[] GetConstantFloatArray(int len) {
            float[] arr = new float[len];
            for (int i = 0; i < len; i++) {
                arr[i] = FloatVecByConstantMult.Constant;
            }
            return arr;
        }
    }
}