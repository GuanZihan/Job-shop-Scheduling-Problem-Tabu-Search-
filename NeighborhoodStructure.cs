using System.Collections.Generic;

namespace demo
{
    public abstract class NeighborhoodStructure
    {
        private Machine[] machines;
        private Operation[] criticalPath;
        //通过搜索邻域结构，返回Machine[]的集合
        abstract public  Dictionary<string, object> searchNeighborhood (Machine[] machines, Operation[] criticalPath);
        abstract public  int evaluateNeighborhood(Machine[] machineNeighborhoods);
        abstract public  Dictionary<string, object> chooseBestFromNeighborhoods(Operation[] operations, Job[] jobs, Machine[][] neighborhoods);

    }
}