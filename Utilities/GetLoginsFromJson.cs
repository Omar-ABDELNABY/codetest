using codetest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace codetest.Utilities
{
    public class GetLoginsFromJson
    {
        readonly string _path;

        public GetLoginsFromJson(string path)
        {
            _path = path;
        }

        public List<Login> Execute()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var directory = Path.Combine(baseDirectory, _path);
            using (var reader = new StreamReader(directory))
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<Login>>(json);
            }
        }

    }
}
