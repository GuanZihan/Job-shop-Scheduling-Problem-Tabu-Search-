using System;
using System.Collections.Generic;
namespace demo {
    public class Machine {
        private int machineId;
        private Operation[] operations;
        public Machine (int machineId) {
            this.machineId = machineId;
        }

        public void setOperations (Operation[] operations) {
            this.operations = operations;
        }

        public Operation[] getOperations () {
            return this.operations;
        }

        public void addOperation (Operation operation) {
            List<Operation> operationTemp;
            if (this.operations == null) {
                operationTemp = new List<Operation> ();
            } else {
                operationTemp = new List<Operation> (this.operations);

            }

            operationTemp.Add (operation);
            this.operations = operationTemp.ToArray ();
        }

        public Operation[] findMachinePredecessor (Operation operation) {
            List<Operation> machinePredecessors = new List<Operation> ();
            for (int i = 0; i < this.operations.Length; i++) {
                if (operations[i] == operation && (i - 1) >= 0) {
                    machinePredecessors.Add (operations[i - 1]);
                    break;
                } else if (operations[i] == operation && i == 0) {
                    machinePredecessors.Add (new Operation (-1, 0, -1, -1));
                }
            }
            return machinePredecessors.ToArray ();
        }

        public Operation[] findMachineSuccessor (Operation operation) {
            List<Operation> machineSuccessors = new List<Operation> ();
            for (int i = 0; i < this.operations.Length; i++) {
                if (operations[i] == operation && i < operations.Length - 1) {
                    machineSuccessors.Add (operations[i + 1]);
                    break;
                } else if (operations[i] == operation && i == operations.Length - 1) {
                    Operation endOperation = new Operation (-2, 0, -2, -2);
                    endOperation.setEndTime (0);
                    machineSuccessors.Add (endOperation);
                }
            }
            return machineSuccessors.ToArray ();
        }

        public override string ToString () {
            return "Machine is " + this.machineId;
        }
    }
}