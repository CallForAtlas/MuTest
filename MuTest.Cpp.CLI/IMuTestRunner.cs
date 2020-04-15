﻿using System.Threading.Tasks;
using MuTest.Cpp.CLI.Options;

namespace MuTest.Cpp.CLI
{
    public interface IMuTestRunner
    {
        Task RunMutationTest(MuTestOptions options);
    }
}