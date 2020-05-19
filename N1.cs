using System;
using System.Collections.Generic;
namespace demo {

    class N1 : NeighborhoodStructure {
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
        public N1 (int problemset) {
            this.problemset = problemset;
        }
        public override Dictionary<string, object> chooseBestFromNeighborhoods (Operation[] operations, Job[] jobs, Machine[][] neighborhoods) {
            // Console.WriteLine(neighborhoods.Length);
            Dictionary<String, object> retDict = new Dictionary<string, object> ();
            int bestScore = 99999;
            Machine[] bestMachine = this.machines;
            int[] bestA = new int[] { };
            List<Operation> criticalPath = new List<Operation> ();
            List<List<Operation>> criticalPathSet = new List<List<Operation>> ();
            var retTabuOperation = new TabuListOperation ();
            for (int i = 0; i < neighborhoods.Length; i++) {

                Schedule schedule_start = new Schedule (operations, jobs, neighborhoods[i], 0, problemset);
                Schedule schedule_end = new Schedule (operations, jobs, neighborhoods[i], 1, problemset);

                Dictionary<string, object> dict = schedule_start.computeStartTime ();
                int[] a = (int[]) dict["startTime"];
                int[] b = schedule_end.computeEndTime ();
                if (a[problemset] < bestScore) {
                    bestScore = a[problemset];
                    bestA = a; //最优头时间集合
                    bestMachine = neighborhoods[i];
                    criticalPath.Clear ();
                    retTabuOperation = this.possibleOperations[i];
                    for (int j = 0; j < problemset; j++) {
                        // Console.WriteLine (j + "的" + "startTime " + a[j] + " end time " + b[j] + "  " + operations[j + 1]+ " " + neighborhoods[i][operations[j + 1].getMachineId()].findMachineSuccessor(operations[j + 1])[0]);
                        if (a[j] + b[j] == bestScore) {
                            criticalPath.Add (operations[j + 1]);
                        }
                    }
                }
            }

            for (int m = 0; m < criticalPath.ToArray ().Length; m++) {
                criticalPath[m].setStartTime (bestA[criticalPath[m].getOperationNumber ()]);
            }

            criticalPath.Sort (new StartTimeComparator ());
            List<Operation> mainCriticalPath = new List<Operation> ();

            mainCriticalPath.Add (criticalPath[0]);
            int index = 0;
            int num = bestA[criticalPath[0].getOperationNumber ()] + criticalPath[0].getProcessTime ();

            while (num != bestScore) {
                for (int i = index; i < criticalPath.ToArray ().Length - 1; i++) {
                    if (criticalPath[index].getProcessTime () + bestA[criticalPath[index].getOperationNumber ()] == bestA[criticalPath[i + 1].getOperationNumber ()]) {
                        // Console.WriteLine(criticalPath[i] + " " + bestA[criticalPath[i].getOperationNumber()]);
                        if (bestMachine[criticalPath[i + 1].getMachineId ()].findMachinePredecessor (criticalPath[i + 1]) [0] == criticalPath[index] || jobs[criticalPath[i + 1].getJobId ()].findJobPredecessor (criticalPath[i + 1]) [0] == criticalPath[index]) {
                            mainCriticalPath.Add (criticalPath[i + 1]);
                            index = i + 1;
                            num += criticalPath[i + 1].getProcessTime ();
                            break;
                        }
                    }
                }
            }

            criticalPathSet.Add (mainCriticalPath);

            Operation[] leftCriticalNodes = findOperationNotOnMainPath (mainCriticalPath.ToArray (), criticalPath.ToArray ());

            // for (int i = 0; i < leftCriticalNodes.Length; i++) {
            //     Console.WriteLine (leftCriticalNodes[i] + "-----" + criticalPath.ToArray ().Length);
            //     List<Operation> newCriticalPath = new List<Operation> ();
            //     List<Operation> successors = findSuccessors (bestMachine, mainCriticalPath, leftCriticalNodes[i]);
            //     List<Operation> predecessors = findPredecessors (bestMachine, mainCriticalPath, leftCriticalNodes[i]);
            //     newCriticalPath.AddRange (predecessors);
            //     newCriticalPath.Add (leftCriticalNodes[i]);
            //     newCriticalPath.AddRange (successors);
            //     criticalPathSet.Add (newCriticalPath);
            //     for (int j = 0; j < newCriticalPath.ToArray ().Length; j++) {
            //         Console.WriteLine (newCriticalPath[j] +" " + newCriticalPath[j].getMachineId());
            //     }
            // }

            criticalPath = mainCriticalPath; //先找到一条关键路径

            this.tabuListOperations.Add (retTabuOperation);
            Console.WriteLine ("现在tabuList长度为" + this.tabuListOperations.ToArray ().Length);
            for (int j = 0; j < criticalPath.ToArray ().Length; j++) {
                Console.Write (criticalPath[j] + "-> ");
            }

            retDict.Add ("bestMachine", bestMachine);
            retDict.Add ("bestScore", bestScore);
            retDict.Add ("criticalPath", mainCriticalPath);
            retDict.Add ("tabuOperation", retTabuOperation);
            return retDict;
        }

        public override int evaluateNeighborhood (Machine[] machineNeighborhoods) {
            return 0;
        }

        public override Dictionary<string, object> searchNeighborhood (Machine[] machines, Operation[] criticalPath) {
            this.possibleOperations.Clear ();
            Dictionary<string, object> retDict = new Dictionary<string, object> ();
            List<Machine[]> retMachines = new List<Machine[]> ();
            if (tabuListOperations.ToArray ().Length == MAX_LENGTH) {
                tabuListOperations.RemoveAt (0);
            }
            int retMachineIndex = 0;
            this.machines = machines;
            // this.criticalPath = criticalPath;

            for (int i = 0; i < criticalPath.Length - 1; i++) {
                Machine[] tempMachine = new Machine[machines.Length];
                //复制tempMachine
                for (int k = 0; k < machines.Length; k++) {
                    Operation[] operations = new Operation[machines[k].getOperations ().Length];
                    for (int j = 0; j < machines[k].getOperations ().Length; j++) {
                        operations[j] = machines[k].getOperations () [j];
                    }
                    Machine machines1 = new Machine (k);
                    machines1.setOperations (operations);
                    tempMachine[k] = machines1;
                }
                //找到邻居之前先判断是否在关键路径和同一机器上；是否在禁忌表里 

                if (criticalPath[i].getMachineId () == criticalPath[i + 1].getMachineId () && !isInTabuList (criticalPath[i + 1].getOperationNumber (), criticalPath[i].getOperationNumber (), this.tabuListOperations.ToArray ())) {

                    // Console.WriteLine(criticalPath[i] + "和" + criticalPath[i + 1]);

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
                    retMachines.Add (tempMachine);

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

        private Operation[] findOperationNotOnMainPath (Operation[] mainCriticalPath, Operation[] allCriticalNodes) {
            List<Operation> operations = new List<Operation> ();
            for (int i = 0; i < allCriticalNodes.Length; i++) {
                int flag = 0;
                for (int j = 0; j < mainCriticalPath.Length; j++) {
                    if (mainCriticalPath[j] == allCriticalNodes[i]) {
                        flag = 1;
                        continue;
                    }
                }
                if (flag == 0) {
                    operations.Add (allCriticalNodes[i]);
                }
            }
            return operations.ToArray ();
        }

        private List<Operation> findSuccessors (Machine[] machines, List<Operation> criticalPath, Operation currentOperation) {
            List<Operation> successors = new List<Operation> ();
            for (int i = 0; i < criticalPath.ToArray ().Length; i++) {
                if (machines[currentOperation.getMachineId ()].findMachineSuccessor (currentOperation) [0] == criticalPath[i]) {
                    if (isContained (machines[currentOperation.getMachineId ()].findMachineSuccessor (currentOperation) [0], criticalPath)) {
                        for (int j = i; j < criticalPath.ToArray ().Length; j++) {
                            successors.Add (criticalPath[j]);
                        }
                        break;
                    } else {
                        successors.Add(currentOperation);
                        currentOperation = machines[currentOperation.getMachineId ()].findMachineSuccessor (currentOperation) [0];
                    }
                }
            }
            return successors;
        }

        private List<Operation> findPredecessors (Machine[] machines, List<Operation> criticalPath, Operation currentOperation) {
            List<Operation> predecessors = new List<Operation> ();
            for (int i = 0; i < criticalPath.ToArray ().Length; i++) {
                if (machines[currentOperation.getMachineId ()].findMachinePredecessor (currentOperation) [0] == criticalPath[i]) {
                    if(isContained(machines[currentOperation.getMachineId ()].findMachinePredecessor (currentOperation) [0], criticalPath)) {
                        for (int j = 0; j < i; j++) {
                        predecessors.Add (criticalPath[j]);
                    }
                    break;
                    } else {
                        predecessors.Add(currentOperation);
                        currentOperation = machines[currentOperation.getMachineId ()].findMachinePredecessor (currentOperation) [0];
                    }
                    
                }
            }
            return predecessors;
        }

        private bool isContained (Operation operation, List<Operation> operations) {
            for (int i = 0; i < operations.ToArray ().Length; i++) {
                if (operations[i] == operation) {
                    return true;
                }
            }
            return false;
        }
    }
}