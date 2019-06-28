using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0018 // Naming Styles
    class CooknasMatrix
    {
        /// <summary>
        /// Function to measure the time needed for inverting a Matrix
        /// (for testing purposes only)
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="seed"></param>
        public static string TestCooknasMatrix(int counter, int seed) {
            Random rnd = new Random(seed);
            int n = rnd.Next(10, 1000);
            Stopwatch stopWatch = new Stopwatch();
            string ret = "";
            double[][] m = MatrixRandom(n, n, seed);
            stopWatch.Start();
            double[][] i = MatrixInverse(m);
            stopWatch.Stop();
            double[][] I = MatrixIdentity(n);
            double[][] p = MatrixProduct(m, i);
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            if (MatrixAreEqual(p, I, 1.0E-8)){
                Console.Out.WriteLine("{0}. Inverse Matrix {1}x{1} PASSED in {2}", counter, n, elapsedTime);
                ret += String.Format("{0}. Inverse Matrix {1}x{1} PASSED in {2}\n", counter, n, elapsedTime);
            }else{
                Console.Out.WriteLine("{0}. Inverse Matrix {1}x{1} FAILED*****************", counter, n);
                ret += String.Format("{0}. Inverse Matrix {1}x{1} FAILED*****************", counter, n);
            }
            return ret;
        }

        /// <summary>
        /// Creates a new matrix (as jagged array) with given
        /// Rows (rows) and Columns (cols)
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public static double[][] MatrixCreate(int rows, int cols) {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
            result[i] = new double[cols];
            return result;
        }

        /// <summary>
        /// Returns a matrix with random values
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static double[][] MatrixRandom(int rows, int cols, int seed, double minVal=Int16.MinValue, double maxVal=Int16.MaxValue ) {
            Random ran = new Random(seed);
            double[][] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = (maxVal - minVal) * ran.NextDouble() + minVal;
            return result;
        }

        /// <summary>
        /// Returns an n x n Identity matrix
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double[][] MatrixIdentity(int n) {
            double[][] result = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
                result[i][i] = 1.0;
            return result;
        }

        /// <summary>
        /// Creates a string representing the matrix
        /// </summary>
        /// <param name="matrix">The matrix to convert to string</param>
        /// <param name="dec">Decimal digits of a double in the final string</param>
        /// <returns></returns>
        public static string MatrixAsString(double[][] matrix, int dec) {
            string s = "";
            for (int i = 0; i < matrix.Length; ++i){
                for (int j = 0; j < matrix[i].Length; ++j)
                    s += matrix[i][j].ToString("F" + dec).PadLeft(8) + " ";
                s += Environment.NewLine;
            }
            return s;
        }

        /// <summary>
        /// Returns true if all values in matrixA == values in matrixB
        /// (Equals means that their difference is less than epsilon)
        /// </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool MatrixAreEqual(double[][] matrixA, double[][] matrixB, double epsilon) { 
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aRows != bRows || aCols != bCols) throw new Exception("Non-conformable matrices");
            for (int i = 0; i < aRows; ++i) // each row of A and B
                for (int j = 0; j < aCols; ++j) // each col of A and B
                    //if (matrixA[i][j] != matrixB[i][j])
                    if (Math.Abs(matrixA[i][j] - matrixB[i][j]) > epsilon)
                        return false;
            return true;
        }

        /// <summary>
        /// Multiplies two matrices given as jagged-arrays and returns the resulting
        /// matrix as another jagged-array.  
        /// </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public static double[][] MatrixProduct(double[][] matrixA, double[][] matrixB) {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aCols != bRows) throw new Exception("Non-conformable matrices in MatrixProduct");

            double[][] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k less-than bRows
                        result[i][j] += matrixA[i][k] * matrixB[k][j];
            return result;
        }

        /// <summary>
        /// result of multiplying an n x m matrix by a m x 1 
        /// column vector (yielding an n x 1 column vector)
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static double[] MatrixVectorProduct(double[][] matrix, double[] vector) {
            int mRows = matrix.Length; int mCols = matrix[0].Length;
            int vRows = vector.Length;
            if (mCols != vRows) throw new Exception("Non-conformable matrix and vector");
            double[] result = new double[mRows];
            for (int i = 0; i < mRows; ++i)
                for (int j = 0; j < mCols; ++j)
                    result[i] += matrix[i][j] * vector[j];
            return result;
        }

        /// <summary>
        /// Doolittle LUP decomposition with partial pivoting.
        /// Returns: result is L (with 1s on diagonal) and U;
        /// perm holds row permutations; toggle is +1 or -1 (even or odd)
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="perm"></param>
        /// <param name="toggle"></param>
        /// <returns></returns>
        static double[][] MatrixDecompose(double[][] matrix, out int[] perm, out int toggle) {
            int rows = matrix.Length;
            int cols = matrix[0].Length; // assume square
            if (rows != cols) throw new Exception("Attempt to decompose a non-square m");
            int n = rows; // convenience

            double[][] result = MatrixDuplicate(matrix);

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; ++i) { perm[i] = i; }
            toggle = 1; // toggle tracks row swaps. (+1 -> even, -1 -> odd. used by MatrixDeterminant)
            for (int j = 0; j < n - 1; ++j){ // each column
                double colMax = Math.Abs(result[j][j]); // find largest val in col
                int pRow = j;
                // reader Matt V needed this:
                for (int i = j + 1; i < n; ++i){
                    if (Math.Abs(result[i][j]) > colMax){
                        colMax = Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }
                // Not sure if this approach is needed always, or not.
                if (pRow != j){ // if largest value not on pivot, swap rows
                    double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;
                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;
                    toggle = -toggle; // adjust the row-swap toggle
                }
                // --------------------------------------------------
                // This part added later (not in original)
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j
                // --------------------------------------------------
                if (result[j][j] == 0.0){
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row){
                        if (result[row][j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1) throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    double[] rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;
                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;
                    toggle = -toggle; // adjust the row-swap toggle
                }

                for (int i = j + 1; i < n; ++i){
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; ++k){
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }
            } // main j column loop
            return result;
        }

        /// <summary>
        /// Inverse Matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[][] MatrixInverse(double[][] matrix) {
            int n = matrix.Length;
            double[][] result = MatrixDuplicate(matrix);

            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null) throw new Exception("Unable to compute inverse");

            double[] b = new double[n];
            for (int i = 0; i < n; ++i){
                for (int j = 0; j < n; ++j){
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }
                double[] x = HelperSolve(lum, b); // 
                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }
            return result;
        }

        /// <summary>
        /// Calculates the determinant of a matrix (if exists)
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double MatrixDeterminant(double[][] matrix) {
            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null) throw new Exception("Unable to compute MatrixDeterminant");
            double result = toggle;
            for (int i = 0; i < lum.Length; ++i)
                result *= lum[i][i];
            return result;
        }

        /// <summary>
        /// Helper function. Before calling this helper, permute b using the perm array
        /// from MatrixDecompose that generated luMatrix
        /// </summary>
        /// <param name="luMatrix"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double[] HelperSolve(double[][] luMatrix, double[] b) {
            int n = luMatrix.Length;
            double[] x = new double[n];
            b.CopyTo(x, 0);
            for (int i = 1; i < n; ++i){
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }
            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i){
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }
            return x;
        }

        /// <summary>
        /// Allocates/creates a duplicate of a matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[][] MatrixDuplicate(double[][] matrix) {
            double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i) // copy the values
                for (int j = 0; j < matrix[i].Length; ++j)
                    result[i][j] = matrix[i][j];
            return result;
        }

        /// <summary>
        /// Solves a system of n equation of type Ax = b
        /// k11.x1+k12.x2+...+k1n.xn = b1
        /// k21.x1+k22.x2+...+k2n.xn = b2
        /// ............................
        /// km1.x1+km2.x2+...+kmn.xn = bn
        /// </summary>
        /// <param name="A">(k11,k12,...,k1n),.....,(km1,km2,....,kmn)</param>
        /// <param name="b">(b1,b2,...,bn)</param>
        /// <returns>x1,x2,x3,....,xn</returns>
        public static double[] SystemSolve(double[][] A, double[] b) {
            int n = A.Length;
            // 1. decompose A
            int[] perm;
            int toggle;
            double[][] luMatrix = MatrixDecompose(A, out perm, out toggle);
            if (luMatrix == null) return null;
            // 2. permute b according to perm[] into bp
            double[] bp = new double[b.Length];
            for (int i = 0; i < n; ++i) bp[i] = b[perm[i]];
            // 3. call helper
            double[] x = HelperSolve(luMatrix, bp);
            return x;
        }

        /// <summary>
        /// Muntiplies two matrices given as 2d-arrays and returns the resulting
        /// matrix as another 2d-array. The columns of first array must be equal to the rows of second
        /// otherelse the function returns null.
        /// </summary>
        /// <param name="a">First array</param>
        /// <param name="b">Second array</param>
        /// <returns></returns>
        public static double[,] MatrixMultiplication(double[,] a, double[,] b) {
            //a rows=r1 columns=c1
            //b rows=r2 columns=c2
            int r1 = a.GetLength(0);
            int c1 = a.GetLength(1);
            int r2 = b.GetLength(0);
            int c2 = b.GetLength(1);
            if (c1 != r2) return null;
            double[,] c = new double[r1, c2];
            for (int i = 0; i < r1; i++){
                for (int j = 0; j < c2; j++){
                    c[i, j] = 0;
                    for (int k = 0; k < c1; k++) c[i, j] += a[i, k] * b[k, j];
                }
            }
            return c;
        }

        /// <summary>
        /// Muntiplies two matrices given as jagged-arrays and returns the resulting
        /// matrix as another jagged-array. 
        /// The columns of first array must be equal to the rows of second
        /// otherelse the function returns null.
        /// </summary>
        /// <param name="first">First matrix as jagged array</param>
        /// <param name="second">Second matrix as jagged array</param>
        /// <returns></returns>
        public static double[][] MatrixMultiplication(double[][] first, double[][] second) {
            //first r1=rows c1=columns
            //second r2=rows c2=columns
            double sum = 0.0;

            int c1 = first[0].Length;//columns of first array
            int r1 = first.Length;//rows of first array
            int c2 = second[0].Length;//columns of second array
            int r2 = second.Length;//rows of second array
            if (c1 != r2) return null;
            //define the resulting matrix as another jagged-array with m rows and q columns
            double[][] multiply = new double[r1][];
            for (int i = 0; i < r1; i++) multiply[i] = new double[c2];
            //now multiply the two matrices
            for (int c = 0; c < r1; c++){
                for (int d = 0; d < c2; d++){
                    sum = 0;
                    for (int k = 0; k < r2; k++){
                        sum += first[c][k] * second[k][d];
                    }
                    multiply[c][d] = sum;
                }
            }
            //and return the resulting matrix
            return multiply;
        }

        public static double[,] MatrixTranspose(double[,] a) {
            int r = a.GetLength(0);
            int c = a.GetLength(1);
            double[,] b = new double[c, r];
            for (int i = 0; i < r; i++){
                for (int j = 0; j < c; j++){
                    b[j, i] = a[i, j];
                }
            }
            return b;
        }

        public static double[][] MatrixTranspose(double[][] a) {
            int r = a.GetLength(0);
            int c = a.GetLength(1);
            double[][] b = new double[c][];
            for (int i = 0; i < c; i++) b[i] = new double[r];
            for (int i = 0; i < r; i++){
                for (int j = 0; j < c; j++){
                    b[j][i] = a[i][j];
                }
            }
            return b;
        }

        public static void TestMatrixMultiplication() {
            double[][] first = new double[][]
            {
                new double[] {1, 4, 2},
                new double[] {2, 5, 1}
            };
            double[][] second = new double[][]
            {
                new double[] {3, 4 , 2, 1},
                new double[] {3, 5 , 7, 1},
                new double[] {1, 2 , 1, 1},
                new double[] {1, 2 , 1, 1}
            };

            double[][] result = MatrixMultiplication(first, second);
            if (result == null) {Console.Out.WriteLine("Matrix Multiplication not possible..."); return;
        }
            int cm = result[0].Length;
            int rm = result.Length;
            Console.Out.WriteLine("Result: Matrix {0}x{1}", rm, cm);
            Console.Out.WriteLine("Matrix start ---------------");
            for(int r = 0; r < rm; r++)
            {
                for(int c = 0; c < cm; c++)
                {
                    Console.Out.Write("\t" + result[r][c]);
                }
                Console.Out.Write("\r\n");
            }
            Console.Out.WriteLine("Matrix end ---------------");
        }
    }
}
