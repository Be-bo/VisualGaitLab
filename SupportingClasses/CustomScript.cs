using System;

namespace VisualGaitLab.SupportingClasses
{
    [Serializable]
    class CustomScript
    {
        public CustomScript(string path)
        {
            string[] temp = path.Split('\\');
            Name = temp[temp.Length - 1].Replace(".py", "");
            Path = path;
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public override bool Equals(object obj)
        {
            return (obj as string) == Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}
