using ResearchPlatform.Models;
using ResearchPlatform.Models.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ResearchPlatform.Input
{
    public static class Fetcher
    {
        static readonly string NOMINATIM_SEARCH_BASE_URL = "https://nominatim.openstreetmap.org/search?";
        static readonly string OVERPASS_API_BASE_URL = "https://overpass-api.de/api/interpreter?data=[out:json];";
        static readonly string GRAPHOPPER_API_BASE_URL = "https://graphhopper.com/api/1/matrix?";
        static readonly string GOOGLE_API_BASE_URL = "https://maps.googleapis.com/maps/api/distancematrix/json?";
        static readonly string GOOGLE_API_KEY = "AIzaSyB3evCs9ya8p1Q29BDYK-JrQyO6m4P455M";
        static readonly string GRAPHOPPER_API_KEY = "9e0379a5-ad09-4284-883a-9834508b11ce";
        static readonly string GRAPHOPPER_API_KEY2 = "4da363e9-748a-4e52-b76b-89bc288eddc2";
        static readonly string GRAPHOPPER_API_KEY3 = "6577c1b9-9ff4-42a2-b2ab-e44eaed579f4";

        static HttpClient _client = new HttpClient();
        public static async Task<Node> FetchCityNodeFromPostcodeAsync(string postcode)
        {
            List<NodeDTO> nodes = null;
            HttpResponseMessage response = await _client.GetAsync($"{NOMINATIM_SEARCH_BASE_URL}postalcode={postcode}&country=Polska&format=json&email=238041@student.pwr.edu.pl");
            if (response.IsSuccessStatusCode)
            {
                var jsonNode = await response.Content.ReadAsStringAsync();
                nodes = JsonSerializer.Deserialize<List<NodeDTO>>(jsonNode);
            }

            return nodes.Count == 1 ? Node.CreateFromDTO(nodes[0]) : null;
        }

        public static async Task<List<Node>> FetchCitiesNodesAroundNodeAsync(Node center, int radius = 135)
        {
            OverpassApiResult result = null;
            HttpResponseMessage response = await _client.GetAsync($"{OVERPASS_API_BASE_URL}node(around:{radius * 1000},{center.Latitude},{center.Longitude})[place~'(city|town)$'];out geom;");
            if (response.IsSuccessStatusCode)
            {
                var jsonNode = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<OverpassApiResult>(jsonNode);
            }

            return result == null && result.elements.Count == 0 ? null : Node.CreateFromOverpassDTO(result.elements).Take(60).ToList();
        }

        public static async Task<Distance> FetchDistanceBetweenNodesAsync(Node from, Node to)
        {
            DistanceDTO distance = null;
            HttpResponseMessage response = await _client.GetAsync($"{GRAPHOPPER_API_BASE_URL}point={from.Latitude},{from.Longitude}&point={to.Latitude},{to.Longitude}&type=json&vehicle=car&out_array=weights&out_array=times&out_array=distances&key={GRAPHOPPER_API_KEY3}");
            if (response.IsSuccessStatusCode)
            {
                var jsonDistance = await response.Content.ReadAsStringAsync();
                distance = JsonSerializer.Deserialize<DistanceDTO>(jsonDistance);
            }

            return distance != null ? Distance.CreateFromDTO(distance, from, to) : null;
        }
    }
}
