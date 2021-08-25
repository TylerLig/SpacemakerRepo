using GeoJSON.Net.Converters;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Spacemaker.Data
{
    public class FileDatabase : IDataAccess
    {
        private readonly string DataPath = "wwwroot/data";
        private JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new GeometryConverter() }
        };

        public List<Solution> GetSolutions()
        {
            Directory.CreateDirectory(DataPath);
            string[] proposedSolutionFiles = Directory.GetFiles(DataPath);

            List<Solution> output = new List<Solution>();
            int count = 1;
            foreach (string filePath in proposedSolutionFiles)
            {
                string json = File.ReadAllText(filePath);
                FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json, SerializerSettings);
                Solution newSolution = new Solution() { Features = featureCollection, Title = "Solution "+count++};
                newSolution.ParsePolygons();
                output.Add(newSolution);
            }
            return output;
        }
    }
}
