using SPAFilter.SPA.Components;

namespace SPAFilter.SPA.Collection
{
    public class CollectionBusinessProcess : ObjectCollection<BusinessProcess>
    {
        internal CollectionBusinessProcess()
        {
            
        }

        public CollectionBusinessProcess(string dirPath)
        {
            var files = GetConfigFiles(dirPath);

            int i = 0;
            foreach (string businessProcess in files)
            {
                Add(new BusinessProcess(businessProcess, ++i));
            }
        }

        public CollectionBusinessProcess Clone()
        {
            CollectionBusinessProcess currentClone = new CollectionBusinessProcess();
            currentClone.AddRange(this);
            return currentClone;
        }
    }
}
