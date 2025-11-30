using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Pipes
{
    public class Service
    {
        static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        readonly BlockingCollection<string> queue = new BlockingCollection<string>();

        readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        NamedPipeServerStream service;
        StreamReader rs;
        StreamWriter ws;

        readonly byte[] TR = Encoding.UTF8.GetBytes(Environment.NewLine);
        readonly Action<string> read;

        string pipe;

        ILogger<Service> logs;


        public Service(string pipe, Action<string> read, ILogger<Service> logs)
        {
            if (pipe == null)
                throw new ArgumentNullException(nameof(pipe));

            if (read == null)
                throw new ArgumentNullException(nameof(read));

            this.read = read;
            this.pipe = pipe;
            this.logs = logs;
        }

        public Service(string pipe, Action<string> read)
        {

        }

        public void Start()
        {
            service = new NamedPipeServerStream(pipe, PipeDirection.InOut, 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            Task.Run(() =>
            {
                service.WaitForConnection();

                Task.Run(async () =>
                {
                    var rs = new StreamReader(service);

                    while (!cancellation.IsCancellationRequested)
                        try
                        {
                            if (!service.IsConnected)
                            {
                                service.Disconnect();
                                service.WaitForConnection();
                            }

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

                            if (service.IsConnected)
                            {
                                var bytes = Encoding.UTF8.GetBytes(data);

                                service.Write(bytes, 0, bytes.Length);
                                service.Write(TR, 0, TR.Length);
                                service.Flush();
                            }
                        }
                        catch (Exception e)
                        {
                            logs.LogError(e, "Filed to write pipe: {failure}", e.Message);
                            
                            await Task.Delay(1000);
                        }
                });
            });
        }

        public void Send(object input)
        {
            Send(JsonConvert.SerializeObject(input, settings));
        }

        public void Send(string data, bool resilient = true)
        {
            if (false == resilient &&
                (null == service || !service.IsConnected))
                return;
            
            queue.Add(data);
        }

        public void Stop()
        {
            cancellation.Cancel();
        }
    }
}
