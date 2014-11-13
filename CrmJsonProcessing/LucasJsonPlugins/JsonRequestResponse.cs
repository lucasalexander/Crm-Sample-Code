using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LucasJsonPlugins
{
    //DataContract decoration necessary for serialization/deserialization to work properly
    [DataContract]
    public class MyJsonResponse
    {
        //datamember name value indicates name of json field to which data will be serialized/from which data will be deserialized
        [DataMember(Name = "output1")]
        public string Output1 { get; set; }

        [DataMember(Name = "output2")]
        public string Output2 { get; set; }

        public MyJsonResponse() { }
    }

    //DataContract decoration necessary for serialization/deserialization to work properly
    [DataContract]
    public class MyJsonRequest
    {
        //datamember name value indicates name of json field to which data will be serialized/from which data will be deserialized
        [DataMember(Name = "input1")]
        public string Input1 { get; set; }

        [DataMember(Name = "input2")]
        public string Input2 { get; set; }

        public MyJsonRequest() { }
    }
}
