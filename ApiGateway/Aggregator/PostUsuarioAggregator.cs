using ApiGateway.Dto;
using BrotliSharpLib;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.Configuration;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using Ocelot.RequestId;
using System.Net;
using System.Net.Http.Headers;

namespace ApiGateway.Aggregator
{
    public class PostUsuarioAggregator : IDefinedAggregator
    {    
        public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
        {
            List<PostDto> articuloPosts = new List<PostDto>();
            List<UsuarioDto> users = new List<UsuarioDto>();

            foreach (var response in responses)
            {
                string downStreamRouteKey = ((DownstreamRoute)response.Items["DownstreamRoute"]).Key;
                DownstreamResponse downstreamResponse = (DownstreamResponse)response.Items["DownstreamResponse"];
                byte[] downstreamResponseContent = await downstreamResponse.Content.ReadAsByteArrayAsync();

                if (downStreamRouteKey == "posts")
                {
                    articuloPosts = JsonConvert.DeserializeObject<List<PostDto>>(DeCompressBrotli(downstreamResponseContent));
                }

                if (downStreamRouteKey == "usuarios")
                {
                    Console.WriteLine("Request: {0}", "Ingresando a Usuarios");
                    users = JsonConvert.DeserializeObject<List<UsuarioDto>>(DeCompressBrotli(downstreamResponseContent));
                    Console.WriteLine("Request: {0}", users.ToString());

                }
            }

            return PostByUsername(articuloPosts, users);

        }
        public DownstreamResponse PostByUsername(List<PostDto> articuloPosts, List<UsuarioDto> users)
        {

            var arrayUsers = new JArray();            
            var contador = 0;
            
            foreach (var usuario in users)
            {
                Console.WriteLine(usuario.id);
                Console.WriteLine(usuario.name);
                Console.WriteLine(usuario.username);
                Console.WriteLine(usuario.email);
                var postusers = new JArray();
                var userId = usuario.id;
                foreach (var post in articuloPosts)
                {

                    Console.WriteLine(post.id);
                    Console.WriteLine(post.title);
                    Console.WriteLine(post.body);
                    if (post.userId == userId)
                    {
                        var ObjPost = new JObject(
                                       new JProperty("id", post.id),
                                       new JProperty("title", post.title),
                                       new JProperty("body", post.body));
                        postusers.Add(ObjPost);
                    }


                }
                var ObjUser = new JObject(
                               new JProperty("id", usuario.id),
                               new JProperty("name", usuario.name),
                               new JProperty("username", usuario.username),
                               new JProperty("email", usuario.email),
                               new JProperty("post", postusers));

                
                arrayUsers.Add(ObjUser);
                contador++; 
                Console.WriteLine(contador + " -------");
            }            

            var objectPostsUsersString = JsonConvert.SerializeObject(arrayUsers);

            var stringContent = new StringContent(objectPostsUsersString)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            return new DownstreamResponse(stringContent, HttpStatusCode.OK, new List<KeyValuePair<string, IEnumerable<string>>>(), "OK");
        }
        private string DeCompressBrotli(byte[] xResponseContent)
        {
            return System.Text.Encoding.UTF8.GetString(Brotli.DecompressBuffer(xResponseContent, 0, xResponseContent.Length, null));
        }
    }
}
