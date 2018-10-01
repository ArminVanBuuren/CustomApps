﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace ProcessFilter.SPA
{
    public class CollectionBusinessProcess : List<BusinessProcess>
    {
        internal CollectionBusinessProcess()
        {
            
        }

        public CollectionBusinessProcess(string prcsPath)
        {
            List<string> files = Directory.GetFiles(prcsPath).ToList();
            files.Sort(StringComparer.CurrentCulture);

            int i = 0;
            foreach (string bpPath in files)
            {
                Add(new BusinessProcess(bpPath, ++i));
            }
        }

        public CollectionBusinessProcess Clone()
        {
            CollectionBusinessProcess currentClone = new CollectionBusinessProcess();
            currentClone.AddRange(this);
            return currentClone;
        }
    }

    public class BusinessProcess : ObjectTempalte
    {
        internal List<string> Operations { get; } = new List<string>();

        public BusinessProcess(string path, int id) : base(path, id)
        {
            Match match = Regex.Match(Name, @"(.+)\.\(\s*(\d+)\s*\)");
            if (match.Success && int.TryParse(match.Groups[2].Value, out int res))
            {
                Name = match.Groups[1].Value;
                ID = res;
            }
            else
                ID = id;
        }

        public void AddBodyOperations(XmlDocument document)
        {
            Operations.Clear();
            foreach (XmlNode xm in document.SelectNodes(@"//param[@name='operation']/@value"))
            {
                if (!Operations.Any(p => p.Equals(xm.InnerText)))
                    Operations.Add(xm.InnerText);
            }
        }
    }


}