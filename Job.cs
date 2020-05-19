using System;
using System.Collections.Generic;
namespace demo {
    public class Job {
        private int id;
        private Operation[] operations;
        public Job (int id) {
            this.id = id;
        }

        public int getId () {
            return this.id;
        }

        public Operation[] GetOperations () {
            return this.operations;
        }

        public void setOperations (Operation[] operations) {
            this.operations = operations;
        }

        public void addOperation (Operation operation) {
            List<Operation> operationList;
            if (this.operations == null) {
                operationList = new List<Operation> ();
            } else {
                operationList = new List<Operation> (this.operations);
            }
            operationList.Add (operation);
        }

        public void editOperation (int operationIdInJob, Operation operation) {
            this.operations[operationIdInJob] = operation;
        }

        public Operation[] findJobPredecessor (Operation operation) {
            List<Operation> jobPredecessors = new List<Operation> ();
            for (int i = 0; i < this.operations.Length; i++) {
                if (operations[i] == operation && i >= 1) {
                    jobPredecessors.Add (operations[i - 1]);
                    break;
                } else if (operations[i] == operation && i == 0) {
                    jobPredecessors.Add (new Operation (-1, 0, -1, -1));
                }
            }
            return jobPredecessors.ToArray ();
        }

        public Operation[] findJobSuccessor (Operation operation) {
            List<Operation> jobSuccessors = new List<Operation> ();
            for (int i = 0; i < this.operations.Length; i++) {
                if (operations[i] == operation && i < operations.Length - 1) {
                    jobSuccessors.Add (operations[i + 1]);
                    break;
                } else if (operations[i] == operation && i == operations.Length - 1) {
                    Operation endOperation = new Operation (-2, 0, -2, -2);
                    endOperation.setEndTime (0);
                    jobSuccessors.Add (endOperation);
                }
            }
            return jobSuccessors.ToArray ();
        }

        public override string ToString () {
            return "Job is " + this.id;
        }

        // public override bool Equals(object obj) => ((Job)obj).id == this.id ? true : false;
    }
}