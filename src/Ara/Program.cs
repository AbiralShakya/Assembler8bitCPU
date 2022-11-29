using Ara.CommandLine;

namespace Ara;

public static class Program {
    public static int Main(string[] args) {
        return AraCommandLine.ProcessArgs(args);
    }
}
