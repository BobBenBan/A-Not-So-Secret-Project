using System;

namespace MusicMachine.Util
{
public static class Dirs
{
    public const string RootNamespace = "MusicMachine";

    public static string GetClassDirOf<T>() => DirHolder<T>.Instance.ClassDir;

    public static string GetFolderDirOf<T>() => DirHolder<T>.Instance.FolderDir;

    private class DirHolder<T>
    {
        public static readonly DirHolder<T> Instance = new DirHolder<T>();
        public readonly string ClassDir;
        public readonly string FolderDir;

        private DirHolder()
        {
            var typeNamespace = typeof(T).Namespace;
            var name          = typeof(T).Name;
            if (typeNamespace == null)
                return;
            var index = typeNamespace.IndexOf(RootNamespace, StringComparison.Ordinal);
            FolderDir = "res://"
                      + (index < 0 ? typeNamespace : typeNamespace.Remove(index, RootNamespace.Length)).Replace(
                            '.',
                            '/');
            ClassDir = $"{FolderDir}/{name}.cs";
        }
    }
}
}