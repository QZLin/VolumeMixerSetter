using CommandLine;
using System;
using VolumeMixerSetter;
using static PIDGet.PIDGet;

static class Program
{
    //[DllImport("user32.dll")]
    //public static extern IntPtr FindWindow(string strClassName, string strWindowName);

    //[DllImport("user32.dll", SetLastError = true)]
    //public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    public class BaseOptions
    {
        [Value(0, MetaName = "PID", Required = false, HelpText = "PID of Application")]
        public string? Pid { get; set; }

        [Option('m', "mute", Default = false, HelpText = "Specify mute mode")]
        public bool Mute { get; set; }

        [Option('v', "volume", Default = false, HelpText = "Specify get mode")]
        public bool Volume { get; set; }

        [Option('n', "process_name", Required = false, HelpText = "Use process name instead of pid")]
        public string? ProcessName { get; set; }

        [Option('z', "process_name_duplicate_action")]
        public NameDuplicateAction NameDuplicateOption { get; set; }
        [Option('y', "process_name_duplicate_err_action")]
        public NameDuplicateErrorAction NameDuplicateErrorOption { get; set; }

        [Option("verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }
    [Verb("get")]
    public class Get : BaseOptions
    {

    }

    [Verb("set")]
    public class Set : BaseOptions
    {
        [Value(1, MetaName = "value", Required = true, HelpText = "[0-100] for volume, 0=true and 1=false for mute")]
        public int Value { get; set; }
    }

    static int Main(string[] args)
    {
        int pid = -1;
        var exit_code = 0;
        var result = Parser.Default.ParseArguments<Get, Set>(args).WithParsed<BaseOptions>(o =>
        {
            if (o.ProcessName == null)
            {
                exit_code = 1;
                pid = Convert.ToInt32(o.Pid!);
                exit_code = 0;
            }
            else
            {
                var pid_option = new Options
                {
                    ImageName = o.ProcessName,
                    NameDuplicateOption = o.NameDuplicateOption,
                    NameDuplicateErrorOption = o.NameDuplicateErrorOption
                };
                pid = PIDGet.PIDGet.Get(pid_option);
                if (pid == -1)
                    exit_code = 1;
            }
            if (!o.Mute & !o.Volume)
            {
                Console.Error.WriteLine("must select an action [mute/volume]");
                exit_code = 1;
            }
        })
        .WithParsed<Get>(o =>
        {

            if (o.Mute)
            {
                var mute = VolumeMixer.GetApplicationMute(pid);
                Func<bool?, int>? f = (mute is null)
                ? s => { Console.Error.WriteLine(s); return 1; }
                : s => { Console.WriteLine(s); return 0; };
                exit_code = f(mute);
            }
            else if (o.Volume)
            {
                var volume = VolumeMixer.GetApplicationVolume(pid);
                Func<float?, int>? f = (volume is null)
                ? s => { Console.Error.WriteLine(s); return 1; }
                : s => { Console.WriteLine(s); return 0; };
                exit_code = f(volume);
            }
        })
        .WithParsed<Set>(o =>
        {
            if (o.Mute)
            {
                VolumeMixer.SetApplicationMute(pid, o.Value == 1);
            }
            else if (o.Volume)
            {
                VolumeMixer.SetApplicationVolume(pid, o.Value);
            }
        });
        return exit_code;
    }
}