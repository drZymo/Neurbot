using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Neurbot.Generic
{
    public abstract class Engine<GameStateTemplate> where GameStateTemplate : class
    {
        private readonly Thread readThread;
        private readonly List<GameStateTemplate> gameStates = new List<GameStateTemplate>();

        public Engine()
        {
            Win32IO.Init();
            readThread = new Thread(new ThreadStart(PollState));
        }

        public void Run()
        {
            try
            {
                DoRun();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Run failure: " + ex.Message);
            }
        }

        private void DoRun()
        {
            mStop = false;
            readThread.Start();
            while (!mStop)
            {
                var states = GameStates();
                if (states.Count > 0)
                {
                    Response(states);
                }
                Thread.Sleep(10);
            }
        }

        private void PollState()
        {
            try
            {
                DoPollState();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Poll state failure: " + ex.Message);
            }
        }

        private void DoPollState()
        {
            while (!mStop)
            {
                var state = ReadMessage<GameStateTemplate>();
                if (state == null)
                    continue;
                lock (gameStates)
                {
                    gameStates.Add(state);
                }
            }
        }

        private List<GameStateTemplate> GameStates()
        {
            lock (gameStates)
            {
                var states = gameStates.ToList();
                gameStates.Clear();
                return states;
            }
        }

        public abstract void Response(List<GameStateTemplate> gameStates);

        private bool mStop;
        public void Stop()
        {
            mStop = true;
            readThread.Join();
        }

        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        private T ReadMessage<T>()
        {
            string line = Console.ReadLine();
            if (String.IsNullOrEmpty(line))
                return default(T);

            try
            {
                return JsonConvert.DeserializeObject<T>(line, jsonSerializerSettings);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public void WriteMessage<T>(T message)
        {
            Console.WriteLine(JsonConvert.SerializeObject(message, Formatting.None, jsonSerializerSettings));
        }
    }
}
