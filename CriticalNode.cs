namespace demo
{
    public class CriticalNode
    {
        public CriticalNode nextNode;
        public Operation currentOperation;
        public CriticalNode(Operation operation) {
            this.currentOperation = operation;
        }
    }
}