using System;
using System.IO;
using System.Text;

namespace EasyGarry
{
    /// <summary>
    /// Single source of truth for the two priming files the interview session reads.
    /// Both the setup screen and <see cref="Interview"/> go through here so the paths
    /// can never drift apart.
    /// </summary>
    internal static class InterviewInputs
    {
        public const string ResumeFileName = "resume.txt";
        public const string JobDescriptionFileName = "jobdescription.txt";

        public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

        public static string ResumePath => Path.Combine(BaseDirectory, ResumeFileName);

        public static string JobDescriptionPath => Path.Combine(BaseDirectory, JobDescriptionFileName);

        /// <summary>
        /// Returns the file's content, or an empty string when it is missing or unreadable.
        /// Never returns an error message as content - callers treat empty as "not provided".
        /// </summary>
        public static string ReadOrEmpty(string filePath)
        {
            try
            {
                return File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes UTF-8 without a BOM so the content round-trips through File.ReadAllText.
        /// </summary>
        public static void Write(string filePath, string content)
        {
            File.WriteAllText(filePath, content ?? string.Empty, new UTF8Encoding(false));
        }
    }
}
