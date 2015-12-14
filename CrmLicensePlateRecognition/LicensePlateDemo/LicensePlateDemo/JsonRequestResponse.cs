using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LicensePlateDemo
{

    [DataContract]
    public class JsonResponse
    {
        [DataMember(Name = "version")]
        public float Version { get; set; }

        [DataMember(Name = "image")]
        public string Image { get; set; }

        [DataMember(Name = "data_type")]
        public string DataType { get; set; }

        [DataMember(Name = "epoch_time")]
        public Int64 EpochTime { get; set; }

        [DataMember(Name = "img_width")]
        public int ImgWidth { get; set; }

        [DataMember(Name = "img_height")]
        public int ImgHeight { get; set; }

        [DataMember(Name = "processing_time_ms")]
        public float ProcessingTime { get; set; }

        [DataMember(Name = "regions_of_interest")]
        public RegionOfInterest[] RegionsOfInterest { get; set; }

        [DataMember(Name = "results")]
        public Result[] Results { get; set; }
    }

    [DataContract]
    public class Candidate
    {
        [DataMember(Name = "plate")]
        public string Plate { get; set; }

        [DataMember(Name = "confidence")]
        public float Confidence { get; set; }

        [DataMember(Name = "matches_template")]
        public bool MatchesTemplate { get; set; }
    }

    [DataContract]
    public class RegionOfInterest
    {
        [DataMember(Name = "x")]
        public float X { get; set; }

        [DataMember(Name = "y")]
        public float Y { get; set; }

        [DataMember(Name = "height")]
        public float Height { get; set; }

        [DataMember(Name = "width")]
        public float Width { get; set; }
    }

    [DataContract]
    public class Result
    {
        [DataMember(Name = "plate")]
        public string Plate { get; set; }

        [DataMember(Name = "confidence")]
        public float Confidence { get; set; }

        [DataMember(Name = "matches_template")]
        public bool MatchesTemplate { get; set; }

        [DataMember(Name = "plate_index")]
        public int PlateIndex { get; set; }

        [DataMember(Name = "region")]
        public string Region { get; set; }

        [DataMember(Name = "region_confidence")]
        public float RegionConfidence { get; set; }

        [DataMember(Name = "processing_time_ms")]
        public float ProcessingTime { get; set; }

        [DataMember(Name = "requested_topn")]
        public int NumRequested { get; set; }

        [DataMember(Name = "coordinates")]
        public PlateCoordinate[] PlateCoordinates { get; set; }

        [DataMember(Name = "candidates")]
        public Candidate[] Candiates { get; set; }
    }

    [DataContract]
    public class PlateCoordinate
    {
        [DataMember(Name = "x")]
        public float X { get; set; }

        [DataMember(Name = "y")]
        public float Y { get; set; }
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
}