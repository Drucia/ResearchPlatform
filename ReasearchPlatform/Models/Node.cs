using ResearchPlatform.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResearchPlatform.Models
{
    public class Node : IEquatable<Node>
    {
        public long ID { get; set; }

        public double Latitude { get; set; }
        
        public double Longitude { get; set; }

        public string Name { get; set; }

        public static Node CreateFromDTO(NodeDTO nodeDTO)
        {
            return new Node() { 
                ID = nodeDTO.place_id, 
                Latitude = Double.Parse(nodeDTO.lat), 
                Longitude = Double.Parse(nodeDTO.lon), 
                Name = nodeDTO.display_name.Split(",")[0]
            };
        }

        public static List<Node> CreateFromOverpassDTO(List<Element> elements)
        {
            return elements
                        .Select(element => new Node() { 
                            ID = element.id,
                            Latitude = element.lat,
                            Longitude = element.lon,
                            Name = element.tags.name
                        }).ToList();
        }

        public bool Equals(Node other)
        {
            return ID == other.ID;
        }

        public override string ToString() 
        {
            return Name;
        }
    }
}
