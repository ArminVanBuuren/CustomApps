namespace SPAFilter.SPA.Components.ROBP
{
    public class ROBPOperation : Operation
    {
        public ROBPOperation(string path, int id, ObjectTemplate parentElement) : base(path, id)
        {
            if (GetNameWithId(Name, out var newName, out var newId))
            {
                Name = newName;
                ID = newId;
            }

            HostTypeName = parentElement.Name;
        }
    }
}
