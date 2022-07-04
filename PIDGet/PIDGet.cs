using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommandLine;
using CommandLine.Text;

namespace PIDGet
{
    public class PIDGet
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        public class Options
        {

            [Value(0, MetaName = "ImageName", Required = false)]
            public string? ImageName { get; set; }

            [Option("process_name_duplicate_action")]
            public NameDuplicateAction NameDuplicateOption { get; set; }
            [Option("process_name_duplicate_err_action")]
            public NameDuplicateErrorAction NameDuplicateErrorOption { get; set; }

            [Option('m', "matchcase", Required = false, Default = false)]
            public bool MatchCase { get; set; }

            [Option('h', "help", Required = false)]
            public string? help_target { get; set; }

        }

        [Flags]
        public enum NameDuplicateAction
        {
            SELECT_FIRST,
            SELECT_LAST,
            SELECT_ALL,
            SELECT_NONE
        }
        [Flags]
        public enum NameDuplicateErrorAction
        {
            IGNORE,
            WARN,
            ERROR
        }

        static void Main(string[] args)
        {
            var result = new Parser(with => with.HelpWriter = null).ParseArguments<Options>(args);

            result
            .WithParsed(o =>
            {
                if (o.help_target != null)
                    Help(o.help_target);
                else if (o.ImageName != null)
                    Console.WriteLine(Get(o));
                else
                    DisplayHelp(result, new List<Error>() { });
            })
            .WithNotParsed(errs => DisplayHelp(result, errs));
        }

        public static void Help(string cmd)
        {
            Console.WriteLine(cmd);
        }
        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }

        public static int Get(Options o)
        {
            string? errors;
            var matched_list = new List<int>();
            var all_process = Process.GetProcesses();
            int result = -1;
            foreach (var process in all_process)
            {
                if (o.MatchCase ? process.ProcessName == o.ImageName :
                    process.ProcessName.ToLower() == o.ImageName!.ToLower())
                {
                    if (o.NameDuplicateOption == NameDuplicateAction.SELECT_FIRST)
                    {
                        result = process.Id;
                        break;
                    }
                    else
                        matched_list.Add(process.Id);
                }
            }
            if (o.NameDuplicateOption == NameDuplicateAction.SELECT_FIRST) { }
            else if (o.NameDuplicateOption == NameDuplicateAction.SELECT_LAST)
                result = matched_list[^1];
            else if (o.NameDuplicateOption == NameDuplicateAction.SELECT_ALL)
            {
                Console.WriteLine(string.Join("\n", matched_list));
                //return ;
            }
            else if (o.NameDuplicateOption == NameDuplicateAction.SELECT_NONE)
                return -1;
            return result;
        }
    }
}
