using System;

// demo of amoeba method numerical optimization
// see "A Simplex Method for Function Minimization", J.A. Nelder and R. Mead,
// The Computer Journal, vol. 7, no. 4, 1965, pp.308-313.

namespace grepnova2
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
    class AmoebaProgram
    {
        public static void Main(string[] args) {
            try
            {
                Console.WriteLine("\nBegin amoeba method optimization demo\n");
                Console.WriteLine("Solving Rosenbrock's function f(x,y) = 100*(y-x^2)^2 + (1-x)^2");
                Console.WriteLine("Function has a minimum at x = 1.0, y = 1.0 when f = 0.0\n");

                int dim = 2;  // problem dimension (number of variables to solve for)
                int amoebaSize = 3;  // number of potential solutions in the amoeba
                double minX = -10.0;
                double maxX = 10.0;
                int maxLoop = 50;

                Console.WriteLine("Creating amoeba with size = " + amoebaSize);
                Console.WriteLine("Setting maxLoop = " + maxLoop);
                Amoeba a = new Amoeba(ObjectiveFunction, null, amoebaSize, dim, minX, maxX, maxLoop, null);  // an amoeba method optimization solver

                Console.WriteLine("\nInitial amoeba is: \n");
                Console.WriteLine(a.ToString());

                Console.WriteLine("\nBeginning reflect-expand-contract solve loop\n");
                Solution sln = a.Solve();
                Console.WriteLine("\nSolve complete\n");

                Console.WriteLine("Final amoeba is: \n");
                Console.WriteLine(a.ToString());

                Console.WriteLine("\nBest solution found: \n");
                Console.WriteLine(sln.ToString());

                Console.WriteLine("\nEnd amoeba method optimization demo\n");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        public static double ObjectiveFunction(double[] vector){//, object dataSource) {
            // Rosenbrock's function, the function to be minimized
            // no data source needed here but real optimization problems will often be based on data
            double x = vector[0];
            double y = vector[1];
            return 100.0 * Math.Pow((y - x * x), 2) + Math.Pow(1 - x, 2);
            //return (x * x) + (y * y); // sphere function
        }

        public static int KillFunction(double v1, double v2, double v3, double v4) {//, object dataSource) {
            return 0;
        }

    } // program class

    public class Solution : IComparable<Solution>
    {
        // a potential solution (array of double) and associated value (so can be sorted against several potential solutions
        public double[] vector;
        public double value;
        Func<double[], double> function;

        static Random random = new Random(1);  // to allow creation of random solutions

        public Solution(Func<double[], double> function, int dim, double minX, double maxX, double[] init=null) {
            // a random Solution
            this.vector = new double[dim];
            this.function = function;
            if (init != null)
                this.vector = init;
            else{
                for (int i = 0; i < dim; ++i)
                    this.vector[i] = (maxX - minX) * random.NextDouble() + minX;
            }
            this.value = function(this.vector);// AmoebaProgram.ObjectiveFunction(this.vector);//, null);
        }

        public Solution(Func<double[], double> function, double[] vector) {
            // a specifiede solution
            this.vector = new double[vector.Length];
            this.function = function;
            Array.Copy(vector, this.vector, vector.Length);
            this.value = function(this.vector);// AmoebaProgram.ObjectiveFunction(this.vector);//, null);
        }

        public int CompareTo(Solution other) // based on vector/solution value
        {
            if (this.value < other.value)
                return -1;
            else if (this.value > other.value)
                return 1;
            else
                return 0;
        }

        public override string ToString() {
            string s = "[ ";
            for (int i = 0; i < this.vector.Length; ++i)
            {
                if (vector[i] >= 0.0) s += " ";
                s += vector[i].ToString("F2") + " ";
            }
            s += "]  val = " + this.value.ToString("F4");
            return s;
        }
    }

    public class Amoeba
    {
        public int amoebaSize;  // number of solutions
        public int dim;         // vector-solution size, also problem dimension
        public Solution[] solutions;  // potential solutions (vector + value)

        public double minX;
        public double maxX;

        public double alpha;  // reflection
        public double beta;   // contraction
        public double gamma;  // expansion

        public int maxLoop;   // limits main solving loop
        Func<double[], double> function;
        Func<double, double, double, double, int> kill_function;



        public Amoeba(Func<double[], double> function, Func<double, double, double, double, int> kill_function, int amoebaSize, int dim, double minX, double maxX, int maxLoop, double[] init) {
            this.amoebaSize = amoebaSize;
            this.dim = dim;
            this.minX = minX;
            this.maxX = maxX;
            this.alpha = 1.0;  // hard-coded values from theory
            this.beta = 0.5;
            this.gamma = 2.0;

            this.maxLoop = maxLoop;
            this.function = function;
            this.kill_function = kill_function;

            this.solutions = new Solution[amoebaSize];
            for (int i = 0; i < solutions.Length; ++i)
                solutions[i] = new Solution(function, dim, minX, maxX, init);  // the Solution ctor calls the objective function to compute value

            Array.Sort(solutions);
        }

        public Solution Centroid() {
            // return the centroid of all solution vectors except for the worst (highest index) vector
            double[] c = new double[dim];
            for (int i = 0; i < amoebaSize - 1; ++i)
                for (int j = 0; j < dim; ++j)
                    c[j] += solutions[i].vector[j];  // accumulate sum of each vector component

            for (int j = 0; j < dim; ++j)
                c[j] = c[j] / (amoebaSize - 1);

            Solution s = new Solution(function, c);  // feed vector to ctor which calls objective function to compute value
            return s;
        }

        public Solution Reflected(Solution centroid) {
            // the reflected solution extends from the worst (lowest index) solution through the centroid
            double[] r = new double[dim];
            double[] worst = this.solutions[amoebaSize - 1].vector;  // convenience only
            for (int j = 0; j < dim; ++j)
                r[j] = ((1 + alpha) * centroid.vector[j]) - (alpha * worst[j]);
            Solution s = new Solution(function, r);
            return s;
        }

        public Solution Expanded(Solution reflected, Solution centroid) {
            // expanded extends even more, from centroid, thru reflected
            double[] e = new double[dim];
            for (int j = 0; j < dim; ++j)
                e[j] = (gamma * reflected.vector[j]) + ((1 - gamma) * centroid.vector[j]);
            Solution s = new Solution(function, e);
            return s;
        }

        public Solution Contracted(Solution centroid) {
            // contracted extends from worst (lowest index) towards centoid, but not past centroid
            double[] v = new double[dim];  // didn't want to reuse 'c' from centoid routine
            double[] worst = this.solutions[amoebaSize - 1].vector;  // convenience only
            for (int j = 0; j < dim; ++j)
                v[j] = (beta * worst[j]) + ((1 - beta) * centroid.vector[j]);
            Solution s = new Solution(function, v);
            return s;
        }

        public void Shrink() {
            // move all vectors, except for the best vector (at index 0), halfway to the best vector
            // compute new objective function values and sort result
            for (int i = 1; i < amoebaSize; ++i)  // note we don't start at [0]
            {
                for (int j = 0; j < dim; ++j)
                {
                    solutions[i].vector[j] = (solutions[i].vector[j] + solutions[0].vector[j]) / 2.0;
                    solutions[i].value = function(solutions[i].vector);// AmoebaProgram.ObjectiveFunction(solutions[i].vector, null);
                }
            }
            Array.Sort(solutions);
        }

        public void ReplaceWorst(Solution newSolution) {
            // replace the worst solution (at index size-1) with contents of parameter newSolution's vector
            for (int j = 0; j < dim; ++j)
                solutions[amoebaSize - 1].vector[j] = newSolution.vector[j];
            solutions[amoebaSize - 1].value = newSolution.value;
            Array.Sort(solutions);
        }

        public bool IsWorseThanAllButWorst(Solution reflected) {
            // Solve needs to know if the reflected vector is worse (greater value) than every vector in the amoeba, except for the worst vector (highest index)
            for (int i = 0; i < amoebaSize - 1; ++i)  // not the highest index (worst)
            {
                if (reflected.value <= solutions[i].value)  // reflected is better (smaller value) than at least one of the non-worst solution vectors
                    return false;
            }
            return true;
        }

        public Solution Solve() {
            int t = 0;  // loop counter
            while (t < maxLoop)
            {
                ++t;

                if (t % 10 == 0)
                {
                    Console.WriteLine("At t = " + t + " curr best solution = " + this.solutions[0]);
                }

                Solution centroid = Centroid();  // compute centroid
                Solution reflected = Reflected(centroid);  // compute reflected

                if (reflected.value < solutions[0].value)  // reflected is better than the curr best
                {
                    Solution expanded = Expanded(reflected, centroid);  // can we do even better??
                    if (expanded.value < solutions[0].value)  // winner! expanded is better than curr best
                        ReplaceWorst(expanded);  // replace curr worst solution with expanded
                    else
                        ReplaceWorst(reflected);  // it was worth a try . . . 
                    continue;
                }

                if (IsWorseThanAllButWorst(reflected) == true)  // reflected is worse (larger value) than all solution vectors (except possibly the worst one)
                {
                    if (reflected.value <= solutions[amoebaSize - 1].value)  // reflected is better (smaller) than the curr worst (last index) vector
                        ReplaceWorst(reflected);

                    Solution contracted = Contracted(centroid);  // compute a point 'inside' the amoeba

                    if (contracted.value > solutions[amoebaSize - 1].value)  // contracted is worse (larger value) than curr worst (last index) solution vector
                        Shrink();
                    else
                        ReplaceWorst(contracted);

                    continue;
                }

                ReplaceWorst(reflected);

                //if (IsSorted() == false)
                //  throw new Exception("Unsorted at k = " + k);
                //if (kill_function != null)
                //    if (kill_function(solutions[0].vector[0], solutions[0].vector[1], solutions[0].vector[2], solutions[0].vector[3]) == -1) break;

            }  // solve loop

            return solutions[0];  // best solution is always at [0]
        }

        //public bool IsSorted()  // state invariant. used during development
        //{
        //  for (int i = 0; i < solutions.Length - 1; ++i)
        //    if (solutions[i].value > solutions[i + 1].value)
        //      return false;
        //  return true;
        //}

        public override string ToString() {
            string s = "";
            for (int i = 0; i < solutions.Length; ++i)
                s += "[" + i + "] " + solutions[i].ToString() + Environment.NewLine;
            return s;
        }

    } // class Amoeba

} // ns
