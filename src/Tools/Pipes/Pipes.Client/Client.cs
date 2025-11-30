using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Pipes
{
    public class Client
    {
        static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        readonly BlockingCollection<string> queue = new BlockingCollection<string>();

        readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        NamedPipeClientStream client;
        StreamReader rs;
        StreamWriter ws;

        readonly byte[] TR = Encoding.UTF8.GetBytes(Environment.NewLine);
        readonly Action<string> read;
        
        string pipe;

        ILogger<Client> logs;


        public Client(string pipe, Action<string> read, ILogger<Client> logs)
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));

            if (read == null)
                throw new ArgumentNullException(nameof(read));

            this.read = read;
            this.pipe = pipe;
            this.logs = logs;
        }


        public void Start()
        {
            client = new NamedPipeClientStream(".", pipe,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            Task.Run(() =>
            {
                client.Connect();

                Task.Run(async () =>
                {
                    var rs = new StreamReader(client);

                    while (!cancellation.IsCancellationRequested)
                        try
                        {
                            if (!client.IsConnected)
                                client.Connect();

                            var data = await rs.ReadLineAsync();
                            if (data == null)
                                continue;
                            
                            read(data);
                        }
                        catch (Exception e)
                        {
                            logs.LogError(e, "Filed to read pipe: {failure}", e.Message);

                            await Task.Delay(1000);
                        }
                });

                Task.Run(async () =>
                {
                    while (!cancellation.IsCancellationRequested)
                        try
                        {
                            var data = queue.Take(cancellation.Token);

                            client.Write(Encoding.UTF8.GetBytes(data));
                            client.Write(TR);
                            client.Flush();
                        }
                        catch (Exception e)
                        {
                            logs.LogError(e, "Filed to write pipe: {failure}", e.Message);

                            await Task.Delay(1000);
                        }
                });
            });
        }

        public void Send(object control)
        {
            queue.Add(JsonConvert.SerializeObject(control, settings));
        }

        public void Stop()
        {
            cancellation.Cancel();
        }
    }
}
