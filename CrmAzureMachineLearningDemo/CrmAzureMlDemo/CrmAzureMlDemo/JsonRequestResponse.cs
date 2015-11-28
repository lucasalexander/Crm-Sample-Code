using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CrmAzureMlDemo
{
    [DataContract]
    public class JsonRequest
    {
        [DataMember(Name = "Inputs")]
        public Input1 InputObj { get; set; }
    }

    [DataContract]
    public class Input1
    {
        [DataMember(Name = "input1")]
        public Input Inputs { get; set; }
    }

    [DataContract]
    public class Input
    {
        [DataMember(Name = "ColumnNames")]
        public string[] Columns { get; set; }

        [DataMember(Name = "Values")]
        public object[][] Values { get; set; }
    }

    [DataContract]
    public class JsonResponse
    {
        [DataMember(Name = "Results")]
        public Result Results { get; set; }

        [DataMember(Name = "ContainerAllocationDurationMs")]
        public int ContainerAllocationDurationMs { get; set; }

        [DataMember(Name = "ExecutionDurationMs")]
        public int ExecutionDurationMs { get; set; }

        [DataMember(Name = "IsWarmContainer")]
        public bool IsWarmContainer { get; set; }

        public JsonResponse()
        {
            Results = new Result();
        }
    }

    [DataContract]
    public class Result
    {
        [DataMember(Name = "output1")]
        public Output Output1 { get; set; }

        public Result()
        {
            Output1 = new Output();
        }
    }

    [DataContract]
    public class Output
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "value")]
        public OutputValue Value { get; set; }

        public Output()
        {
            Value = new OutputValue();
        }
    }

    [DataContract]
    public class OutputValue
    {
        [DataMember(Name = "ColumnNames")]
        public string[] Names { get; set; }

        [DataMember(Name = "ColumnTypes")]
        public string[] Types { get; set; }

        [DataMember(Name = "Values")]
        public string[][] Values { get; set; }

        public OutputValue()
        {
            Values = new string[1][];
        }
    }
}
