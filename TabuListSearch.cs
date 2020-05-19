using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
namespace demo {
    public class TabuListSearch {
        private static int MAX_LENGTH = 8;
        public Machine[][] candidates;
        public int tabuListIndex = 0;
        public int[] currentSolution = new int[MAX_LENGTH];
        private Operation[] criticalPath;
        private int currentMakeSpan;
        public int maxNeighborScore;
        public int bestMakeSpan = 999999;
        public int changedElement;
        public int currentIteration;
        public int lastIteration;
        private Schedule schedule;
        private Operation[] operations;
        private Job[] jobs;
        private Machine[] machines;
        private int jobNumbers;
        private int machineNumbers;
        public int problemSet;
        //1. Read txt and initialize the problem
        //including Operation information, Job INformation
        //store Operation information in fileParser.operations
        //store Job information in fileParser.jobs, in which every Job Instance has its own array to store the operations
        public TabuListSearch (String filePath) {
            FileParser fileParser = FileParser.GetFileParser ();
            fileParser.readFile (filePath);
            this.operations = fileParser.getOperations ();
            this.jobs = fileParser.getJobs ();
            this.machineNumbers = fileParser.getMachineNumber ();
            this.jobNumbers = fileParser.getJobNumbers ();
            this.problemSet = this.jobNumbers * this.machineNumbers;
        }

        //2. generate first permutation, machine should be assigned(initial solution)
        public Machine[] generateInitialSolution () {
            //assign to the currentSolution
            Machine[] retMachineList = new Machine[this.machineNumbers];
            for (int i = 0; i < retMachineList.Length; i++) {
                retMachineList[i] = new Machine (i);
            }
            for (int i = 1; i < this.operations.Length - 1; i++) {
                // Console.WriteLine(b[i]);
                Machine machine = retMachineList[operations[i].getMachineId ()];
                machine.addOperation (operations[i]);
            }

            return retMachineList;
        }
        //3. schedule.makeSpan() to compute the startTime, endTime, and critical Path(comoute)
        public void computeMakeSpan (Operation[] operations, Job[] jobs, Machine[] machines) {
            this.schedule = new Schedule (operations, jobs, machines, 0, problemSet);

            Dictionary<string, object> retDict = schedule.computeStartTime ();
            int[] startTimeAndMakeSpan = (int[]) retDict["startTime"];
            // this.criticalPath = ((List<Operation>)retDict["criticalPath"]).ToArray();

            // Console.WriteLine(startTimeAndMakeSpan.Length);
            Schedule schedule1 = new Schedule (operations, jobs, machines, 1, problemSet);
            int[] array = schedule1.computeEndTime (); //写死了的

            List<Operation> criticalPathList = new List<Operation> ();
            for (int i = 0; i < this.problemSet; i++) {

                if (startTimeAndMakeSpan[i] + array[i] == startTimeAndMakeSpan[this.problemSet]) {
                    Console.Write (i + "->");
                    criticalPathList.Add (operations[i + 1]);
                }
            }

            // for (int m = 0; m < criticalPath.Length; m++) {
            //     criticalPath[m].setStartTime(startTimeAndMakeSpan[criticalPath[m].getOperationNumber()]);
            // }
            criticalPathList.Sort (new StartTimeComparator ());
            this.criticalPath = criticalPathList.ToArray ();
            this.currentMakeSpan = startTimeAndMakeSpan[this.problemSet]; //main purpose is to assign to the currentMakeSpan field
        }
        //stopping criteria
        public bool isStop (Machine[] machines) {
            //if already sorted

            //after many tries without improving the Cmax
            if (currentIteration - lastIteration > 10) {
                return true;
            }

            return false;
        }
        //evaluation method
        //5. choose the best one from neighborhood(evaluation)

        public int evaluateSolution (int[] solution) {
            int score = 0;
            tranverse (solution);
            for (int i = 0; i < 7; i++) {
                if ((solution[i + 1] - solution[i]) >= 0) {
                    score++;
                }
            }
            return score;
        }

        //search strategy
        //not belong to tabulist or it satisfies aspiration function
        //4. search the neighborhood(neighborhood structure)
        //aspiration function
        public bool isAspiration (int[] array) {
            return false;
        }

        private static void tranverse (int[] array) {
            for (int i = 0; i < array.Length; i++) {
                Console.Write (array[i] + " ");
            }
            Console.WriteLine ("\n");
        }

        private void filterCandidate (Machine[][] candidates) {

        }

        public void tabuListSearch () {
            List<int> makeSpanSet = new List<int> ();

            this.machines = generateInitialSolution ();
            this.computeMakeSpan (this.operations, this.jobs, this.machines);
            if (currentMakeSpan < bestMakeSpan) {
                bestMakeSpan = currentMakeSpan;
            }
            Console.WriteLine ("The Best makespan is " + bestMakeSpan);

            N1Balas n1Balas = new N1Balas (problemSet); //创建邻域结构
            N1 n1 = new N1 (problemSet);
            N5 n5 = new N5 (problemSet);
            A : while (!isStop (machines)) {
                currentIteration++;
                Dictionary<string, object> neighborhoodDict = n5.searchNeighborhood (machines, this.criticalPath);
                // Dictionary<string, object> neighborhoodDict = n1.searchNeighborhood (machines, this.criticalPath);

                candidates = (Machine[][]) neighborhoodDict["candidateMachines"];
                // int[] candidateEvaluations = (int[])neighborhoodDict["candidateEvaluations"];
                if (candidates.Length == 0) {
                    break;
                }
                Dictionary<string, object> retDict = n5.chooseBestFromNeighborhoods (this.operations, this.jobs, candidates);

                // Dictionary<string, object> retDict = n1.chooseBestFromNeighborhoods (this.operations, this.jobs, candidates);
                Machine[] bestSolution = (Machine[]) retDict["bestMachine"];
                // N1.TabuListOperation tabuOperation = (N1.TabuListOperation) retDict["tabuOperation"];
                this.maxNeighborScore = (int) retDict["bestScore"];
                this.criticalPath = ((List<Operation>) retDict["criticalPath"]).ToArray ();
                makeSpanSet.Add (maxNeighborScore);

                if (maxNeighborScore < bestMakeSpan) {
                    // Console.WriteLine ("修正最大完工时间!!! 有改进!!!" + "以前的是" + maxNeighborScore);
                    this.machines = bestSolution;
                    this.bestMakeSpan = maxNeighborScore;
                    lastIteration = currentIteration;

                } else if (maxNeighborScore > bestMakeSpan) {
                    this.machines = bestSolution;
                } else {
                    this.machines = bestSolution;
                    this.bestMakeSpan = maxNeighborScore;
                }
            }

            while (!isStop (machines)) {
                // lastIteration = currentIteration;
                currentIteration++;
                // Dictionary<string, object> neighborhoodDict = n5.searchNeighborhood (machines, this.criticalPath);
                Dictionary<string, object> neighborhoodDict = n1.searchNeighborhood (machines, this.criticalPath);

                candidates = (Machine[][]) neighborhoodDict["candidateMachines"];
                // int[] candidateEvaluations = (int[])neighborhoodDict["candidateEvaluations"];
                if (candidates.Length == 0) {
                    break;
                }
                // Dictionary<string, object> retDict = n5.chooseBestFromNeighborhoods (this.operations, this.jobs, candidates);

                Dictionary<string, object> retDict = n1.chooseBestFromNeighborhoods (this.operations, this.jobs, candidates);
                Machine[] bestSolution = (Machine[]) retDict["bestMachine"];
                // N1.TabuListOperation tabuOperation = (N1.TabuListOperation) retDict["tabuOperation"];
                this.maxNeighborScore = (int) retDict["bestScore"];
                this.criticalPath = ((List<Operation>) retDict["criticalPath"]).ToArray ();
                makeSpanSet.Add (maxNeighborScore);

                if (maxNeighborScore < bestMakeSpan) {
                    Console.WriteLine ("修正最大完工时间!!! 有改进!!!" + "以前的是" + maxNeighborScore);
                    this.machines = bestSolution;
                    this.bestMakeSpan = maxNeighborScore;
                    lastIteration = currentIteration;
                    goto A;
                } else if (maxNeighborScore > bestMakeSpan) {
                    this.machines = bestSolution;
                } else {
                    this.machines = bestSolution;
                    this.bestMakeSpan = maxNeighborScore;
                    goto A;
                }
            }
            Console.WriteLine ("搜寻完毕，现在找到的最小makespan是" + bestMakeSpan);
            for (int i = 0; i < makeSpanSet.ToArray ().Length; i++) {
                Console.Write (makeSpanSet[i] + ",");
            }
        }
    }
}