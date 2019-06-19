using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionCommands : ObjectCollection<Command>
    {
        internal CollectionCommands()
        {

        }

        public CollectionCommands(string dirPath)
        {
            var files = GetConfigFiles(dirPath);

            int i = 0;
            foreach (string command in files)
            {
                Add(new Command(command, ++i));
            }
        }

        public CollectionCommands Clone()
        {
            var clone = new CollectionCommands();
            clone.AddRange(this);
            return clone;
        }
    }
}
