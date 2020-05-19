using System;
using System.Collections.Generic;
namespace demo {
    public class N1Balas {
        Machine[] machines; //暂时保存当前的机器顺序
        Operation[] criticalPath; //暂时保存当前的关键路径
        public struct TabuListOperation {
            public int startOperationNumber;
            public int endOperationNumber;
        }
        public TabuListOperation tabuListOperation;
        public static int MAX_LENGTH_UPPER = 17;
        public static int MAX_LENGTH_LOWER = 16;
        public static int MAX_LENGTH = MAX_LENGTH_LOWER;
        public List<TabuListOperation> possibleOperations = new List<TabuListOperation> ();
        public List<TabuListOperation> tabuListOperations = new List<TabuListOperation> ();
        public int problemset;
        public Machine[] currentMachines;
        public int[] currentStartTime;
        public int[] currentEndTime;
        public N1Balas (int problemset) {
            this.problemset = problemset;
        }
        public Dictionary<string, object> chooseBestFromNeighborhoods (Operation[] operations, Job[] jobs, Machine[][] neighborhoods, int[] candidateEvaluations) {
            // Console.WriteLine(neighborhoods.Length);
            Dictionary<String, object> retDict = new Dictionary<string, object> ();
            int bestScore = 99999;
            Machine[] bestMachine = this.machines;
            int[] bestA = new int[] { };
            List<Operation> criticalPath = new List<Operation> ();
            var retTabuOperation = new TabuListOperation ();
            int neighborIndex = 0;
            for (int i = 0; i < neighborhoods.Length; i++) {

                if (candidateEvaluations[i] < bestScore) {
                    // bestScore = a[problemset];
                    // bestA = a; //最优头时间集合
                    // bestMachine = neighborhoods[i];
                    // criticalPath.Clear ();
                    // retTabuOperation = this.possibleOperations[i];
                    // for (int j = 0; j < problemset; j++) {
                    //     // Console.WriteLine (j + "的" + "startTime " + a[j] + " end time " + b[j] + "  " + operations[j + 1]+ " " + neighborhoods[i][operations[j + 1].getMachineId()].findMachineSuccessor(operations[j + 1])[0]);
                    //     if (a[j] + b[j] == bestScore) {
                    //         criticalPath.Add (operations[j + 1]);

                    //     }
                    // }
                    bestScore = candidateEvaluations[i];
                    neighborIndex = i;
                }
            }

            Schedule schedule_start = new Schedule (operations, jobs, neighborhoods[neighborIndex], 0, problemset);
            Schedule schedule_end = new Schedule (operations, jobs, neighborhoods[neighborIndex], 1, problemset);

            Dictionary<string, object> dict = schedule_start.computeStartTime ();
            bestA = (int[]) dict["startTime"];
            int[] b = schedule_end.computeEndTime ();
            bestScore = bestA[problemset];
            bestMachine = neighborhoods[neighborIndex];
            retTabuOperation = this.possibleOperations[neighborIndex];
            for (int j = 0; j < problemset; j++) {
                // Console.WriteLine (j + "的" + "startTime " + a[j] + " end time " + b[j] + "  " + operations[j + 1]+ " " + neighborhoods[i][operations[j + 1].getMachineId()].findMachineSuccessor(operations[j + 1])[0]);
                if (bestA[j] + b[j] == bestScore) {
                    criticalPath.Add (operations[j + 1]);

                }
            }

            for (int m = 0; m < criticalPath.ToArray ().Length; m++) {
                criticalPath[m].setStartTime (bestA[criticalPath[m].getOperationNumber ()]);
            }

            criticalPath.Sort (new StartTimeComparator ());
            List<Operation> test = new List<Operation> ();

            test.Add (criticalPath[0]);
            int index = 0;
            int num = bestA[criticalPath[0].getOperationNumber ()] + criticalPath[0].getProcessTime ();

            while (num != bestScore) {
                for (int i = index; i < criticalPath.ToArray ().Length - 1; i++) {
                    if (criticalPath[index].getProcessTime () + bestA[criticalPath[index].getOperationNumber ()] == bestA[criticalPath[i + 1].getOperationNumber ()]) {
                        // Console.WriteLine(criticalPath[i] + " " + bestA[criticalPath[i].getOperationNumber()]);
                        if (bestMachine[criticalPath[i + 1].getMachineId ()].findMachinePredecessor (criticalPath[i + 1]) [0] == criticalPath[index] || jobs[criticalPath[i + 1].getJobId ()].findJobPredecessor (criticalPath[i + 1]) [0] == criticalPath[index]) {
                            test.Add (criticalPath[i + 1]);
                            index = i + 1;
                            num += criticalPath[i + 1].getProcessTime ();
                            break;
                        }
                    }
                }
            }

            criticalPath = test;
            this.tabuListOperations.Add (retTabuOperation);
            Console.WriteLine ("现在tabuList长度为" + this.tabuListOperations.ToArray ().Length);
            for (int j = 0; j < criticalPath.ToArray ().Length; j++) {
                Console.Write (criticalPath[j] + "-> ");
            }

            retDict.Add ("bestMachine", bestMachine);
            retDict.Add ("bestScore", bestScore);
            retDict.Add ("criticalPath", criticalPath);
            retDict.Add ("tabuOperation", retTabuOperation);
            return retDict;
        }

        public int evaluateNeighborhood (Machine[] machineNeighborhoods) {
            return 0;
        }

        public Dictionary<string, object> searchNeighborhood (Operation[] operations, Job[] jobs, Machine[] machines, Operation[] criticalPath) {
            this.possibleOperations.Clear ();
            Dictionary<string, object> retDict = new Dictionary<string, object> ();
            List<Machine[]> retMachines = new List<Machine[]> ();
            List<int> retEvaluations = new List<int> ();
            if (tabuListOperations.ToArray ().Length == MAX_LENGTH) {
                tabuListOperations.RemoveAt (0);
            }
            int retMachineIndex = 0;
            this.machines = machines;
            this.criticalPath = criticalPath;

            Schedule schedule_start = new Schedule (operations, jobs, machines, 0, problemset);
            Schedule schedule_end = new Schedule (operations, jobs, machines, 1, problemset);

            Dictionary<string, object> dict = schedule_start.computeStartTime ();
            int[] startTime = (int[]) dict["startTime"];
            int[] endTime = schedule_end.computeEndTime ();

            for (int i = 0; i < criticalPath.Length - 1; i++) {
                Machine[] tempMachine = new Machine[machines.Length];
                //复制tempMachine
                for (int k = 0; k < machines.Length; k++) {
                    Operation[] operationList = new Operation[machines[k].getOperations ().Length];
                    for (int j = 0; j < machines[k].getOperations ().Length; j++) {
                        operationList[j] = machines[k].getOperations () [j];
                    }
                    Machine machines1 = new Machine (k);
                    machines1.setOperations (operationList);
                    tempMachine[k] = machines1;
                }
                //找到邻居之前先判断是否在关键路径和同一机器上；是否在禁忌表里 

                if (criticalPath[i].getMachineId () == criticalPath[i + 1].getMachineId () && !isInTabuList (criticalPath[i + 1].getOperationNumber (), criticalPath[i].getOperationNumber (), this.tabuListOperations.ToArray ())) {

                    TabuListOperation tabuListOperation = new TabuListOperation ();

                    tabuListOperation.startOperationNumber = criticalPath[i + 1].getOperationNumber ();
                    tabuListOperation.endOperationNumber = criticalPath[i].getOperationNumber (); //找到了邻居之后立即入队
                    possibleOperations.Add (tabuListOperation); //备选的tabuListOperation对

                    int indexInMachine_1 = searchOperation (machines[criticalPath[i].getMachineId ()].getOperations (), criticalPath[i]);
                    int indexInMachine_2 = searchOperation (machines[criticalPath[i + 1].getMachineId ()].getOperations (), criticalPath[i + 1]);

                    Operation[] a = machines[criticalPath[i].getMachineId ()].getOperations ();
                    Operation[] tempOperations = new Operation[a.Length];

                    for (int j = 0; j < a.Length; j++) {
                        tempOperations[j] = a[j];
                    }

                    Operation o1 = tempOperations[indexInMachine_1];
                    Operation o2 = tempOperations[indexInMachine_2];
                    tempOperations[indexInMachine_1] = o2;
                    tempOperations[indexInMachine_2] = o1;
                    tempMachine[criticalPath[i].getMachineId ()].setOperations (tempOperations); //设置operations
                    retMachines.Add (tempMachine); //返回的邻居+1

                    //创建一个返回数组，记录所有邻居的evaluation
                    int r_b_new_1 = machines[criticalPath[i].getMachineId ()].findMachinePredecessor (criticalPath[i]) [0].getProcessTime ();
                    if (machines[criticalPath[i].getMachineId ()].findMachinePredecessor (criticalPath[i]) [0].getOperationNumber () != -1) {
                        r_b_new_1 = startTime[machines[criticalPath[i].getMachineId ()].findMachinePredecessor (criticalPath[i]) [0].getOperationNumber ()] + machines[criticalPath[i].getMachineId ()].findMachinePredecessor (criticalPath[i]) [0].getProcessTime ();
                    }

                    int r_b_new_2 = jobs[criticalPath[i + 1].getJobId ()].findJobPredecessor (criticalPath[i + 1]) [0].getProcessTime ();
                    if (jobs[criticalPath[i + 1].getJobId ()].findJobPredecessor (criticalPath[i + 1]) [0].getOperationNumber () != -1) {
                        r_b_new_2 = startTime[jobs[criticalPath[i + 1].getJobId ()].findJobPredecessor (criticalPath[i + 1]) [0].getOperationNumber ()] + jobs[criticalPath[i + 1].getJobId ()].findJobPredecessor (criticalPath[i + 1]) [0].getProcessTime ();
                    }

                    int r_b_new = Math.Max (r_b_new_1, r_b_new_2); //r_b_new

                    int r_a_new_2 = jobs[criticalPath[i].getJobId ()].findJobPredecessor (criticalPath[i]) [0].getProcessTime ();
                    if (jobs[criticalPath[i].getJobId ()].findJobPredecessor (criticalPath[i]) [0].getOperationNumber () != -1) {
                        r_a_new_2 = startTime[jobs[criticalPath[i].getJobId ()].findJobPredecessor (criticalPath[i]) [0].getOperationNumber ()] + jobs[criticalPath[i].getJobId ()].findJobPredecessor (criticalPath[i]) [0].getProcessTime ();
                    }

                    int r_a_new = Math.Max (r_b_new + criticalPath[i + 1].getProcessTime (), r_a_new_2);

                    int t_a_new_1 = 0;
                    if (machines[criticalPath[i + 1].getMachineId ()].findMachineSuccessor (criticalPath[i + 1]) [0].getOperationNumber () != -2) {
                        t_a_new_1 = endTime[machines[criticalPath[i + 1].getMachineId ()].findMachineSuccessor (criticalPath[i + 1]) [0].getOperationNumber ()];
                    }

                    int t_a_new_2 = 0;
                    Console.WriteLine (criticalPath[i]);
                    if (jobs[criticalPath[i].getJobId ()].findJobSuccessor (criticalPath[i]) [0].getOperationNumber () != -2) {
                        t_a_new_2 = endTime[jobs[criticalPath[i].getJobId ()].findJobSuccessor (criticalPath[i]) [0].getOperationNumber ()];
                    }
                    int t_a_new = Math.Max (t_a_new_1, t_a_new_2) + criticalPath[i].getProcessTime ();

                    int t_b_new_2 = 0;
                    if (jobs[criticalPath[i + 1].getJobId ()].findJobSuccessor (criticalPath[i + 1]) [0].getOperationNumber () != -2) {
                        t_b_new_2 = endTime[jobs[criticalPath[i + 1].getJobId ()].findJobSuccessor (criticalPath[i + 1]) [0].getOperationNumber ()];
                    }
                    int t_b_new = Math.Max (t_a_new, t_b_new_2) + criticalPath[i + 1].getProcessTime ();

                    retEvaluations.Add (Math.Max (r_a_new + t_a_new, r_b_new + t_b_new));

                    retMachineIndex++;

                }

                //     if(criticalPath[i].getMachineId () == criticalPath[i + 1].getMachineId () && retMachines.ToArray().Length == 0) {
                //         TabuListOperation tabuListOperation = new TabuListOperation ();

                //         tabuListOperation.startOperationNumber = criticalPath[i + 1].getOperationNumber ();
                //         tabuListOperation.endOperationNumber = criticalPath[i].getOperationNumber (); //找到了邻居之后立即入队
                //         possibleOperations.Add (tabuListOperation); //备选的tabuListOperation对

                //         int indexInMachine_1 = searchOperation (machines[criticalPath[i].getMachineId ()].getOperations (), criticalPath[i]);
                //         int indexInMachine_2 = searchOperation (machines[criticalPath[i + 1].getMachineId ()].getOperations (), criticalPath[i + 1]);

                //         Operation[] a = machines[criticalPath[i].getMachineId ()].getOperations ();
                //         Operation[] tempOperations = new Operation[a.Length];

                //         for (int j = 0; j < a.Length; j++) {
                //             tempOperations[j] = a[j];
                //         }

                //         Operation o1 = tempOperations[indexInMachine_1];
                //         Operation o2 = tempOperations[indexInMachine_2];
                //         tempOperations[indexInMachine_1] = o2;
                //         tempOperations[indexInMachine_2] = o1;
                //         tempMachine[criticalPath[i].getMachineId ()].setOperations (tempOperations); //设置operations
                //         retMachines.Add (tempMachine);

                //         retMachineIndex++;
                // }
            }

            retDict.Add ("candidateMachines", retMachines.ToArray ());
            retDict.Add ("candidateEvaluations", retEvaluations.ToArray ());
            return retDict;
        }

        private int searchOperation (Operation[] operations, Operation operation) {
            for (int i = 0; i < operations.Length; i++) {
                if (operations[i] == operation) {
                    return i;
                }
            }
            return -1;
        }

        private bool isInTabuList (int startOperationNumber, int endOperationNumber, TabuListOperation[] tabuListOperations) {
            for (int i = 0; i < tabuListOperations.Length; i++) {
                if (tabuListOperations[i].startOperationNumber == startOperationNumber && tabuListOperations[i].endOperationNumber == endOperationNumber) {
                    return true;
                }
            }
            return false;
        }
    }

}