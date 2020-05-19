using System;
using System.IO;

namespace demo {
    public class FileParser {

        //total job number
        private int jobNumber;
        // total machine number
        private int machineNumber;
        //set of operations
        private Operation[] operations;
        private Job[] jobs;
        private static FileParser fileParser;

        private FileParser () {

        }

        public static FileParser GetFileParser () {
            if (fileParser == null) {
                fileParser = new FileParser ();
            }
            return fileParser;

        }

        public Operation[] getOperations () {
            return this.operations;
        }

        public Job[] getJobs() {
            return this.jobs;
        }

        public int getJobNumbers() {
            return this.jobNumber;
        }
        public int getMachineNumber() {
            return this.machineNumber;
        }

        //read txt file to initialize Operation Class
        public void readFile (String path) {
            using (StreamReader sr = new StreamReader (path)) {
                String line;

                while ((line = sr.ReadLine ()) != null) {
                    if (line.StartsWith ("#")) {
                        //do nothing
                    } else {
                        //read the first line, ie. job number and machine number
                        String[] numbers = line.Split (" ");
                        jobNumber = Int32.Parse (numbers[0]);
                        machineNumber = Int32.Parse (numbers[1]);
                        this.operations = new Operation[machineNumber * jobNumber + 2]; //here I initialize the operations[] firstly
                        //then I add the start node and end node
                        Operation startOperation = new Operation(-1, 0, -1, -1);
                        startOperation.setStartTime(0);
                        Operation endOperation = new Operation(-2, 0, -2, -2);
                        endOperation.setEndTime(0);
                        this.operations[0] = startOperation;
                        this.operations[machineNumber * jobNumber + 1] = endOperation;
                        //next, initialize the job set
                        this.jobs = new Job[jobNumber];
                        break;
                    }
                }

                int operationNumber = 0;
                int jobNumberIndex = 0;
                while ((line = sr.ReadLine ()) != null) {

                    String[] operationAndProcessTime = line.Split (" ");

                    Operation[] operationInAJob = new Operation[machineNumber];

                    for (int i = 0; i < operationAndProcessTime.Length; i = i + 2) {
                        /*
                        operationNumber: operationNumber
                        Int32.Parse(operationAndProcessTime[i + 1]): processTime
                        Int32.Parse(operationAndProcessTime[i]): machineId
                        jobNumberIndex: jobId
                        */
                        Operation operation = new Operation (operationNumber, Int32.Parse (operationAndProcessTime[i + 1]), Int32.Parse (operationAndProcessTime[i]), jobNumberIndex);
                        this.operations[operationNumber + 1] = operation; //operations: store all the operations
                        operationInAJob[i / 2] = operation; //operationInAJob: store only the speicfic operations in a job
                        operationNumber++;
                    }
                    //store Job Information
                    Job job = new Job (jobNumberIndex);
                    job.setOperations (operationInAJob);
                    this.jobs[jobNumberIndex] = job;

                    jobNumberIndex++;

                }

                Console.WriteLine ("Read File Finished...");
                Console.WriteLine ("Totally, there are " + machineNumber + " machines and " + jobNumber + " jobNumbers");
            }
        }

    }
}