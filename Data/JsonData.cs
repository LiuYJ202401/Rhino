using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace PublicContent.JsonData
{
    public class JsonData
    {
        public static void SaveToJsonFile<T>(string filePath, T data)
        {
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }
        public static T LoadFromJsonFile<T>(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
    public class Rs_AutoRail
    {
        public double Rail_Space{ get; set; }
        public double Rail_Width { get; set; }
        public double Rail_Length { get; set; }
        public double Rail_Height { get; set; }
        public double fushou_Width { get; set; }
        public double fushou_Height { get; set; }
        public double delta { get; set; }
        public bool reverse { get; set; }
    }
}
