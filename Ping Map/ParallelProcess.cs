using System.Collections;
using static Main.Program;

namespace ParallellProcess
{
    public class Processor
    {
        public static ArrayList Scan(ArrayList IPlist)
        {

            var output = false;
            ArrayList working = new ArrayList();

            foreach (String IP in IPlist)
            {
                output = Ping(IP);

                if (output)
                {
                    working.Add(IP);
                }

                float completion = (IPlist.IndexOf(IP) * 100) / IPlist.Count;
                Console.WriteLine($"{IP}: {output} | Completion: {completion}%");
            }

            return working;

        }
    }
}
