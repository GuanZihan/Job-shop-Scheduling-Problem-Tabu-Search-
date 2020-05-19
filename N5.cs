using System;
using System.Collections.Generic;
namespace demo {
    public class N5 {
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
        public N5 (int problemset) {
            this.problemset = problemset;
        }
        public Dictionary<string, object> chooseBestFromNeighborhoods (Operation[] operations, Job[] jobs, Machine[][] neighborhoods) {
            // Console.WriteLine(neighborhoods.Length);
            Dictionary<String, object> retDict = new Dictionary<string, object> ();
            int bestScore = 99999;
            Machine[] bestMachine = this.machines;
            int[] bestA = new int[] { };
            List<Operation> criticalPath = new List<Operation> ();
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

            
            // Operation[] leftCriticalNodes = findOperationNotOnMainPath(test.ToArray(), criticalPath.ToArray());

            criticalPath = test;
            // criticalPath.Add(new Operation(-1, 0, -1, -1));
            // for(int i = 0 ; i < leftCriticalNodes.Length; i ++) {
            //     Console.WriteLine(leftCriticalNodes[i] + "-----");
            // }
            // criticalPath.AddRange(leftCriticalNodes);
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

        public Dictionary<string, object> searchNeighborhood (Machine[] machines, Operation[] criticalPath) {
            this.possibleOperations.Clear ();
            Dictionary<string, object> retDict = new Dictionary<string, object> ();
            List<Machine[]> retMachines = new List<Machine[]> ();
            Dictionary<int, List<Operation>> blockList = new Dictionary<int, List<Operation>> ();
            if (tabuListOperations.ToArray ().Length == MAX_LENGTH) {
                tabuListOperations.RemoveAt (0);
            }
            this.machines = machines;
            this.criticalPath = criticalPath;

            for (int i = 0; i < criticalPath.Length - 1; i++) {

                //复制tempMachine
                //找到邻居之前先判断是否在关键路径和同一机器上；是否在禁忌表里 
                if (criticalPath[i].getMachineId () == criticalPath[i + 1].getMachineId () && !isInTabuList (criticalPath[i + 1].getOperationNumber (), criticalPath[i].getOperationNumber (), this.tabuListOperations.ToArray ())) {
                    List<int> arrayList = new List<int>(blockList.Keys);
                    if (!isContain(arrayList.ToArray(), criticalPath[i].getMachineId ())) {
                        List<Operation> oper = new List<Operation> ();
                        oper.Add (criticalPath[i]);
                        oper.Add (criticalPath[i + 1]);
                        blockList[criticalPath[i].getMachineId ()] = oper;
                    } else {
                        
                        blockList[criticalPath[i].getMachineId ()].Add (criticalPath[i]);
                        blockList[criticalPath[i].getMachineId ()].Add (criticalPath[i + 1]);
                    }
                }
            }

            List<int> test = new List<int> (blockList.Keys);

            for (int i = 0; i < blockList.Count; i++) {
                // 每次循环都创建一个tempMachine
                Machine[] tempMachine = new Machine[machines.Length];
                for (int k = 0; k < machines.Length; k++) {
                    Operation[] operations = new Operation[machines[k].getOperations ().Length];
                    for (int j = 0; j < machines[k].getOperations ().Length; j++) {
                        operations[j] = machines[k].getOperations () [j];
                    }
                    Machine machines1 = new Machine (k);
                    machines1.setOperations (operations);
                    tempMachine[k] = machines1;
                }
                Machine[] tempMachine_2 = new Machine[machines.Length];
                for (int k = 0; k < machines.Length; k++) {
                    Operation[] operations = new Operation[machines[k].getOperations ().Length];
                    for (int j = 0; j < machines[k].getOperations ().Length; j++) {
                        operations[j] = machines[k].getOperations () [j];
                    }
                    Machine machines1 = new Machine (k);
                    machines1.setOperations (operations);
                    tempMachine_2[k] = machines1;
                }
                if (i == 0) {
                    if (blockList[test[i]].ToArray ().Length >= 2) {
                        //add to tabuListOperation
                        TabuListOperation tabuListOperation = new TabuListOperation ();
                        tabuListOperation.startOperationNumber = blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 1].getOperationNumber ();
                        tabuListOperation.endOperationNumber = blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2].getOperationNumber ();
                        possibleOperations.Add (tabuListOperation); //备选的tabuListOperation对
                        int indexInMachine_1 = searchOperation (machines[blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2].getMachineId ()].getOperations (), blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2]);
                        int indexInMachine_2 = searchOperation (machines[blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 1].getMachineId ()].getOperations (), blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 1]);
                        Operation[] a = machines[blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2].getMachineId ()].getOperations ();
                        Operation[] tempOperations = new Operation[a.Length];

                        for (int j = 0; j < a.Length; j++) {
                            tempOperations[j] = a[j];
                        }

                        Operation o1 = tempOperations[indexInMachine_1];
                        Operation o2 = tempOperations[indexInMachine_2];
                        tempOperations[indexInMachine_1] = o2;
                        tempOperations[indexInMachine_2] = o1;
                        tempMachine[blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2].getMachineId ()].setOperations (tempOperations); //设置operations
                        retMachines.Add (tempMachine);
                    }
                } else if (i < blockList.Count - 1) {
                    if (blockList[test[i]].ToArray ().Length >= 2) {
                        //add to temp
                        TabuListOperation tabuListOperation = new TabuListOperation ();
                        tabuListOperation.startOperationNumber = blockList[test[i]].ToArray () [0].getOperationNumber ();
                        tabuListOperation.endOperationNumber = blockList[test[i]].ToArray () [1].getOperationNumber ();
                        possibleOperations.Add (tabuListOperation); //备选的tabuListOperation对
                        int indexInMachine_1 = searchOperation (machines[blockList[test[i]].ToArray () [0].getMachineId ()].getOperations (), blockList[test[i]].ToArray () [0]);
                        int indexInMachine_2 = searchOperation (machines[blockList[test[i]].ToArray () [1].getMachineId ()].getOperations (), blockList[test[i]].ToArray () [1]);
                        Operation[] a = machines[blockList[test[i]].ToArray () [0].getMachineId ()].getOperations ();
                        Operation[] tempOperations = new Operation[a.Length];

                        for (int j = 0; j < a.Length; j++) {
                            tempOperations[j] = a[j];
                        }

                        Operation o1 = tempOperations[indexInMachine_1];
                        Operation o2 = tempOperations[indexInMachine_2];
                        tempOperations[indexInMachine_1] = o2;
                        tempOperations[indexInMachine_2] = o1;
                        tempMachine[blockList[test[i]].ToArray () [0].getMachineId ()].setOperations (tempOperations); //设置operations
                        
                        retMachines.Add (tempMachine);

                        //后两个

                        TabuListOperation tabuListOperation_2 = new TabuListOperation ();
                        tabuListOperation_2.startOperationNumber = blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 1].getOperationNumber ();
                        tabuListOperation_2.endOperationNumber = blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2].getOperationNumber ();
                        possibleOperations.Add (tabuListOperation_2); //备选的tabuListOperation对
                        int indexInMachine_1_2 = searchOperation (machines[blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2].getMachineId ()].getOperations (), blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2]);
                        int indexInMachine_2_2 = searchOperation (machines[blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 1].getMachineId ()].getOperations (), blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 1]);
                        Operation[] b = machines[blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2].getMachineId ()].getOperations ();
                        Operation[] tempOperations_2 = new Operation[a.Length];

                        for (int j = 0; j < a.Length; j++) {
                            tempOperations_2[j] = a[j];
                        }

                        Operation o1_2 = tempOperations_2[indexInMachine_1_2];
                        Operation o2_2 = tempOperations_2[indexInMachine_2_2];
                        tempOperations_2[indexInMachine_1_2] = o2_2;
                        tempOperations_2[indexInMachine_2_2] = o1_2;
                        tempMachine_2[blockList[test[i]].ToArray () [blockList[test[i]].ToArray ().Length - 2].getMachineId ()].setOperations (tempOperations_2); //设置operations
                        retMachines.Add (tempMachine_2);
                    }
                } else {
                    if (blockList[test[i]].ToArray ().Length >= 2) {
                        //add to temp
                        TabuListOperation tabuListOperation = new TabuListOperation ();
                        tabuListOperation.startOperationNumber = blockList[test[i]].ToArray () [0].getOperationNumber ();
                        tabuListOperation.endOperationNumber = blockList[test[i]].ToArray () [1].getOperationNumber ();
                        possibleOperations.Add (tabuListOperation); //备选的tabuListOperation对
                        int indexInMachine_1 = searchOperation (machines[blockList[test[i]].ToArray () [0].getMachineId ()].getOperations (), blockList[test[i]].ToArray () [0]);
                        int indexInMachine_2 = searchOperation (machines[blockList[test[i]].ToArray () [1].getMachineId ()].getOperations (), blockList[test[i]].ToArray () [1]);
                        Operation[] a = machines[blockList[test[i]].ToArray () [0].getMachineId ()].getOperations ();
                        Operation[] tempOperations = new Operation[a.Length];

                        for (int j = 0; j < a.Length; j++) {
                            tempOperations[j] = a[j];
                        }

                        Operation o1 = tempOperations[indexInMachine_1];
                        Operation o2 = tempOperations[indexInMachine_2];
                        tempOperations[indexInMachine_1] = o2;
                        tempOperations[indexInMachine_2] = o1;
                        tempMachine[blockList[test[i]].ToArray () [0].getMachineId ()].setOperations (tempOperations); //设置operations
                        retMachines.Add (tempMachine);
                    }
                }
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

        private bool isContain(int[] array, int i) {
            for(int j = 0; j < array.Length; j ++) {
                if(array[j] == i) {
                    return true;
                }
            }
            return false;
        }

        private Operation[] findOperationNotOnMainPath(Operation[] mainCriticalPath, Operation[] allCriticalNodes) {
            List<Operation> operations = new List<Operation>();
            for(int i = 0; i < allCriticalNodes.Length; i ++) {
                int flag = 0;
                for(int j = 0; j < mainCriticalPath.Length; j ++) {
                    if(mainCriticalPath[j] == allCriticalNodes[i]) {
                        flag = 1;
                        continue;
                    }
                }
                if(flag == 0) {
                    operations.Add(allCriticalNodes[i]);
                }
            }
            return operations.ToArray();
        }
    }

}