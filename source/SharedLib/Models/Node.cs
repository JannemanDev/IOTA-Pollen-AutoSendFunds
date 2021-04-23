using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleBase;

namespace SharedLib.Models
{
    public class Node : Entity
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string Id => Url;

        public string Url { get; set; }
        
        [JsonConstructor]
        public Node(string url)
        {
            Url = url;
        }

        public override string ToString()
        {
            return $"Node: {Url}";
        }
        
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                Node otherNode = (Node)obj;
                return (Url == otherNode.Url); //nodes are equal when their url is the same
            }
        }

        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }
    }
}
