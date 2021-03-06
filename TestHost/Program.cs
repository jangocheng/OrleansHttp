﻿using System;
using System.Diagnostics;
using System.Net;
using Orleans;
using Orleans.Runtime.Configuration;
using TestGrains;
using BenchmarkDotNet.Running;

namespace TestHost
{
    class DevSilo : IDisposable
    {
        private static OrleansHostWrapper hostWrapper;

        public DevSilo()
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = new string[0],
            });
        }

        static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper();

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        public void Dispose()
        {
            if (hostWrapper == null) return;
            hostWrapper.Dispose();
            GC.SuppressFinalize(hostWrapper);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            using (var silo = new DevSilo())
            {
                RunAllTests();

                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
            }
        }

        private static void RunAllTests()
        {
            var summary = BenchmarkRunner.Run<Benchmarks>();
            Console.WriteLine(summary);
        }



    }
}
