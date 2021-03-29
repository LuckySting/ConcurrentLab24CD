using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentLab4
{
    class Program
    {
        static void QSortSerial(int start, int end)
        {
            if (end - start + 1 < 2)
            {
                return;
            }

            int first = start;
            int last = end;
            int middle = arrayCopy[(start + end) / 2];
            do
            {
                while (arrayCopy[first] < middle) first++;
                while (arrayCopy[last] > middle) last--;
                if (first <= last)
                {
                    int temp = arrayCopy[first];
                    arrayCopy[first] = arrayCopy[last];
                    arrayCopy[last] = temp;
                    first++;
                    last--;
                }
            } while (first < last);

            if (start < last)
            {
                QSortSerial(start, last);
            }

            if (first < end)
            {
                QSortSerial(first, end);
            }
        }

        static void QSortParallel(int start, int end, int threads = 8)
        {
            // Console.Write("Thread {0}: from {1} to {2}\n", threads, start, end);
            if (end - start + 1 < 2)
            {
                return;
            }

            int first = start;
            int last = end;
            int middle = arrayCopy[(start + end) / 2];
            do
            {
                while (arrayCopy[first] < middle) first++;
                while (arrayCopy[last] > middle) last--;
                if (first <= last)
                {
                    int temp = arrayCopy[first];
                    arrayCopy[first] = arrayCopy[last];
                    arrayCopy[last] = temp;
                    first++;
                    last--;
                }
            } while (first < last);

            if (threads > 1)
            {
                Thread threadOne = new Thread(() => { QSortParallel(start, last, threads / 2); });
                Thread threadTwo = new Thread(() => { QSortParallel(first, end, threads / 2); });
                if (start < last)
                {
                    threadOne.Start();
                }

                if (first < end)
                {
                    threadTwo.Start();
                }

                threadOne.Join();
                threadTwo.Join();
            }
            else
            {
                if (start < last)
                {
                    QSortSerial(start, last);
                }

                if (first < end)
                {
                    QSortSerial(first, end);
                }
            }
        }

        static int[] arr;
        static int[] arrayCopy;
        static int size = 10000000;

        static void QSortTask(int start, int end, int taskIdx = 0)
        {
            if (end - start + 1 < 2)
            {
                return;
            }

            int first = start;
            int last = end;
            int middle = arrayCopy[(start + end) / 2];
            do
            {
                while (arrayCopy[first] < middle) first++;
                while (arrayCopy[last] > middle) last--;
                if (first <= last)
                {
                    int temp = arrayCopy[first];
                    arrayCopy[first] = arrayCopy[last];
                    arrayCopy[last] = temp;
                    first++;
                    last--;
                }
            } while (first < last);

            Task taskOne = new Task(() => { QSortParallel(start, last, taskIdx + 2); });
            Task taskTwo = new Task(() => { QSortParallel(first, end, taskIdx + 4); });
            if (start < last)
            {
                taskOne.Start();
            }

            if (first < end)
            {
                taskTwo.Start();
            }

            Task.WaitAll(new Task[] {taskOne, taskTwo});
        }


        static public int RunSerialSort()
        {
            arrayCopy = new int[size];
            for (int i = 0; i < size; i++)
            {
                arrayCopy[i] = arr[i];
            }

            QSortSerial(0, size - 1);
            return 0;
        }

        static public int RunParallelSort()
        {
            arrayCopy = new int[size];
            for (int i = 0; i < size; i++)
            {
                arrayCopy[i] = arr[i];
            }

            QSortParallel(0, size - 1);
            return 0;
        }

        static public int RunTaskSort()
        {
            arrayCopy = new int[size];
            for (int i = 0; i < size; i++)
            {
                arrayCopy[i] = arr[i];
            }

            QSortTask(0, size - 1);
            return 0;
        }

        static void Benchmark(Func<int> func, int warmups, int benchmarks)
        {
            Console.Write("Warm up {0} times:\n", warmups);
            for (int i = 0; i < warmups; i++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                func();
                sw.Stop();
                Console.Write("WarmUp {0}: {1:F} milliseconds\n", i + 1, sw.Elapsed.TotalMilliseconds);
            }

            Console.Write("\nBenchmark {0} times:\n", benchmarks);
            double meanTime = 0;
            for (int i = 0; i < benchmarks; i++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                func();
                sw.Stop();
                Console.Write("Benchmark {0}: {1:F} milliseconds\n", i + 1, sw.Elapsed.TotalMilliseconds);
                meanTime += sw.Elapsed.TotalMilliseconds;
            }

            Console.Write("Mean time: {0:F}\n", meanTime / benchmarks);
        }

        static void Main(string[] args)
        {
            foreach (var s in new[] {1000, 10000, 100000, 1000000, 10000000, 20000000, 30000000})
            {
                size = s;
                arr = new int[size];
                Random rand = new Random();
                for (int i = 0; i < size; i++)
                {
                    arr[i] = rand.Next();
                }
                Console.WriteLine("\n{0}: ", size);
                Console.WriteLine("Serial:");
                Benchmark(RunSerialSort, 3, 5);
                Console.WriteLine("Parallel:");
                Benchmark(RunParallelSort, 3, 5);
                Console.WriteLine("Task:");
                Benchmark(RunTaskSort, 3, 5);
            }
        }
    }
}