using System.IO;
using System;
using System.Linq;
using Heroes.ReplayParser;
using Foole.Mpq;

namespace Heroes.ReplayParser.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var replayFileName = Environment.GetCommandLineArgs().LastOrDefault();
            if (replayFileName == "") {
                Console.WriteLine("You must specify a path to a replay file!");
                return;
            }

            // Use temp directory for MpqLib directory permissions requirements
            var tmpPath = Path.GetTempFileName();
            File.Copy(replayFileName, tmpPath, overwrite: true);

            try
            {
                var replayParseResult = DataParser.ParseReplay(tmpPath, ignoreErrors: false, deleteFile: false);
                if (replayParseResult.Item1 != DataParser.ReplayParseResult.Success)
                {
                    Console.WriteLine("Failed to parse replay: " + replayParseResult.Item1);
                    return;
                }

                var replay = replayParseResult.Item2;

                Console.WriteLine("Replay build: " + replay.ReplayBuild);
                Console.WriteLine("Map: " + replay.Map);
                foreach (var player in replay.Players.OrderByDescending(i => i.IsWinner))
                    Console.WriteLine("Player: " + player.Name + "#" + player.BattleTag + ", Win: " + player.IsWinner + ", Hero: " + player.Character + ", Lvl: " + player.CharacterLevel + ", Talents: " + string.Join(",", player.Talents.Select(i => i.TalentID + ":" + i.TalentName)));

            }
            finally
            {
                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);
            }
        }

        private static byte[] GetMpqArchiveFileBytes(MpqArchive archive, string fileName)
        {
            using (var mpqStream = archive.OpenFile(archive.Single(i => i.Filename == fileName)))
            {
                var buffer = new byte[mpqStream.Length];
                mpqStream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}
