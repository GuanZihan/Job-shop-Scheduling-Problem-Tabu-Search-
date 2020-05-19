using System.Collections.Generic;
namespace demo
{
    public class Operation
    {
        private int operationNumber; //作业序号
        private int processTime; //加工时间
        private int machineId; //机器序号
        private int jobId; //工件序号
        // private Operation[] jobPredecessor; //前继作业
        // private Operation[] jobSuccessor; //后继作业
        // private Operation[] machinePredecessor; //前继作业
        // private Operation[] machineSuccessor; //后继作业
        private int startTime; //头时间
        private int endTime; //尾时间

        public Operation (int operationNumber, int processTime, int machineId, int jobId) {
            this.operationNumber = operationNumber;
            this.processTime = processTime;
            this.machineId = machineId;
            this.jobId = jobId;
        }

        public int getOperationNumber () {
            return this.operationNumber;
        }

        public int getProcessTime () {
            return this.processTime;
        }

        public int getMachineId () {
            return this.machineId;
        }

        public int getJobId () {
            return this.jobId;
        }

        public int getStartTime () {
            return this.startTime;
        }

        public int getEndTime () {
            return this.endTime;
        }

        public void setStartTime(int startTime) {
            this.startTime = startTime;
        }

        public void setEndTime(int endTime) {
            this.endTime = endTime;
        }

        public override string ToString() {
            return "Operation Number " + this.operationNumber;
        }

        public override bool Equals(object obj) {
            if(obj == null) return false;
            if(this.getOperationNumber() == ((Operation)obj).getOperationNumber()) {
                return true;
            }
            return false;
        }

        public override int GetHashCode() {
            return getOperationNumber().GetHashCode();
        }
    }
}