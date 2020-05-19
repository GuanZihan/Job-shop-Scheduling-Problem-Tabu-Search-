using System;
using System.Collections.Generic;

namespace demo {
    public class Schedule {
        private Operation[] operationOnJobs;
        private Operation[] operationOnMachines;
        private Operation[] lastOperationsOnJob;
        public Operation startOperation;
        private Operation endOperation;
        private Operation[] operations; //一定要注意！！！！！operations[0]是开始节点
        private Job[] jobs;
        private Machine[] machines;

        private int reverseFlag;
        private int jobOrMachine;
        private int problemSet;
        /*
        for any given problem, jobs and operations are fixed while machines are free to change
        传过来的Operations，jobs，machines中的Operation都没有赋前继和后继
        */
        public Schedule (Operation[] operations, Job[] jobs, Machine[] machines, int reverseFlag, int problemSet) {
            this.startOperation = operations[0];
            this.jobs = jobs;
            this.machines = machines;
            this.operations = operations;
            this.endOperation = operations[operations.Length - 1];
            this.problemSet = problemSet;
            //initialize operationOnJobs, operationOnMachines, startOperation and endOperation

            if (reverseFlag == 0) {
                this.reverseFlag = reverseFlag;
                assignValueFromStartNode ();
            }

            if (reverseFlag == 1) {
                this.reverseFlag = reverseFlag;
                assignValueFromEndNode ();
            }

        }

        /*
         */
        public Dictionary<string, object> computeStartTime () {
            int makeSpan = 0;
            int[] retList = new int[problemSet + 1]; //注意这里是写死了的
            // List<Operation> criticalPath = new List<Operation>();
            Dictionary<string, object> retDict = new Dictionary<string, object> ();
            Console.WriteLine ("computing makespan...");
            //0.initialize the starttime for start node
            //1.intersect and return the set
            var a = Intersect (operationOnJobs, operationOnMachines);
            int index = 0;
            List<Operation> criticalPath = new List<Operation> ();
            // Console.WriteLine(a.Length);
            while (a.Length > 0) {
                //2.loop in the set and schedule them
                for (int i = 0; i < a.Length; i++) {
                    // Console.WriteLine(a[i].getOperationNumber() + " " + a[i].getJobId());
                    var startTime = scheduleOperationFromStart (jobs[a[i].getJobId ()].findJobPredecessor (a[i]), machines[a[i].getMachineId ()].findMachinePredecessor (a[i]));
                    a[i].setStartTime (startTime);

                    retList[a[i].getOperationNumber ()] = a[i].getStartTime ();
                    //更新makespan
                    if (startTime + a[i].getProcessTime () > makeSpan) {
                        makeSpan = startTime + a[i].getProcessTime ();

                    }
                }
                //3.remove old Operation and Add New
                this.operationOnJobs = updateSuccessorOperationList (a, this.operationOnJobs, 1);
                this.operationOnMachines = updateSuccessorOperationList (a, this.operationOnMachines, 2);
                //                tranverse(operationOnJobs);
                // tranverse(operationOnMachines);
                index++;
                a = Intersect (operationOnJobs, operationOnMachines);
            }
            Console.WriteLine ("makespan is " + makeSpan);
            retList[problemSet] = makeSpan; //写死了的

            retDict.Add ("startTime", retList);
            return retDict;
        }

        public int[] computeEndTime () {
            Console.WriteLine ("computing makespan...");
            this.operations[this.operations.Length - 1].setEndTime (0);
            var a = Intersect (operationOnJobs, operationOnMachines);
            int[] retList = new int[problemSet]; //注意这里是写死了的
            while (a.Length > 0) {
                for (int i = 0; i < a.Length; i++) {
                    var endTime = scheduleOperationFromEnd (a[i], jobs[a[i].getJobId ()].findJobSuccessor (a[i]), machines[a[i].getMachineId ()].findMachineSuccessor (a[i]));
                    a[i].setEndTime (endTime);
                    retList[a[i].getOperationNumber ()] = a[i].getEndTime ();
                }
                this.operationOnJobs = updatePredecessorOperationList (a, this.operationOnJobs, 1);
                this.operationOnMachines = updatePredecessorOperationList (a, this.operationOnMachines, 2);

                a = Intersect (operationOnJobs, operationOnMachines);
                // Console.WriteLine(index + " " + a.Length + " " +  operationOnMachines.Length + " " + operationOnJobs.Length);
                // for(int j = 0; j < 10; j ++) {
                //     Console.WriteLine(operationOnJobs[j] + " " + operationOnMachines[j]);
                // }
            }
            return retList;
        }

        private Operation[] Intersect (Operation[] a, Operation[] b) {
            Operation[] retOperationsArray = new Operation[] { };
            List<Operation> retOperations = new List<Operation> ();

            for (int i = 0; i < a.Length; i++) {
                for (int j = 0; j < b.Length; j++) {
                    if (a[i] == b[j]) {
                        retOperations.Add (a[i]);
                    }
                }
            }

            retOperationsArray = retOperations.ToArray ();
            return retOperationsArray;
        }

        private int scheduleOperationFromStart (Operation[] jobPredecessor, Operation[] machinePredecessor) {
            int maxStartTime = 0;
            for (int i = 0; i < machinePredecessor.Length; i++) {
                if (machinePredecessor[i].getStartTime () + machinePredecessor[i].getProcessTime () > maxStartTime) {
                    // Console.WriteLine(machinePredecessor[i]);
                    maxStartTime = machinePredecessor[i].getStartTime () + machinePredecessor[i].getProcessTime ();
                }
            }
            for (int i = 0; i < jobPredecessor.Length; i++) {
                if (jobPredecessor[i].getStartTime () + jobPredecessor[i].getProcessTime () > maxStartTime) {
                    maxStartTime = jobPredecessor[i].getStartTime () + jobPredecessor[i].getProcessTime ();
                }
            }
            return maxStartTime;
        }

        private int scheduleOperationFromEnd (Operation operation, Operation[] jobSuccessor, Operation[] machineSuccessor) {
            int endTime = 0;
            for (int i = 0; i < jobSuccessor.Length; i++) {
                if (jobSuccessor[i].getEndTime () + operation.getProcessTime () > endTime) {
                    endTime = jobSuccessor[i].getEndTime () + operation.getProcessTime ();
                }
            }

            for (int i = 0; i < machineSuccessor.Length; i++) {
                if (machineSuccessor[i].getEndTime () + operation.getProcessTime () > endTime) {
                    endTime = machineSuccessor[i].getEndTime () + operation.getProcessTime ();
                }
            }
            return endTime;
        }
        private Operation[] updateSuccessorOperationList (Operation[] scheduledOperations, Operation[] operations, int flag) {
            List<Operation> retOperations = new List<Operation> (operations);
            if (flag == 1) {
                for (int i = 0; i < scheduledOperations.Length; i++) {
                    //remove scheduled Operations
                    retOperations.Remove (scheduledOperations[i]);

                    //add New Schedulable Operations
                    retOperations.AddRange (this.jobs[scheduledOperations[i].getJobId ()].findJobSuccessor (scheduledOperations[i]));
                }
            }

            if (flag == 2) {
                for (int i = 0; i < scheduledOperations.Length; i++) {
                    //remove scheduled Operations
                    retOperations.Remove (scheduledOperations[i]);
                    //add New Schedulable Operations
                    retOperations.AddRange (this.machines[scheduledOperations[i].getMachineId ()].findMachineSuccessor (scheduledOperations[i]));
                    // Console.WriteLine(scheduledOperations[i] + " " + this.machines[scheduledOperations[i].getMachineId ()].findMachineSuccessor (scheduledOperations[i])[0]);
                }
            }
            return retOperations.ToArray ();
        }

        private Operation[] updatePredecessorOperationList (Operation[] scheduledOperations, Operation[] operations, int flag) {
            List<Operation> retOperations = new List<Operation> (operations);
            if (flag == 1) {
                for (int i = 0; i < scheduledOperations.Length; i++) {
                    //remove scheduled Operations
                    retOperations.Remove (scheduledOperations[i]);
                    //add New Schedulable Operations
                    retOperations.AddRange (this.jobs[scheduledOperations[i].getJobId ()].findJobPredecessor (scheduledOperations[i]));
                }
            }

            if (flag == 2) {
                for (int i = 0; i < scheduledOperations.Length; i++) {
                    //remove scheduled Operations
                    retOperations.Remove (scheduledOperations[i]);
                    //add New Schedulable Operations
                    retOperations.AddRange (this.machines[scheduledOperations[i].getMachineId ()].findMachinePredecessor (scheduledOperations[i]));
                }
            }
            return retOperations.ToArray ();

        }
        private bool isContained (Operation[] operations, Operation operation) {
            for (int i = 0; i < operations.Length; i++) {
                if (operations[i] == operation) {
                    return true;
                }

            }
            return false;
        }

        private void assignValueFromStartNode () {
            operationOnJobs = new Operation[jobs.Length];
            for (int i = 0; i < operationOnJobs.Length; i++) {
                operationOnJobs[i] = jobs[i].GetOperations () [0];
            }

            operationOnMachines = new Operation[machines.Length];
            for (int i = 0; i < operationOnMachines.Length; i++) {
                operationOnMachines[i] = machines[i].getOperations () [0];
            }

            lastOperationsOnJob = new Operation[jobs.Length];
            for (int i = 0; i < operationOnJobs.Length; i++) {
                lastOperationsOnJob[i] = jobs[i].GetOperations () [jobs[i].GetOperations ().Length - 1];
            }
        }
        private void assignValueFromEndNode () {
            operationOnJobs = new Operation[jobs.Length];
            for (int i = 0; i < operationOnJobs.Length; i++) {
                operationOnJobs[i] = jobs[i].GetOperations () [jobs[i].GetOperations ().Length - 1]; //拿jobs里面每个job的最后一个
            }

            operationOnMachines = new Operation[machines.Length];
            for (int i = 0; i < operationOnMachines.Length; i++) {
                operationOnMachines[i] = machines[i].getOperations () [machines[i].getOperations ().Length - 1]; //每个machine最后一个
            }

            lastOperationsOnJob = new Operation[jobs.Length];
            for (int i = 0; i < operationOnJobs.Length; i++) {
                lastOperationsOnJob[i] = jobs[i].GetOperations () [0]; //对于反方向来说，lastOperation就是开头的
            }
        }

    }
}